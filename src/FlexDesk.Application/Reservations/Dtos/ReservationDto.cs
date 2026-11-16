using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Reservations.Dtos;

public record ReservationDto(
    Guid Id,
    Guid WorkspaceId,
    string WorkspaceName,
    Guid UserId,
    string UserFullName,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    ReservationStatus Status,
    DateTime CreatedAt);
