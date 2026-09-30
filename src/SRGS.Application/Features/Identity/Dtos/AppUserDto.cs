using System.Security.Claims;

namespace SRGS.Application.Features.Identity.Dtos;

public sealed record AppUserDto(int UserId, string Username, IList<string> Roles);