namespace PorteroDigital.Application.Models.Houses;

public sealed record QrValidationResult(
    Guid HouseId,
    string HouseIdentifier,
    string AddressLabel,
    bool CameraEnabled,
    string? CameraStreamUrl);
