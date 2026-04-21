using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorteroDigital.Application.Abstractions.Persistence;
using PorteroDigital.Application.Abstractions.Services;

namespace PorteroDigital.WebAPI.Controllers;

[ApiController]
[Route("api/houses")]
public sealed class HousesController(
    IHouseQrValidator houseQrValidator,
    IApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var houses = await context.Houses
            .AsNoTracking()
            .Select(h => new
            {
                h.Id,
                h.Identifier,
                h.AddressLabel,
                h.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(houses);
    }

    [HttpGet("{houseId:guid}/qr")]
    public async Task<IActionResult> ValidateQr(Guid houseId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Token requerido",
                Detail = "El token QR es obligatorio para validar la casa.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await houseQrValidator.ValidateAsync(houseId, token, cancellationToken);

        if (result is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Casa no disponible",
                Detail = "La casa no existe, no está activa o el QR es inválido.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(result);
    }
}