using FlexDesk.Application.Workspaces.Dtos;
using FluentValidation;

namespace FlexDesk.Application.Workspaces.Validators;

public class CreateWorkspaceRequestValidator : AbstractValidator<CreateWorkspaceRequest>
{
    public CreateWorkspaceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Capacity)
            .GreaterThan(0);

        RuleFor(x => x.PricePerHour)
            .GreaterThan(0);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.BuildingNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Floor)
            .InclusiveBetween(-5, 200);
    }
}
