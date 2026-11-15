using FluentValidation;

namespace SRGS.Application.Features.Requests.Commands.DecideApproval;

public sealed class DecideApprovalCommandValidator : AbstractValidator<DecideApprovalCommand>
{
    public DecideApprovalCommandValidator()
    {
        RuleFor(x => x.RequestId).GreaterThan(0);
        RuleFor(x => x.ApprovalType).IsInEnum();
        RuleFor(x => x.Decision).IsInEnum().NotEqual(SRGS.Domain.Requests.Approvals.Enums.ApprovalDecisionStatus.Pending);
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
