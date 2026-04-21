namespace PorteroDigital.Domain.Entities;

public sealed class House
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string AddressLabel { get; set; } = string.Empty;
    public string QrToken { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public CameraConfiguration? CameraConfiguration { get; set; }
    public ICollection<Resident> Residents { get; set; } = new List<Resident>();
    public ICollection<VisitorLog> VisitorLogs { get; set; } = new List<VisitorLog>();
}