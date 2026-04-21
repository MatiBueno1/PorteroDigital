using PorteroDigital.Application.Models.Auth;

namespace PorteroDigital.Application.Abstractions.Services;

public interface IResidentAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<bool> UpdateCredentialsAsync(Guid residentId, string currentPassword, string? newEmail, string? newPassword, CancellationToken cancellationToken);
}
