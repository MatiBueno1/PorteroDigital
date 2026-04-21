using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PorteroDigital.Domain.Entities;

namespace PorteroDigital.Infrastructure.Persistence.Configurations;

public sealed class HouseConfiguration : IEntityTypeConfiguration<House>
{
    public void Configure(EntityTypeBuilder<House> builder)
    {
        builder.ToTable("Houses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Identifier)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AddressLabel)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.QrToken)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Identifier)
            .IsUnique();

        builder.HasIndex(x => x.QrToken)
            .IsUnique();

        builder.HasMany(x => x.Residents)
            .WithOne(x => x.House)
            .HasForeignKey(x => x.HouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.VisitorLogs)
            .WithOne(x => x.House)
            .HasForeignKey(x => x.HouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CameraConfiguration)
            .WithOne(x => x.House)
            .HasForeignKey<CameraConfiguration>(x => x.HouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
