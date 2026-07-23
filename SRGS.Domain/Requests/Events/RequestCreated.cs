using SRGS.Domain.Common;

namespace SRGS.Domain.Requests.Events;

public sealed class RequestCreated : DomainEvent
{
    public int RequestId { get; set; }
}
