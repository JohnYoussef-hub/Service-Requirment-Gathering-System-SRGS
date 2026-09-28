using SRGS.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Models;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace SRGS.infrastructure.Identity;

public class IdentityService(IAppDbContext context, ILdapService ldapService) : IIdentityService
{
    private const string DefaultRoleName = "Requester";

    private readonly IAppDbContext _context = context;
    private readonly ILdapService _ldapService = ldapService;

    public async Task<Result<AppUserDto>> AuthenticateAsync(string username, string password)
    {
        var ldapResult = await _ldapService.AuthenticateAsync(username, password);

        if (ldapResult.IsError)
        {
            return ApplicationErrors.InvalidCredentials;
        }

        var ldapUser = ldapResult.Value;
        var userResult = await GetOrProvisionUserAsync(ldapUser);
        if (userResult.IsError)
        {
            return userResult.Errors;
        }

        var user = userResult.Value;
        var roles = await GetRoleNamesAsync(user.Id);

        return new AppUserDto(user.Id.ToString(), user.Username, roles);
    }


    // -----private helpers for AuthenticateAsync-------------
    private async Task<Result<User>> GetOrProvisionUserAsync(LdapUserDto ldapUser)
    {
        var username = ldapUser.Username.Trim();
        var user = await _context.Users
            .SingleOrDefaultAsync(candidate => candidate.Username == username);

        if (user is not null)
        {
            RefreshUserProfile(user, ldapUser);
            await _context.SaveChangesAsync(CancellationToken.None);
            return user;
        }

        var createdUser = User.Create(
            username,
            ldapUser.FirstName,
            ldapUser.FamilyName,
            ldapUser.Email,
            ldapUser.EmployeeNumber,
            ldapUser.MiddleName,
            ldapUser.PhoneNumber,
            ldapUser.Department,
            ldapUser.SubDepartment,
            ldapUser.Title);

        if (createdUser.IsError)
        {
            return createdUser.Errors;
        }

        var defaultRole = await _context.Roles
            .SingleOrDefaultAsync(role => role.RoleName == DefaultRoleName);

        if (defaultRole is null)
        {
            return ApplicationErrors.DefaultRoleNotFound;
        }

        var roleAssignment = createdUser.Value.AssignRole(defaultRole.Id);
        if (roleAssignment.IsError)
        {
            return roleAssignment.Errors;
        }

        var newUser = createdUser.Value;
        _context.Users.Add(newUser);

        try
        {
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        catch (DbUpdateException exception) when (IsUsernameUniqueConstraintViolation(exception))
        {
            _context.Users.Local.Remove(newUser);

            var concurrentUser = await _context.Users
                .SingleOrDefaultAsync(candidate => candidate.Username == username);

            if (concurrentUser is null)
            {
                throw;
            }

            RefreshUserProfile(concurrentUser, ldapUser);
            await _context.SaveChangesAsync(CancellationToken.None);
            return concurrentUser;
        }

        _context.UserRoleRows.Add(new UserRoleRow
        {
            UserId = newUser.Id,
            RoleId = defaultRole.Id
        });
        await _context.SaveChangesAsync(CancellationToken.None);

        return newUser;
    }

    private static bool IsUsernameUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }

    private static void RefreshUserProfile(User user, LdapUserDto ldapUser)
    {
        user.UpdateContactInfo(ldapUser.Email, ldapUser.PhoneNumber);
        user.UpdateOrgInfo(ldapUser.Department, ldapUser.SubDepartment, ldapUser.Title);
    }

    private async Task<IList<string>> GetRoleNamesAsync(int userId)
    {
        return await (
            from userRole in _context.UserRoleRows
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.RoleName)
            .AsNoTracking()
            .ToListAsync();
    }

    // -------------------------------------------------------


    public Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetUserNameAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        throw new NotImplementedException();
    }

    //TODO: 
}