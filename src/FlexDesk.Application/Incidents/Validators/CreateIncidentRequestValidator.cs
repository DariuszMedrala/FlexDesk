using FlexDesk.Application.Incidents.Dtos;
using FluentValidation;

namespace FlexDesk.Application.Incidents.Validators;

public class CreateIncidentRequestValidator : AbstractValidator<CreateIncidentRequest>
{
    public CreateIncidentRequestValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
