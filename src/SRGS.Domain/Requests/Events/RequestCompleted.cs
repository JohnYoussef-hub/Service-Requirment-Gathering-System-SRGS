using SRGS.Domain.Common;

namespace SRGS.Domain.Requests.Events;

public sealed class RequestCompleted : DomainEvent
{
    public int RequestId { get; set; }
}
