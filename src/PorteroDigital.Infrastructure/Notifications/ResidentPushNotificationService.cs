using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PorteroDigital.Domain.Entities;
using PorteroDigital.Domain.Enums;

namespace PorteroDigital.Infrastructure.Notifications;

public sealed class ResidentPushNotificationService(
    HttpClient httpClient,
    IOptions<PushNotificationOptions> options,
    ILogger<ResidentPushNotificationService> logger)
{
    public async Task NotifyVisitorArrivedAsync(IEnumerable<ResidentDevice> devices, House house, VisitorLog visitorLog, CancellationToken cancellationToken)
    {
        var deviceList = devices.Where(d => d.IsActive && d.PushToken.StartsWith("ExponentPushToken")).ToList();

        if (deviceList.Count == 0)
        {
            return;
        }

        var settings = options.Value;

        var expoEndpoint = "https://exp.host/--/api/v2/push/send";
        var notifications = new List<object>();

        foreach (var device in deviceList)
        {
            var sound = string.IsNullOrWhiteSpace(device.NotificationSound)
                ? device.Platform == PushPlatform.Ios ? settings.IosSound : settings.AndroidSound
                : device.NotificationSound;

            notifications.Add(new
            {
                to = device.PushToken,
                sound = sound == "default" ? "default" : null,
                title = "🛎️ Nueva Visita",
                body = $"{visitorLog.VisitorName} está en la entrada. {(string.IsNullOrWhiteSpace(visitorLog.Reason) ? "" : $"Motivo: {visitorLog.Reason}")}",
                data = new
                {
                    type = "visitor_ring",
                    houseId = house.Id,
                    id = visitorLog.Id,
                    visitorName = visitorLog.VisitorName,
                    reason = visitorLog.Reason
                }
            });

            logger.LogInformation("Enviando Push a {DeviceId} ({Platform}) - Token: {Token}", device.Id, device.Platform, device.PushToken);
        }

        try
        {
            var response = await httpClient.PostAsJsonAsync(expoEndpoint, notifications, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Error al enviar Push a Expo: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepción al enviar Push Notifications");
        }
    }
}
