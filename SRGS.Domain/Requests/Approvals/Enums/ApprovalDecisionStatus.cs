namespace SRGS.Domain.Requests.Approvals.Enums;

// Backed by CHECK CK_APPROVAL_decision: ('Approved','Rejected','Pending'). Default is Pending.
public enum ApprovalDecisionStatus
{
    Pending,
    Approved,
    Rejected
}
