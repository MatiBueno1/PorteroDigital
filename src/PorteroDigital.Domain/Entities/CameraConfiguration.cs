namespace PorteroDigital.Domain.Entities;

public sealed class CameraConfiguration
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public string? SnapshotUrl { get; set; }
    public string? AccessToken { get; set; }
    public bool IsEnabled { get; set; } = true;
    public House House { get; set; } = null!;
}
