using PorteroDigital.Domain.Enums;

namespace PorteroDigital.Application.Models.Mobile;

public sealed record DecideVisitRequest(
    VisitorLogStatus Status,
    string? DecisionDetail);
