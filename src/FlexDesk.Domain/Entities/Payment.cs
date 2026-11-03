using FlexDesk.Domain.Enums;

namespace FlexDesk.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; }
    public bool IsSuccessful { get; set; }

    public Reservation Reservation { get; set; } = null!;
}
