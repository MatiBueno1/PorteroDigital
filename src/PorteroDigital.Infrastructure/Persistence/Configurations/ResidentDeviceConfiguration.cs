using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PorteroDigital.Domain.Entities;

namespace PorteroDigital.Infrastructure.Persistence.Configurations;

public sealed class ResidentDeviceConfiguration : IEntityTypeConfiguration<ResidentDevice>
{
    public void Configure(EntityTypeBuilder<ResidentDevice> builder)
    {
        builder.ToTable("ResidentDevices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Platform)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PushToken)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.NotificationSound)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.LastSeenAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.ResidentId, x.PushToken })
            .IsUnique();
    }
}
