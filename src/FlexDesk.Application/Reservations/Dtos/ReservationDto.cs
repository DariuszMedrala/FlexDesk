using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Reservations.Dtos;

public record ReservationDto(
    Guid Id,
    Guid WorkspaceId,
    string WorkspaceName,
    Guid UserId,
    string UserEmail,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    ReservationStatus Status,
    DateTime CreatedAt);
