namespace SRGS.Domain.Requests.Enums;

// NOTE: status has no CHECK constraint yet in the DB (tracked as a pending schema
// improvement). This enum is my best-guess lifecycle based on the default value
// ('Under Analysis') and the approval/development/UAT tables already in the schema —
// confirm the exact set and legal transitions against the BRD and adjust
// Request.CanTransitionTo accordingly before treating this as final.
public enum RequestStatus
{
    UnderAnalysis,
    PendingApproval,
    Approved,
    Rejected,
    InDevelopment,
    OnHold,
    InUat,
    Completed,
    Cancelled
}
