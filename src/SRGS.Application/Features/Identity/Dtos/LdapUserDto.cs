namespace SRGS.Application.Features.Identity.Dtos;

/// <summary>
/// Contract returned by the real LDAP implementation. The LDAP JSON user wrapper
/// object is not part of this DTO because the implementation unwraps it. Empty or
/// whitespace strings are acceptable for optional fields because the domain User
/// trims them to null.
/// </summary>
public sealed record LdapUserDto(
    string Username,
    string FirstName,
    string? MiddleName,
    string FamilyName,
    string Email,
    string? PhoneNumber,
    string? Department,
    string? SubDepartment,
    string? Title,
    string EmployeeNumber);