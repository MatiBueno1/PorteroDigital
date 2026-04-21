using Microsoft.EntityFrameworkCore;
using PorteroDigital.Application.Abstractions.Persistence;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Application.Models.Visitors;
using PorteroDigital.Domain.Entities;

namespace PorteroDigital.Infrastructure.Services;

public sealed class VisitorLogService(IApplicationDbContext dbContext) : IVisitorLogService
{
    public async Task<VisitorLogDto?> CreateAsync(CreateVisitorLogRequest request, CancellationToken cancellationToken)
    {
        var house = await dbContext.Houses
            .SingleOrDefaultAsync(x => x.Id == request.HouseId && x.IsActive, cancellationToken);

        if (house is null)
        {
            return null;
        }

        if (!string.Equals(house.QrToken, request.QrToken.Trim(), StringComparison.Ordinal))
        {
            return null;
        }

        var visitorLog = new VisitorLog
        {
            Id = Guid.NewGuid(),
            HouseId = request.HouseId,
            VisitorName = request.VisitorName.Trim(),
            Reason = request.Reason.Trim(),
            RequestedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.VisitorLogs.Add(visitorLog);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(visitorLog);
    }

    public async Task<VisitorLogDto?> GetByIdAsync(Guid visitorLogId, CancellationToken cancellationToken)
    {
        var visitorLog = await dbContext.VisitorLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == visitorLogId, cancellationToken);

        return visitorLog is null ? null : Map(visitorLog);
    }

    private static VisitorLogDto Map(VisitorLog visitorLog)
    {
        return new VisitorLogDto(
            visitorLog.Id,
            visitorLog.HouseId,
            visitorLog.VisitorName,
            visitorLog.Reason,
            visitorLog.Status.ToString(),
            visitorLog.RequestedAtUtc);
    }
}
