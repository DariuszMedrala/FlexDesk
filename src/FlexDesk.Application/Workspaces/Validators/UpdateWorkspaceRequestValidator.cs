using FlexDesk.Application.Workspaces.Dtos;
using FluentValidation;

namespace FlexDesk.Application.Workspaces.Validators;

public class UpdateWorkspaceRequestValidator : AbstractValidator<UpdateWorkspaceRequest>
{
    public UpdateWorkspaceRequestValidator()
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
    }
}
