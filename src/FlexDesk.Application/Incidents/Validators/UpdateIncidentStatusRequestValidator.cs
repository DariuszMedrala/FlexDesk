using FlexDesk.Application.Incidents.Dtos;
using FluentValidation;

namespace FlexDesk.Application.Incidents.Validators;

public class UpdateIncidentStatusRequestValidator : AbstractValidator<UpdateIncidentStatusRequest>
{
    public UpdateIncidentStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
