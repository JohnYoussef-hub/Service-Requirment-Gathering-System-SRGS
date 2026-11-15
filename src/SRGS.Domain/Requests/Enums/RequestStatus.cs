namespace SRGS.Domain.Requests.Enums;

public enum RequestStatus
{
    Submitted,
    UnderReview,
    ClarificationRequired,
    Analysis,
    ManagementApproval,
    StakeholdersApproval,
    Assigned,
    Rejected,
    InDevelopment,
    InternalTesting,
    UAT,
    Deployed,
    Closed,
    Cancelled
}
