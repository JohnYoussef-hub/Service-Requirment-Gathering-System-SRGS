using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Common.Interfaces;

public interface ILdapService
{
    Task<Result<LdapUserDto>> AuthenticateAsync(string username, string password);
}