namespace PorteroDigital.Application.Abstractions.Services;

public interface ICameraControlService
{
    Task<bool> SetLightStatusAsync(bool on, CancellationToken cancellationToken);
    Task<bool> SetNightVisionAsync(string mode, CancellationToken cancellationToken); // "Auto", "On", "Off"
}
