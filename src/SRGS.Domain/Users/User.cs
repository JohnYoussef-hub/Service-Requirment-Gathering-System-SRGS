using SRGS.Domain.Common;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Roles;

namespace SRGS.Domain.Users;

/// <summary>
/// Maps to the USER table. Surrogate INT key on purpose — see the schema comment: AD
/// usernames (username) can be renamed, so they stay a UNIQUE column, never the PK.
/// Role assignment is modeled as a set of RoleIds (USER_ROLE junction) rather than a
/// separate join entity, since the junction row itself carries no extra data.
/// </summary>
public sealed class User : Entity<int>
{
    public string Username { get; private set; }
    public string FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string FamilyName { get; private set; }
    public string Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Department { get; private set; }
    public string? SubDepartment { get; private set; }
    public string? Title { get; private set; }
    public string EmployeeNumber { get; private set; }

    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {FamilyName}"
        : $"{FirstName} {MiddleName} {FamilyName}";

    private readonly HashSet<int> _roleIds = [];
    public IReadOnlyCollection<int> RoleIds => _roleIds;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private User()
#pragma warning restore CS8618
    { }

    private User(
        string username,
        string firstName,
        string? middleName,
        string familyName,
        string email,
        string? phoneNumber,
        string? department,
        string? subDepartment,
        string? title,
        string employeeNumber)
    {
        Username = username;
        FirstName = firstName;
        MiddleName = middleName;
        FamilyName = familyName;
        Email = email;
        PhoneNumber = phoneNumber;
        Department = department;
        SubDepartment = subDepartment;
        Title = title;
        EmployeeNumber = employeeNumber;
    }

    public static Result<User> Create(
        string username,
        string firstName,
        string familyName,
        string email,
        string employeeNumber,
        string? middleName = null,
        string? phoneNumber = null,
        string? department = null,
        string? subDepartment = null,
        string? title = null)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return UserErrors.UsernameRequired;
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return UserErrors.FirstNameRequired;
        }

        if (string.IsNullOrWhiteSpace(familyName))
        {
            return UserErrors.FamilyNameRequired;
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return UserErrors.EmailInvalid;
        }

        if (string.IsNullOrWhiteSpace(employeeNumber))
        {
            return UserErrors.EmployeeNumberRequired;
        }

        return new User(
            username.Trim(),
            firstName.Trim(),
            string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim(),
            familyName.Trim(),
            email.Trim(),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            string.IsNullOrWhiteSpace(department) ? null : department.Trim(),
            string.IsNullOrWhiteSpace(subDepartment) ? null : subDepartment.Trim(),
            string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            employeeNumber.Trim());
    }

    public Result<Updated> UpdateContactInfo(string email, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return UserErrors.EmailInvalid;
        }

        Email = email.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();

        return Result.Updated;
    }

    public Result<Updated> UpdateOrgInfo(string? department, string? subDepartment, string? title)
    {
        Department = string.IsNullOrWhiteSpace(department) ? null : department.Trim();
        SubDepartment = string.IsNullOrWhiteSpace(subDepartment) ? null : subDepartment.Trim();
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();

        return Result.Updated;
    }

    public Result<Updated> AssignRole(int roleId)
    {
        if (roleId <= 0)
        {
            return UserErrors.RoleIdRequired;
        }

        if (!_roleIds.Add(roleId))
        {
            return RoleErrors.AlreadyAssigned;
        }

        return Result.Updated;
    }

    public Result<Updated> RemoveRole(int roleId)
    {
        if (!_roleIds.Remove(roleId))
        {
            return RoleErrors.NotAssigned;
        }

        return Result.Updated;
    }
}
