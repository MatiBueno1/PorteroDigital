using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using PorteroDigital.Infrastructure;
using PorteroDigital.Infrastructure.Persistence;
using PorteroDigital.Infrastructure.Security;
using PorteroDigital.Domain.Entities;
using PorteroDigital.WebAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Cargar .env manualmente (mapeando a estructura de .NET)
var dotenv = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(dotenv)) dotenv = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "", ".env");
if (!File.Exists(dotenv)) dotenv = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? "", ".env");

if (File.Exists(dotenv))
{
    foreach (var line in File.ReadAllLines(dotenv))
    {
        var parts = line.Split('=', 2);
        if (parts.Length != 2) continue;
        var key = parts[0].Trim();
        var value = parts[1].Trim().Trim('"');
        
        // Mapeo manual de variables de entorno a configuración de .NET
        var netKey = key switch {
            "CAMERA_RTSP_URL" => "Camera:RtspUrl",
            "CAMERA_LIGHT_ON_URL" => "Camera:LightOnUrl",
            "CAMERA_LIGHT_OFF_URL" => "Camera:LightOffUrl",
            "CAMERA_NIGHT_VISION_ON_URL" => "Camera:NightVisionOnUrl",
            "CAMERA_NIGHT_VISION_OFF_URL" => "Camera:NightVisionOffUrl",
            "CAMERA_NIGHT_VISION_AUTO_URL" => "Camera:NightVisionAutoUrl",
            "JWT_SIGNING_KEY" => "Jwt:SigningKey",
            _ => key.Replace("__", ":")
        };
        builder.Configuration[netKey] = value;
    }
}

// Configurar el puerto para la nube
var port = Environment.GetEnvironmentVariable("PORT") ?? "5166";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // AllowAnyOrigin() cubre http/https normales.
        // SetIsOriginAllowed con "null" cubre archivos locales (file://)
        // que el navegador envía con Origin: null.
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true || origin == "null");
    });

    // Política separada para SignalR que sí necesita credenciales/websockets
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// UseHttpsRedirection desactivado en dev: el frontend local (file://) usa http y no puede seguir redirects.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ResidentNotificationsHub>("/hubs/resident-notifications").RequireCors("SignalRPolicy");

// Migraciones automáticas (Para que funcione en Neon desde el inicio)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PorteroDigitalDbContext>();
        var connString = dbContext.Database.GetConnectionString();
        logger.LogInformation("🗄️  Conectando a DB. Provider: {Provider} | ConnectionString (inicio): {Conn}",
            dbContext.Database.ProviderName,
            connString?[..Math.Min(60, connString.Length)] + "...");

        // EnsureCreated falla en proveedores de nube como Neon porque detecta
        // tablas nativas del sistema, asume que la DB ya está creada y no hace nada.
        // Solución robusta: intentamos consultar nuestra tabla, si falla, forzamos creación.
        try
        {
            dbContext.Houses.Any();
            logger.LogInformation("✅ Base de datos ya estaba inicializada.");
        }
        catch
        {
            var dbCreator = dbContext.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IDatabaseCreator>() as Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator;
            if (dbCreator != null)
            {
                dbCreator.CreateTables();
                logger.LogInformation("✅ Tablas creadas forzadamente de cero.");

                // CreateTables NO ejecuta el HasData (seed). Lo hacemos manualmente si está vacío:
                if (!dbContext.Houses.Any())
                {
                    logger.LogInformation("🌱 Insertando datos semilla (Houses & Residents)...");
                    for (int i = 1; i <= 12; i++)
                    {
                        var houseId = new Guid($"00000000-0000-0000-0000-0000000000{i:D2}");
                        var residentId = new Guid($"10000000-0000-0000-0000-0000000000{i:D2}");
                        
                        dbContext.Houses.Add(new House
                        {
                            Id = houseId,
                            Identifier = $"Casa {i}",
                            AddressLabel = $"Pasillo Unidad {i}",
                            QrToken = $"TOKEN-CASA-{i:D2}",
                            IsActive = true,
                            CreatedAtUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
                        });

                        dbContext.Residents.Add(new Resident
                        {
                            Id = residentId,
                            HouseId = houseId,
                            FullName = $"Inquilino Casa {i}",
                            Email = $"casa{i:D2}@portero.local",
                            PhoneNumber = $"341000{i:D4}",
                            PasswordHash = PorteroDigital.Infrastructure.Security.ResidentPasswordHasher.HashSeed("Portero123!", residentId),
                            IsPrimaryContact = true,
                            IsActive = true,
                            CreatedAtUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
                        });
                    }
                    dbContext.SaveChanges();
                    logger.LogInformation("✅ Datos semilla insertados correctamente.");
                }
            }
        }

        // Intento de auto-reparación segura para las nuevas columnas de la feature actual:
        try
        {
            dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Houses\" ADD COLUMN \"PublicContactNumbers\" character varying(150) NULL;");
            dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Houses\" ADD COLUMN \"ShowContactNumbers\" boolean NOT NULL DEFAULT FALSE;");
            logger.LogInformation("✅ Columnas nuevas de contacto añadidas vía auto-repair.");
        }
        catch { /* Ignorar si las columnas ya existen o falló (ej: en SQLite usa sintaxis TEXT) */ }
        
        try
        {
            dbContext.Database.ExecuteSqlRaw("ALTER TABLE Houses ADD COLUMN PublicContactNumbers TEXT NULL;");
            dbContext.Database.ExecuteSqlRaw("ALTER TABLE Houses ADD COLUMN ShowContactNumbers INTEGER NOT NULL DEFAULT 0;");
        }
        catch { /* Resonancia para SQLite */ }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "❌ Error crítico en DB. El app arrancó igual. Revisar connection string.");
        // No se hace throw: el app arranca para que Render lo vea como 'live'
        // y los logs muestren el error real.
    }
}


app.MapGet("/", () => "Portero Digital API is running! 🚀");
app.MapGet("/healthz", () => Results.Ok("Healthy"));
app.MapGet("/test-db", async (PorteroDigitalDbContext db, CancellationToken ct) => 
{
    try 
    {
        var connStr = db.Database.GetConnectionString();
        var canConnect = await db.Database.CanConnectAsync(ct);
        
        var tables = new List<string>();
        using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
            await db.Database.OpenConnectionAsync(ct);
            using var result = await command.ExecuteReaderAsync(ct);
            while (await result.ReadAsync(ct))
            {
                tables.Add(result.GetString(0));
            }
        }
        
        return Results.Ok(new { 
            CanConnect = canConnect, 
            Provider = db.Database.ProviderName, 
            Tables = tables,
            ConnectionStringStart = connStr?[..Math.Min(60, connStr.Length)] + "..."
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.ToString(), title: "Database Connection Failure");
    }
});

app.Run();
