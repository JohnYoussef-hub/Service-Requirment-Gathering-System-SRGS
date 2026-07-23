using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Lookups;

public static class RequestTypeErrors
{
    public static Error NameRequired =>
        Error.Validation("RequestType.TypeName.Required", "Request type name is required.");
}
