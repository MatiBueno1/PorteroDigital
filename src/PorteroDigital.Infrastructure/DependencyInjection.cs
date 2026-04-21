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
                var postgresConnectionString = configuration.GetConnectionString("Postgres") ?? configuration["DATABASE_URL"];
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
}
