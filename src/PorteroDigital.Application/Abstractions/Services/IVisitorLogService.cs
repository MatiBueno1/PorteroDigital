using PorteroDigital.Application.Models.Visitors;

namespace PorteroDigital.Application.Abstractions.Services;

public interface IVisitorLogService
{
    Task<VisitorLogDto?> CreateAsync(CreateVisitorLogRequest request, CancellationToken cancellationToken);
    Task<VisitorLogDto?> GetByIdAsync(Guid visitorLogId, CancellationToken cancellationToken);
}
