namespace SRGS.Domain.Requests.Enums;

// Same caveat as RequestStatus: no CHECK constraint in the DB yet. Default is 'Logging'.
// Modeled as a broadly sequential pipeline phase, separate from Status (Status can be
// e.g. OnHold/Rejected/Cancelled at any Phase).
public enum RequestPhase
{
    Logging,
    Analysis,
    Approval,
    Development,
    Testing,
    Deployment,
    Closed
}
