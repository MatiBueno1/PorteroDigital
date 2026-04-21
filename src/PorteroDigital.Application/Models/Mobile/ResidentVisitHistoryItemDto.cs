namespace PorteroDigital.Application.Models.Mobile;

public sealed record ResidentVisitHistoryItemDto(
    Guid Id,
    string VisitorName,
    string Reason,
    string Status,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? RespondedAtUtc,
    string? DecisionDetail);
