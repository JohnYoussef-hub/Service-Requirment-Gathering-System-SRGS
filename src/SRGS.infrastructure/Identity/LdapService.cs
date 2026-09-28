using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.infrastructure.Identity;

public sealed class LdapService : ILdapService
{
    public Task<Result<LdapUserDto>> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult<Result<LdapUserDto>>(Error.Conflict(
                "Ldap.InvalidCredentials",
                "The username or password is invalid."));
        }

        return Task.FromResult<Result<LdapUserDto>>(new LdapUserDto(
            "test.user",
            "Test",
            null,
            "User",
            "test.user@example.com",
            null,
            null,
            null,
            null,
            "00000"));
    }
}