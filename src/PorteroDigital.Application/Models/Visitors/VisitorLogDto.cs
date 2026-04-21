namespace PorteroDigital.Application.Models.Visitors;

public sealed record VisitorLogDto(
    Guid Id,
    Guid HouseId,
    string VisitorName,
    string Reason,
    string Status,
    DateTimeOffset RequestedAtUtc);
