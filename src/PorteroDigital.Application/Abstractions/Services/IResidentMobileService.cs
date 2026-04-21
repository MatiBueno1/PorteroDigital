using PorteroDigital.Application.Models.Mobile;

namespace PorteroDigital.Application.Abstractions.Services;

public interface IResidentMobileService
{
    Task<ResidentPanelDto?> GetPanelAsync(Guid residentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ResidentVisitHistoryItemDto>> GetHistoryAsync(Guid residentId, CancellationToken cancellationToken);
    Task<ResidentVisitHistoryItemDto?> DecideVisitAsync(Guid residentId, Guid visitorLogId, DecideVisitRequest request, CancellationToken cancellationToken);
    Task RegisterDeviceAsync(Guid residentId, RegisterDeviceRequest request, CancellationToken cancellationToken);
    Task<string?> UpdateNameAsync(Guid residentId, string newName, CancellationToken cancellationToken);
}
