using Microsoft.AspNetCore.Mvc;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Application.Models.Visitors;

namespace PorteroDigital.WebAPI.Controllers;

[ApiController]
[Route("api/visitor-logs")]
public sealed class VisitorLogsController(IVisitorLogService visitorLogService) : ControllerBase
{
    [HttpGet("{visitorLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid visitorLogId, CancellationToken cancellationToken)
    {
        var visitorLog = await visitorLogService.GetByIdAsync(visitorLogId, cancellationToken);

        if (visitorLog is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Visita no encontrada",
                Detail = "No existe un registro de visita con el identificador indicado.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(visitorLog);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateVisitorLogRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VisitorName) || string.IsNullOrWhiteSpace(request.Reason) || string.IsNullOrWhiteSpace(request.QrToken))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Solicitud inválida",
                Detail = "HouseId, QrToken, VisitorName y Reason son obligatorios.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var visitorLog = await visitorLogService.CreateAsync(request, cancellationToken);

        if (visitorLog is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Casa no disponible",
                Detail = "La casa no existe, no está activa o el QR es inválido.",
                Status = StatusCodes.Status404NotFound
            });
        }

        //await hubContext.Clients
            //.Group(ResidentNotificationsHub.GetHouseGroup(visitorLog.HouseId))
           // .SendAsync("VisitorRequested", visitorLog, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { visitorLogId = visitorLog.Id }, visitorLog);
    }
}
