using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Approvals;

public static class ApprovalErrors
{
    public static Error RequestIdRequired =>
        Error.Validation("Approval.RequestId.Required", "Request id is required.");

    public static Error ApproverRequired =>
        Error.Validation("Approval.ApproverUserId.Required", "Approver id is required.");

    public static Error TypeInvalid =>
        Error.Validation("Approval.ApprovalType.Invalid", "The provided approval type is invalid.");

    public static Error DecisionInvalid =>
        Error.Validation("Approval.Decision.Invalid", "Decision must be Approved or Rejected.");

    public static Error AlreadyDecided(int id) => Error.Conflict(
        code: "Approval.AlreadyDecided",
        description: $"Approval '{id}' has already been decided.");
}
