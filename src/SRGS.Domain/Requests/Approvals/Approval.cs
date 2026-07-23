using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Requests.Approvals.Enums;
using SRGS.Domain.Requests.Approvals.Events;

namespace SRGS.Domain.Requests.Approvals;

public sealed class Approval : Entity<int>
{
    public int RequestId { get; private set; }
    public int ApproverId { get; private set; }
    public ApprovalType ApprovalType { get; private set; }
    public ApprovalDecisionStatus Decision { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Approval()
#pragma warning restore CS8618
    { }

    private Approval(int requestId, int approverId, ApprovalType approvalType)
    {
        RequestId = requestId;
        ApproverId = approverId;
        ApprovalType = approvalType;
        Decision = ApprovalDecisionStatus.Pending;
    }

    public static Result<Approval> Create(int requestId, int approverId, ApprovalType approvalType)
    {
        if (requestId <= 0)
        {
            return ApprovalErrors.RequestIdRequired;
        }

        if (approverId <= 0)
        {
            return ApprovalErrors.ApproverRequired;
        }

        if (!Enum.IsDefined(approvalType))
        {
            return ApprovalErrors.TypeInvalid;
        }

        return new Approval(requestId, approverId, approvalType);
    }

    public Result<Updated> Decide(ApprovalDecisionStatus decision)
    {
        if (Decision != ApprovalDecisionStatus.Pending)
        {
            return ApprovalErrors.AlreadyDecided(Id);
        }

        if (decision == ApprovalDecisionStatus.Pending)
        {
            return ApprovalErrors.DecisionInvalid;
        }

        Decision = decision;
        DecidedAtUtc = DateTimeOffset.UtcNow;

        AddDomainEvent(new ApprovalDecided { ApprovalId = Id, RequestId = RequestId, Decision = decision });

        return Result.Updated;
    }
}
