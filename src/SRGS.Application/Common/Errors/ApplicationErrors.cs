using SRGS.Domain.Common.Results;

namespace SRGS.Application.Common.Errors;

// Cross-entity/lookup errors that don't belong to any single aggregate's own *Errors class
// (e.g. Domain.Requests.RequestErrors) — same split MechanicShop uses.
public static class ApplicationErrors
{
    public static Error UserNotFound =>
        Error.NotFound("ApplicationErrors.User.NotFound", "User does not exist.");

    public static Error RequestTypeNotFound =>
        Error.NotFound("ApplicationErrors.RequestType.NotFound", "Request type does not exist.");

    public static Error ModuleTypeNotFound =>
        Error.NotFound("ApplicationErrors.ModuleType.NotFound", "Module type does not exist.");
}
