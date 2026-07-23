using SRGS.Domain.Common;

namespace SRGS.Domain.Requests.Events;

public sealed class RequestAssignedToBusinessAnalyst : DomainEvent
{
    public int RequestId { get; set; }
    public int BusinessAnalystId { get; set; }
}
