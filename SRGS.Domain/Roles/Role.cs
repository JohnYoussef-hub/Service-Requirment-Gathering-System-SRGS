using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Roles;

/// <summary>
/// ROLE is a real, admin-managed lookup table (many-to-many with USER via USER_ROLE),
/// not a fixed C# enum — new roles can be added without a code change/migration of an enum.
/// </summary>
public sealed class Role : Entity<int>
{
    public string RoleName { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Role()
#pragma warning restore CS8618
    { }

    private Role(string roleName)
    {
        RoleName = roleName;
    }

    public static Result<Role> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return RoleErrors.NameRequired;
        }

        return new Role(roleName.Trim());
    }

    public Result<Updated> Rename(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return RoleErrors.NameRequired;
        }

        RoleName = roleName.Trim();

        return Result.Updated;
    }
}
