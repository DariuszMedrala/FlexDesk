using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Reservations.Dtos;

public record PaymentDto(
    Guid Id,
    Guid ReservationId,
    decimal Amount,
    DateTime PaymentDate,
    PaymentMethod Method,
    bool IsSuccessful);
