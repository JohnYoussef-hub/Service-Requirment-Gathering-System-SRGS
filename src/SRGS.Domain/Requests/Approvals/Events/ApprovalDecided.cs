using SRGS.Domain.Common;
using SRGS.Domain.Requests.Approvals.Enums;

namespace SRGS.Domain.Requests.Approvals.Events;

// Handlers can use this to advance Request.Status (e.g. once every required approval is
// in, move the request from PendingApproval to Approved) and to write a NOTIFICATION row.
public sealed class ApprovalDecided : DomainEvent
{
    public int ApprovalId { get; set; }
    public int RequestId { get; set; }
    public ApprovalDecisionStatus Decision { get; set; }
}
