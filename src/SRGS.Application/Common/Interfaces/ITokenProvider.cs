using System.Security.Claims;
using SRGS.Application.Features.Identity;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

    Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token);
}