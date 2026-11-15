using FluentValidation;

namespace SRGS.Application.Features.Requests.Commands.TransitionRequest;

public sealed class TransitionRequestCommandValidator : AbstractValidator<TransitionRequestCommand>
{
    public TransitionRequestCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.TargetStatus).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
