using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PorteroDigital.Infrastructure;
using PorteroDigital.Infrastructure.Security;
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

app.Run();
