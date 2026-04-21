using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PorteroDigital.Domain.Entities;

namespace PorteroDigital.Infrastructure.Persistence.Configurations;

public sealed class CameraConfigurationEntityConfiguration : IEntityTypeConfiguration<CameraConfiguration>
{
    public void Configure(EntityTypeBuilder<CameraConfiguration> builder)
    {
        builder.ToTable("CameraConfigurations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.StreamUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.SnapshotUrl)
            .HasMaxLength(500);

        builder.Property(x => x.AccessToken)
            .HasMaxLength(500);

        builder.HasIndex(x => x.HouseId)
            .IsUnique();
    }
}
