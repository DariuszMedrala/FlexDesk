using FlexDesk.Application.Reservations.Dtos;

namespace FlexDesk.Application.Reservations;

public interface IReservationService
{
    Task<IReadOnlyList<ReservationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ReservationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);

    Task<PaymentDto> PayAsync(Guid id, PayReservationRequest request, CancellationToken cancellationToken = default);

    Task<ReservationDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
