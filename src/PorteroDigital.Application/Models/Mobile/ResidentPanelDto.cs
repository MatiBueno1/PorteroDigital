namespace PorteroDigital.Application.Models.Mobile;

public sealed record ResidentPanelDto(
    Guid ResidentId,
    string ResidentName,
    Guid HouseId,
    string HouseIdentifier,
    string HouseLabel,
    bool CameraEnabled,
    string? CameraStreamUrl,
    string? PublicContactNumbers,
    bool ShowContactNumbers,
    IReadOnlyList<ResidentVisitHistoryItemDto> RecentVisits);
