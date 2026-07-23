using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Roles;

public static class RoleErrors
{
    public static Error NameRequired =>
        Error.Validation("Role.RoleName.Required", "Role name is required.");

    public static Error AlreadyAssigned =>
        Error.Conflict("Role.AlreadyAssigned", "This role is already assigned to the user.");

    public static Error NotAssigned =>
        Error.NotFound("Role.NotAssigned", "This role is not assigned to the user.");
}
