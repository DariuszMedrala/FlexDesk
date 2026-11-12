using FlexDesk.Application.Reservations.Dtos;
using FluentValidation;

namespace FlexDesk.Application.Reservations.Validators;

public class PayReservationRequestValidator : AbstractValidator<PayReservationRequest>
{
    public PayReservationRequestValidator()
    {
        RuleFor(x => x.Method)
            .IsInEnum();
    }
}
