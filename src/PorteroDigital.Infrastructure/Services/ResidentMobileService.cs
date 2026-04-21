using Microsoft.EntityFrameworkCore;
using PorteroDigital.Application.Abstractions.Persistence;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Application.Models.Mobile;
using PorteroDigital.Domain.Entities;
using PorteroDigital.Domain.Enums;

namespace PorteroDigital.Infrastructure.Services;

public sealed class ResidentMobileService(IApplicationDbContext dbContext) : IResidentMobileService
{
    public async Task<ResidentPanelDto?> GetPanelAsync(Guid residentId, CancellationToken cancellationToken)
    {
        var resident = await dbContext.Residents
            .AsNoTracking()
            .Include(r => r.House)
            .ThenInclude(h => h.CameraConfiguration)
            .SingleOrDefaultAsync(r => r.Id == residentId && r.IsActive, cancellationToken);

        if (resident is null)
        {
            return null;
        }

        var recentVisits = await GetHistoryAsync(residentId, cancellationToken);

        return new ResidentPanelDto(
            resident.Id,
            resident.FullName,
            resident.HouseId,
            resident.House.Identifier,
            resident.House.AddressLabel,
            resident.House.CameraConfiguration?.IsEnabled == true,
            resident.House.CameraConfiguration?.IsEnabled == true ? resident.House.CameraConfiguration.StreamUrl : null,
            recentVisits);
    }

    public async Task<IReadOnlyList<ResidentVisitHistoryItemDto>> GetHistoryAsync(Guid residentId, CancellationToken cancellationToken)
    {
        try
        {
            return await GetHistoryInternalAsync(residentId, cancellationToken);
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("ResidentDecision"))
        {
            // Auto-repair: column missing
            if (dbContext is DbContext efContext)
            {
                await efContext.Database.ExecuteSqlRawAsync("ALTER TABLE VisitorLogs ADD COLUMN ResidentDecision TEXT NULL;", cancellationToken);
            }
            return await GetHistoryInternalAsync(residentId, cancellationToken);
        }
    }

    private async Task<IReadOnlyList<ResidentVisitHistoryItemDto>> GetHistoryInternalAsync(Guid residentId, CancellationToken cancellationToken)
    {
        var resident = await dbContext.Residents
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == residentId && r.IsActive, cancellationToken);

        if (resident is null) return [];

        // Consulta ultra simple: Solo filtro por HouseId y Take(100). Todo lo demás en memoria.
        var allLogs = await dbContext.VisitorLogs
            .Where(v => v.HouseId == resident.HouseId)
            .Take(100)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var fiveMinutesAgo = now.AddMinutes(-5);
        var changed = false;

        foreach (var log in allLogs.Where(l => l.Status == VisitorLogStatus.Pending))
        {
            if (log.RequestedAtUtc < fiveMinutesAgo)
            {
                log.Status = VisitorLogStatus.Expired;
                changed = true;
            }
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return allLogs
            .OrderByDescending(v => v.RequestedAtUtc)
            .Take(50)
            .Select(v => new ResidentVisitHistoryItemDto(
                v.Id,
                v.VisitorName,
                v.Reason,
                v.Status.ToString(),
                v.RequestedAtUtc,
                v.RespondedAtUtc,
                v.ResidentDecision))
            .ToList();
    }

    public async Task<ResidentVisitHistoryItemDto?> DecideVisitAsync(Guid residentId, Guid visitorLogId, DecideVisitRequest request, CancellationToken cancellationToken)
    {
        if (request.Status is not VisitorLogStatus.Approved and not VisitorLogStatus.Rejected)
        {
            return null;
        }

        var resident = await dbContext.Residents
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.Id == residentId && r.IsActive, cancellationToken);

        if (resident is null) return null;

        var visitorLog = await dbContext.VisitorLogs
            .SingleOrDefaultAsync(v => v.Id == visitorLogId, cancellationToken);

        if (visitorLog is null || visitorLog.HouseId != resident.HouseId)
        {
            return null;
        }

        visitorLog.Status = request.Status;
        visitorLog.RespondedAtUtc = DateTimeOffset.UtcNow;
        visitorLog.ResidentDecision = string.IsNullOrWhiteSpace(request.DecisionDetail)
            ? $"{request.Status}"
            : request.DecisionDetail.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ResidentVisitHistoryItemDto(
            visitorLog.Id,
            visitorLog.VisitorName,
            visitorLog.Reason,
            visitorLog.Status.ToString(),
            visitorLog.RequestedAtUtc,
            visitorLog.RespondedAtUtc,
            visitorLog.ResidentDecision);
    }

    public async Task RegisterDeviceAsync(Guid residentId, RegisterDeviceRequest request, CancellationToken cancellationToken)
    {
        var resident = await dbContext.Residents
            .SingleOrDefaultAsync(r => r.Id == residentId && r.IsActive, cancellationToken);

        if (resident is null)
        {
            return;
        }

        var existingDevice = await dbContext.ResidentDevices
            .SingleOrDefaultAsync(d => d.ResidentId == residentId && d.PushToken == request.PushToken.Trim(), cancellationToken);

        if (existingDevice is null)
        {
            dbContext.ResidentDevices.Add(new ResidentDevice
            {
                Id = Guid.NewGuid(),
                ResidentId = residentId,
                DeviceName = request.DeviceName.Trim(),
                Platform = request.Platform,
                PushToken = request.PushToken.Trim(),
                NotificationSound = string.IsNullOrWhiteSpace(request.NotificationSound) ? "default" : request.NotificationSound.Trim(),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                LastSeenAtUtc = DateTimeOffset.UtcNow,
                IsActive = true
            });
        }
        else
        {
            existingDevice.DeviceName = request.DeviceName.Trim();
            existingDevice.Platform = request.Platform;
            existingDevice.NotificationSound = string.IsNullOrWhiteSpace(request.NotificationSound) ? "default" : request.NotificationSound.Trim();
            existingDevice.LastSeenAtUtc = DateTimeOffset.UtcNow;
            existingDevice.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> UpdateNameAsync(Guid residentId, string newName, CancellationToken cancellationToken)
    {
        var resident = await dbContext.Residents
            .SingleOrDefaultAsync(r => r.Id == residentId && r.IsActive, cancellationToken);

        if (resident is null)
        {
            return null;
        }

        var trimmed = newName.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        resident.FullName = trimmed;
        await dbContext.SaveChangesAsync(cancellationToken);
        return resident.FullName;
    }
}
