using FlexDesk.Application.Reservations.Dtos;
using FlexDesk.Domain.Entities;

namespace FlexDesk.Application.Reservations;

public static class ReservationMapping
{
    public static ReservationDto ToDto(this Reservation reservation) =>
        new(
            reservation.Id,
            reservation.WorkspaceId,
            reservation.Workspace.Name,
            reservation.UserId,
            reservation.User.Email,
            reservation.StartTime,
            reservation.EndTime,
            reservation.TotalPrice,
            reservation.Status,
            reservation.CreatedAt);

    public static PaymentDto ToDto(this Payment payment) =>
        new(
            payment.Id,
            payment.ReservationId,
            payment.Amount,
            payment.PaymentDate,
            payment.Method,
            payment.IsSuccessful);
}
