namespace FlexDesk.Application.Common.Messaging;

public record ReservationConfirmedEvent(
    Guid ReservationId,
    string UserEmail,
    string WorkspaceName,
    DateTime StartTime,
    DateTime EndTime,
    decimal Amount);
