using System.Security.Claims;

namespace SRGS.Application.Features.Identity.Dtos;

public sealed record AppUserDto(string UserId, string Username, IList<string> Roles, IList<Claim> Claims);