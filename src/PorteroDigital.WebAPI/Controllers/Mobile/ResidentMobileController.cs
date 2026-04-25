using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Application.Models.Mobile;

namespace PorteroDigital.WebAPI.Controllers.Mobile;

[ApiController]
[Authorize]
[Route("api/mobile")]
public sealed class ResidentMobileController(IResidentMobileService residentMobileService) : ControllerBase
{
    [HttpGet("panel")]
    public async Task<IActionResult> GetPanel(CancellationToken cancellationToken)
    {
        var residentId = GetResidentId();

        if (residentId is null)
        {
            return Unauthorized();
        }

        var panel = await residentMobileService.GetPanelAsync(residentId.Value, cancellationToken);
        return panel is null ? NotFound() : Ok(panel);
    }

    [HttpGet("visits")]
    public async Task<IActionResult> GetVisits(CancellationToken cancellationToken)
    {
        var residentId = GetResidentId();

        if (residentId is null)
        {
            return Unauthorized();
        }

        var visits = await residentMobileService.GetHistoryAsync(residentId.Value, cancellationToken);
        return Ok(visits);
    }

    [HttpPost("devices")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request, CancellationToken cancellationToken)
    {
        var residentId = GetResidentId();

        if (residentId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.DeviceName) || string.IsNullOrWhiteSpace(request.PushToken))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Solicitud invalida",
                Detail = "DeviceName y PushToken son obligatorios.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        await residentMobileService.RegisterDeviceAsync(residentId.Value, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("visits/{visitorLogId:guid}/decision")]
    public async Task<IActionResult> DecideVisit(Guid visitorLogId, [FromBody] DecideVisitRequest request, CancellationToken cancellationToken)
    {
        var residentId = GetResidentId();

        if (residentId is null)
        {
            return Unauthorized();
        }

        var decision = await residentMobileService.DecideVisitAsync(residentId.Value, visitorLogId, request, cancellationToken);

        if (decision is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Visita no encontrada",
                Detail = "La visita no existe para el inquilino autenticado o el estado solicitado no es valido.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(decision);
    }

    [HttpPatch("me/name")]
    public async Task<IActionResult> UpdateName([FromBody] UpdateNameRequest request, CancellationToken cancellationToken)
    {
        var residentId = GetResidentId();

        if (residentId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Nombre invalido",
                Detail = "El nombre no puede estar vacio.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var saved = await residentMobileService.UpdateNameAsync(residentId.Value, request.Name, cancellationToken);

        if (saved is null)
        {
            return NotFound();
        }

        return Ok(new { name = saved });
    }

    [HttpPost("me/credentials")]
    public async Task<IActionResult> UpdateCredentials([FromBody] UpdateCredentialsRequest request, [FromServices] IResidentAuthService authService, CancellationToken cancellationToken)
    {
        var residentId = GetResidentId();

        if (residentId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Contraseña requerida",
                Detail = "Debes ingresar tu contraseña actual para realizar cambios.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var success = await authService.UpdateCredentialsAsync(
            residentId.Value, 
            request.CurrentPassword, 
            request.NewEmail, 
            request.NewPassword, 
            cancellationToken);

        if (!success)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Error de actualización",
                Detail = "No se pudieron actualizar las credenciales. Verifica tu contraseña actual.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return NoContent();
    }

    [HttpPatch("me/house/contact")]
    public async Task<IActionResult> UpdateHouseContact([FromBody] UpdateHouseContactRequest request, CancellationToken cancellationToken)
    {
        var residentId = GetResidentId();

        if (residentId is null)
        {
            return Unauthorized();
        }

        var success = await residentMobileService.UpdateContactConfigAsync(
            residentId.Value, 
            request.PublicContactNumbers, 
            request.ShowContactNumbers, 
            cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private Guid? GetResidentId()
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(subject, out var residentId) ? residentId : null;
    }
}

public record UpdateHouseContactRequest(string? PublicContactNumbers, bool ShowContactNumbers);
public record UpdateCredentialsRequest(string CurrentPassword, string? NewEmail, string? NewPassword);
