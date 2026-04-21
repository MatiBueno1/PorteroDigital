using PorteroDigital.Domain.Enums;

namespace PorteroDigital.Domain.Entities;

public sealed class ResidentDevice
{
    public Guid Id { get; set; }
    public Guid ResidentId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public PushPlatform Platform { get; set; }
    public string PushToken { get; set; } = string.Empty;
    public string NotificationSound { get; set; } = "default";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public Resident Resident { get; set; } = null!;
}
