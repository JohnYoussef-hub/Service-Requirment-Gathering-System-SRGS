using SRGS.Domain.Common;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Domain.Requests.Events;

// Handlers can use this to write CHANGE_HISTORY rows and queue NOTIFICATION rows
// (e.g. notify the requester on Approved/Rejected/Completed).
public sealed class RequestStatusChanged : DomainEvent
{
    public int RequestId { get; set; }
    public RequestStatus OldStatus { get; set; }
    public RequestStatus NewStatus { get; set; }
}
