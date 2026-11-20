using FlexDesk.Application.Reservations.Dtos;
using FluentValidation;

namespace FlexDesk.Application.Reservations.Validators;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty();

        RuleFor(x => x.StartTime)
            .NotEmpty();

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .GreaterThan(x => x.StartTime)
            .WithMessage("Czas zakończenia musi być późniejszy niż czas rozpoczęcia.");
    }
}
