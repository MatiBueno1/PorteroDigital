using Microsoft.EntityFrameworkCore;
using PorteroDigital.Application.Abstractions.Persistence;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Application.Models.Auth;
using PorteroDigital.Infrastructure.Security;

namespace PorteroDigital.Infrastructure.Services;

public sealed class ResidentAuthService(
    IApplicationDbContext dbContext,
    ResidentPasswordHasher passwordHasher,
    ResidentTokenService tokenService) : IResidentAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();
        var resident = await dbContext.Residents
            .Include(r => r.House)
            .SingleOrDefaultAsync(r => r.Email.ToLower() == emailLower && r.IsActive, cancellationToken);

        if (resident is null)
        {
            return null;
        }

        if (!passwordHasher.Verify(request.Password, resident.PasswordHash))
        {
            return null;
        }

        resident.LastLoginAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var token = tokenService.CreateToken(resident, resident.House);

        return new LoginResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            resident.Id,
            resident.HouseId,
            resident.FullName,
            resident.House.AddressLabel);
    }

    public async Task<bool> UpdateCredentialsAsync(Guid residentId, string currentPassword, string? newEmail, string? newPassword, CancellationToken cancellationToken)
    {
        var resident = await dbContext.Residents
            .SingleOrDefaultAsync(r => r.Id == residentId && r.IsActive, cancellationToken);

        if (resident is null) return false;

        // Validar contraseña actual
        if (!passwordHasher.Verify(currentPassword, resident.PasswordHash))
        {
            return false;
        }

        // Actualizar email si se provee
        if (!string.IsNullOrWhiteSpace(newEmail))
        {
            resident.Email = newEmail.Trim().ToLowerInvariant();
        }

        // Actualizar password si se provee
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            resident.PasswordHash = passwordHasher.Hash(newPassword);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
