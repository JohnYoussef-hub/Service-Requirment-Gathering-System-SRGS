using SRGS.Domain.Common;
using SRGS.Domain.Requests.Enums;

namespace SRGS.Domain.Requests.Events;

public sealed class RequestPhaseChanged : DomainEvent
{
    public int RequestId { get; set; }
    public RequestPhase OldPhase { get; set; }
    public RequestPhase NewPhase { get; set; }
}
