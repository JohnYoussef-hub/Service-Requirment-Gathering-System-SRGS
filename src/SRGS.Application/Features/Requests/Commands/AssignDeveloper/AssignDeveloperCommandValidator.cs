using FluentValidation;

namespace SRGS.Application.Features.Requests.Commands.AssignDeveloper;

public sealed class AssignDeveloperCommandValidator : AbstractValidator<AssignDeveloperCommand>
{
    public AssignDeveloperCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.DeveloperId).GreaterThan(0);
    }
}
