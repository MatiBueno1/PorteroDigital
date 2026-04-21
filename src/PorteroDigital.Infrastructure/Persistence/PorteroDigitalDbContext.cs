using Microsoft.EntityFrameworkCore;
using PorteroDigital.Application.Abstractions.Persistence;
using PorteroDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PorteroDigital.Infrastructure.Security;

namespace PorteroDigital.Infrastructure.Persistence;

public sealed class PorteroDigitalDbContext(DbContextOptions<PorteroDigitalDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<House> Houses => Set<House>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<ResidentDevice> ResidentDevices => Set<ResidentDevice>();
    public DbSet<VisitorLog> VisitorLogs => Set<VisitorLog>();
    public DbSet<CameraConfiguration> CameraConfigurations => Set<CameraConfiguration>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PorteroDigitalDbContext).Assembly);
        
        for (int i = 1; i <= 12; i++)
        {
            var houseId = new Guid($"00000000-0000-0000-0000-0000000000{i:D2}");
            var residentId = new Guid($"10000000-0000-0000-0000-0000000000{i:D2}");

            modelBuilder.Entity<House>().HasData(new House
            {
                Id = houseId,
                Identifier = $"Casa {i}",
                AddressLabel = $"Pasillo Unidad {i}",
                QrToken = $"TOKEN-CASA-{i:D2}",
                IsActive = true,
                CreatedAtUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
            });

            modelBuilder.Entity<Resident>().HasData(new Resident
            {
                Id = residentId,
                HouseId = houseId,
                FullName = $"Inquilino Casa {i}",
                Email = $"casa{i:D2}@portero.local",
                PhoneNumber = $"341000{i:D4}",
                PasswordHash = ResidentPasswordHasher.HashSeed("Portero123!", residentId),
                IsPrimaryContact = true,
                IsActive = true,
                CreatedAtUtc = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
            });
        }

        base.OnModelCreating(modelBuilder);
    }
}
