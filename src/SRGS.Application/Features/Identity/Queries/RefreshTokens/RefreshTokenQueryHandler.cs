using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using SRGS.Application.Common.Errors;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Identity.Queries.RefreshTokens;

public class RefreshTokenQueryHandler(ILogger<RefreshTokenQueryHandler> logger, IIdentityService identityService, IAppDbContext context, ITokenProvider tokenProvider)
    : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;
    private readonly IAppDbContext _context = context;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken ct)
    {
        var principal = await _tokenProvider.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);

        if (principal is null)
        {
            _logger.LogError("Expired access token is not valid");

            return ApplicationErrors.InvalidRefreshToken;
        }

        var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (userId is null)
        {
            _logger.LogError("Invalid userId claim");

            return ApplicationErrors.InvalidRefreshToken;
        }

        if (!int.TryParse(userId, out var userIdValue))
        {
            _logger.LogError("Invalid userId claim");

            return ApplicationErrors.InvalidRefreshToken;
        }

        var getUserResult = await _identityService.GetUserByIdAsync(userId);

        if (getUserResult.IsError)
        {
            _logger.LogError("Get user by id error occurred: {ErrorDescription}", getUserResult.TopError.Description);
            return ApplicationErrors.InvalidRefreshToken;
        }

        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == request.RefreshToken && r.UserId == userIdValue, ct);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            _logger.LogError("Refresh token has expired");

            return ApplicationErrors.InvalidRefreshToken;
        }

        var generateTokenResult = await _tokenProvider.GenerateJwtTokenAsync(getUserResult.Value, ct);

        if (generateTokenResult.IsError)
        {
            _logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Description);

            return generateTokenResult.Errors;
        }

        return generateTokenResult.Value;
    }
}