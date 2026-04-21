using PorteroDigital.Domain.Enums;

namespace PorteroDigital.Domain.Entities;

public sealed class VisitorLog
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public VisitorLogStatus Status { get; set; } = VisitorLogStatus.Pending;
    public DateTimeOffset RequestedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RespondedAtUtc { get; set; }
    public string? ResidentDecision { get; set; }
    public House House { get; set; } = null!;
}