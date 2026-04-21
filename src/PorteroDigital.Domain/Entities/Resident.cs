namespace PorteroDigital.Domain.Entities;

public sealed class Resident
{
    public Guid Id { get; set; }
    public Guid HouseId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAtUtc { get; set; }
    public House House { get; set; } = null!;
    public ICollection<ResidentDevice> Devices { get; set; } = new List<ResidentDevice>();
}
