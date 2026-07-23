using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Lookups;

public static class ModuleTypeErrors
{
    public static Error NameRequired =>
        Error.Validation("ModuleType.ModuleName.Required", "Module name is required.");
}
