namespace PorteroDigital.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "PorteroDigital";
    public string Audience { get; set; } = "PorteroDigital.Mobile";
    public string SigningKey { get; set; } = "CHANGE-THIS-KEY-IN-REAL-ENVIRONMENT-AT-LEAST-32-CHARS";
    public int ExpirationMinutes { get; set; } = 480;
}
