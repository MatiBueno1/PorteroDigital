using PorteroDigital.Domain.Enums;

namespace PorteroDigital.Application.Models.Mobile;

public sealed record RegisterDeviceRequest(
    string DeviceName,
    PushPlatform Platform,
    string PushToken,
    string NotificationSound);
