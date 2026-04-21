using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PorteroDigital.Domain.Entities;
using PorteroDigital.Domain.Enums;
using PorteroDigital.Infrastructure.Notifications;
using PorteroDigital.Infrastructure.Persistence;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.WebAPI.Hubs;

namespace PorteroDigital.WebAPI.Controllers;

[ApiController]
[Route("api/visits")]
public sealed class VisitsController(
    IHubContext<ResidentNotificationsHub> hubContext,
    ResidentPushNotificationService pushNotificationService,
    ICameraControlService cameraControlService,
    PorteroDigitalDbContext context) : ControllerBase
{
    [HttpPost("notify/{houseId:guid}")]
    public async Task<IActionResult> NotifyVisit(Guid houseId, [FromBody] VisitRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Solicitud invalida",
                Detail = "Name y Reason son obligatorios.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var house = await context.Houses
            .Include(h => h.Residents.Where(r => r.IsActive))
            .ThenInclude(r => r.Devices.Where(d => d.IsActive))
            .SingleOrDefaultAsync(h => h.Id == houseId && h.IsActive, cancellationToken);

        if (house is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Casa no disponible",
                Detail = "La casa no existe o no esta activa.",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Si se provee un QR token, validar que coincida (para retrocompatibilidad con QR individuales)
        if (!string.IsNullOrWhiteSpace(request.QrToken) && !string.Equals(house.QrToken, request.QrToken.Trim(), StringComparison.Ordinal))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "QR Invalido",
                Detail = "El QR proporcionado no es valido para esta casa.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var log = new VisitorLog
        {
            Id = Guid.NewGuid(),
            HouseId = houseId,
            VisitorName = request.Name.Trim(),
            Reason = request.Reason.Trim(),
            Status = VisitorLogStatus.Pending,
            RequestedAtUtc = DateTimeOffset.UtcNow
        };

        context.VisitorLogs.Add(log);
        await context.SaveChangesAsync(cancellationToken);

        var response = new
        {
            log.Id,
            log.HouseId,
            log.VisitorName,
            log.Reason,
            Status = log.Status.ToString(),
            log.RequestedAtUtc
        };

        await hubContext.Clients.Group(ResidentNotificationsHub.GetHouseGroup(houseId)).SendAsync("ReceiveNotification", new
        {
            id = log.Id,
            houseId = log.HouseId,
            visitorName = log.VisitorName,
            reason = log.Reason,
            status = log.Status.ToString(),
            timestamp = log.RequestedAtUtc.ToString("HH:mm")
        }, cancellationToken);

        var activeDevices = house.Residents
            .SelectMany(r => r.Devices)
            .Where(d => d.IsActive)
            .ToList();

        await pushNotificationService.NotifyVisitorArrivedAsync(activeDevices, house, log, cancellationToken);

        // Automatización: Luz si es después de las 18:30
        var now = DateTimeOffset.Now; // Hora local del servidor
        if (now.Hour > 18 || (now.Hour == 18 && now.Minute >= 30))
        {
            _ = cameraControlService.SetLightStatusAsync(true, CancellationToken.None);
        }

        return CreatedAtAction(nameof(GetHistory), new { houseId }, response);
    }

    [HttpGet("history/{houseId:guid}")]
    public async Task<IActionResult> GetHistory(Guid houseId, CancellationToken cancellationToken)
    {
        var history = await context.VisitorLogs
            .AsNoTracking()
            .Where(v => v.HouseId == houseId)
            .OrderByDescending(v => v.RequestedAtUtc)
            .Take(10)
            .Select(v => new
            {
                v.Id,
                v.VisitorName,
                v.Reason,
                Status = v.Status.ToString(),
                Timestamp = v.RequestedAtUtc.ToString("dd/MM HH:mm")
            })
            .ToListAsync(cancellationToken);

        return Ok(history);
    }
}

public sealed record VisitRequest(string Name, string Reason, string? QrToken = null);
