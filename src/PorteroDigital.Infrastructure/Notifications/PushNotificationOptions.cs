namespace PorteroDigital.Infrastructure.Notifications;

public sealed class PushNotificationOptions
{
    public const string SectionName = "PushNotifications";

    public bool Enabled { get; set; }
    public string AndroidChannelId { get; set; } = "visitors";
    public string AndroidSound { get; set; } = "default";
    public string IosSound { get; set; } = "default";
}
