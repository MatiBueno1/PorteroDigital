namespace PorteroDigital.Application.Models.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid ResidentId,
    Guid HouseId,
    string ResidentName,
    string HouseLabel);
