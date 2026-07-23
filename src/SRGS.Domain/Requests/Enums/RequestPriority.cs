namespace SRGS.Domain.Requests.Enums;

// Backed by CHECK CK_REQUEST_priority: ('Low','Medium','High','Critical'). Default is Low.
public enum RequestPriority
{
    Low,
    Medium,
    High,
    Critical
}
