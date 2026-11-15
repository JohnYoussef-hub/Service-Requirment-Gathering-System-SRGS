using FluentValidation;

namespace SRGS.Application.Features.Requests.Commands.RespondToClarification;

public sealed class RespondToClarificationCommandValidator : AbstractValidator<RespondToClarificationCommand>
{
    public RespondToClarificationCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.Response).NotEmpty().MaximumLength(2000);
    }
}
