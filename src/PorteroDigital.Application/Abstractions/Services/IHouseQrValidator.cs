using PorteroDigital.Application.Models.Houses;

namespace PorteroDigital.Application.Abstractions.Services;

public interface IHouseQrValidator
{
    Task<QrValidationResult?> ValidateAsync(Guid houseId, string qrToken, CancellationToken cancellationToken);
}
