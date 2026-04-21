namespace PorteroDigital.Application.Models.Visitors;

public sealed record CreateVisitorLogRequest(
    Guid HouseId,
    string QrToken,
    string VisitorName,
    string Reason);
