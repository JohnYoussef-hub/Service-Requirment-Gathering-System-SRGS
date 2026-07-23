using SRGS.Domain.Common.Results;

namespace SRGS.Domain.Users;

public static class UserErrors
{
    public static Error UsernameRequired =>
        Error.Validation("User.Username.Required", "Username is required.");

    public static Error FirstNameRequired =>
        Error.Validation("User.FirstName.Required", "First name is required.");

    public static Error FamilyNameRequired =>
        Error.Validation("User.FamilyName.Required", "Family name is required.");

    public static Error EmailInvalid =>
        Error.Validation("User.Email.Invalid", "A valid email is required.");

    public static Error EmployeeNumberRequired =>
        Error.Validation("User.EmployeeNumber.Required", "Employee number is required.");

    public static Error RoleIdRequired =>
        Error.Validation("User.RoleId.Required", "A valid role id is required.");

    public static Error NotFound =>
        Error.NotFound("User.NotFound", "User was not found.");
}
