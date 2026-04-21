using Microsoft.AspNetCore.Mvc;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Application.Models.Auth;

namespace PorteroDigital.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IResidentAuthService residentAuthService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales invalidas",
                Detail = "Email y password son obligatorios.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var response = await residentAuthService.LoginAsync(request, cancellationToken);

        if (response is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Acceso denegado",
                Detail = "Las credenciales no son validas.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(response);
    }
}
