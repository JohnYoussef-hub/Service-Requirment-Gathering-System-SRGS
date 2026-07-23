using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Requests.Lookups;

// Maps to MODULE_TYPE. Same admin-managed lookup pattern as RequestType.
public sealed class ModuleType : Entity<int>
{
    public string ModuleName { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ModuleType()
#pragma warning restore CS8618
    { }

    private ModuleType(string moduleName)
    {
        ModuleName = moduleName;
    }

    public static Result<ModuleType> Create(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            return ModuleTypeErrors.NameRequired;
        }

        return new ModuleType(moduleName.Trim());
    }

    public Result<Updated> Rename(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            return ModuleTypeErrors.NameRequired;
        }

        ModuleName = moduleName.Trim();

        return Result.Updated;
    }
}
