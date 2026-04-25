using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PorteroDigital.Application.Abstractions.Persistence;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Infrastructure.Notifications;
using PorteroDigital.Infrastructure.Persistence;
using PorteroDigital.Infrastructure.Security;
using PorteroDigital.Infrastructure.Services;

namespace PorteroDigital.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["Database:Provider"] ?? "Sqlite";
        var sqliteConnectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration.GetConnectionString("Sqlite");
        var sqlServerConnectionString = configuration.GetConnectionString("SqlServer");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PushNotificationOptions>(configuration.GetSection(PushNotificationOptions.SectionName));

        services.AddDbContext<PorteroDigitalDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(sqlServerConnectionString))
            {
                options.UseSqlServer(sqlServerConnectionString);
                return;
            }

            if (string.Equals(databaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
            {
                var rawUrl = configuration.GetConnectionString("Postgres") ?? configuration["DATABASE_URL"];
                // Convertir URI postgresql:// a key=value string compatible con Npgsql.
                // DATABASE_URL de Neon/Render incluye params como ?channel_binding=prefer
                // que Npgsql rechaza con KeyNotFoundException. Parseamos manualmente.
                var postgresConnectionString = ConvertPostgresUriToConnectionString(rawUrl);
                options.UseNpgsql(postgresConnectionString);
                return;
            }

            options.UseSqlite(sqliteConnectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<PorteroDigitalDbContext>());
        services.AddScoped<IHouseQrValidator, HouseQrValidator>();
        services.AddScoped<IVisitorLogService, VisitorLogService>();
        services.AddScoped<IResidentAuthService, ResidentAuthService>();
        services.AddScoped<IResidentMobileService, ResidentMobileService>();
        services.AddHttpClient<ResidentPushNotificationService>();
        services.AddHttpClient<ICameraControlService, CameraControlService>();
        services.AddSingleton<ResidentPasswordHasher>();
        services.AddSingleton<ResidentTokenService>();

        return services;
    }

    /// <summary>
    /// Convierte una URI de PostgreSQL (postgres://user:pass@host:port/db?params)
    /// a un connection string key=value compatible con Npgsql.
    /// Descarta query params no reconocidos (ej: channel_binding de Neon)
    /// que causan KeyNotFoundException en NpgsqlConnectionStringBuilder.
    /// </summary>
    private static string? ConvertPostgresUriToConnectionString(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri)) return uri;

        // Si ya es key=value, devolverlo tal cual
        if (!uri.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !uri.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return uri;

        try
        {
            var parsed   = new Uri(uri);
            var userInfo = parsed.UserInfo.Split(':', 2);
            var host     = parsed.Host;
            var port     = parsed.Port > 0 ? parsed.Port : 5432;
            var database = parsed.AbsolutePath.TrimStart('/');
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

            // SSL requerido para Neon/Supabase/Render Postgres
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch
        {
            // Si el parsing falla, pasar la URI original y dejar que Npgsql intente
            return uri;
        }
    }
}
