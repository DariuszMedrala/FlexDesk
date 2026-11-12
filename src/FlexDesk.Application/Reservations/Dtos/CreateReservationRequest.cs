namespace FlexDesk.Application.Reservations.Dtos;

public record CreateReservationRequest(
    Guid WorkspaceId,
    DateTime StartTime,
    DateTime EndTime);
