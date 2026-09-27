using SRGS.Application.Common.Interfaces;
using SRGS.Application.Common.Errors;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;

using Microsoft.EntityFrameworkCore;

namespace SRGS.infrastructure.Identity;

public class IdentityService(IAppDbContext context, ILdapService ldapService) : IIdentityService
{
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
        var user = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Username == ldapUser.Username);

        if (user is null)
        {
            return ApplicationErrors.UserNotFound;
        }

        var roles = await (
            from userRole in _context.UserRoleRows
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == user.Id
            select role.RoleName)
            .AsNoTracking()
            .ToListAsync();

        return new AppUserDto(user.Id.ToString(), user.Username, roles, []);
    }

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