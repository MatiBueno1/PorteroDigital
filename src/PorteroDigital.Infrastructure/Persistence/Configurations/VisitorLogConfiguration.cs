using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PorteroDigital.Domain.Entities;

namespace PorteroDigital.Infrastructure.Persistence.Configurations;

public sealed class VisitorLogConfiguration : IEntityTypeConfiguration<VisitorLog>
{
    public void Configure(EntityTypeBuilder<VisitorLog> builder)
    {
        builder.ToTable("VisitorLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VisitorName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.RequestedAtUtc)
            .IsRequired();

        builder.Property(x => x.ResidentDecision)
            .HasMaxLength(250);

        builder.HasIndex(x => new { x.HouseId, x.RequestedAtUtc });
    }
}
