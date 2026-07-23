namespace SRGS.Domain.Requests.Enums;

// Backed by CHECK CK_REQUEST_uat: ('Passed','Failed','Pending'). Column is nullable —
// null means UAT hasn't started yet, so this enum only covers the "has an opinion" states.
public enum UatResult
{
    Pending,
    Passed,
    Failed
}
