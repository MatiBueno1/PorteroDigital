using Microsoft.EntityFrameworkCore;
using PorteroDigital.Application.Abstractions.Persistence;
using PorteroDigital.Application.Abstractions.Services;
using PorteroDigital.Application.Models.Houses;

namespace PorteroDigital.Infrastructure.Services;

public sealed class HouseQrValidator(IApplicationDbContext dbContext) : IHouseQrValidator
{
    public async Task<QrValidationResult?> ValidateAsync(Guid houseId, string qrToken, CancellationToken cancellationToken)
    {
        var house = await dbContext.Houses
            .AsNoTracking()
            .Include(x => x.CameraConfiguration)
            .SingleOrDefaultAsync(x => x.Id == houseId && x.IsActive, cancellationToken);

        if (house is null)
        {
            return null;
        }

        if (!string.Equals(house.QrToken, qrToken.Trim(), StringComparison.Ordinal))
        {
            return null;
        }

        return new QrValidationResult(
            house.Id,
            house.Identifier,
            house.AddressLabel,
            house.CameraConfiguration?.IsEnabled == true,
            house.CameraConfiguration?.IsEnabled == true ? house.CameraConfiguration.StreamUrl : null);
    }
}
