using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Lookups;

// Maps to REQUEST_TYPE. Admin-managed lookup, not an enum, so new request types don't
// require a code change.
public sealed class RequestType : Entity<int>
{
    public string TypeName { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private RequestType()
#pragma warning restore CS8618
    { }

    private RequestType(string typeName)
    {
        TypeName = typeName;
    }

    public static Result<RequestType> Create(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return RequestTypeErrors.NameRequired;
        }

        return new RequestType(typeName.Trim());
    }

    public Result<Updated> Rename(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return RequestTypeErrors.NameRequired;
        }

        TypeName = typeName.Trim();

        return Result.Updated;
    }
}
