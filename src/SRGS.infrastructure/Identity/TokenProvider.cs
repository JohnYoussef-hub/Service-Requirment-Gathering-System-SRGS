using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Identity;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;
using SRGS.Domain.Identity;
using SRGS.infrastructure.Identity.JwtOptions;

namespace SRGS.infrastructure.Identity;

public class TokenProvider(IOptions<JwtSettings> options, IAppDbContext context) : ITokenProvider
{
    private readonly IOptions<JwtSettings> _config = options;
    private readonly IAppDbContext _context = context;

    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default)
    {
        var tokenResult = await CreateAsync(user, ct);

        if (tokenResult.IsError)
        {
            return tokenResult.Errors;
        }

        return tokenResult.Value;
    }

    private async Task<Result<TokenResponse>> CreateAsync(AppUserDto user, CancellationToken ct = default)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Value.Secret));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _config.Value.Issuer,
            Audience = _config.Value.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_config.Value.TokenExpirationInMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);

        var oldRefreshToken = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.UserId)
            .ExecuteDeleteAsync(ct);

        var refreshToken = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId,
            // add the expiration time for the refresh token, e.g., 7 days from now
            DateTime.UtcNow.AddDays(7)
        );

        if (refreshToken.IsError)
        {
            return refreshToken.Errors;
        }

        var refreshTokenEntity = refreshToken.Value;
        await _context.RefreshTokens.AddAsync(refreshTokenEntity, ct);
        await _context.SaveChangesAsync(ct);

        return new TokenResponse
        {

            AccessToken = token,
            RefreshToken = refreshTokenEntity.Token,
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(_config.Value.TokenExpirationInMinutes)
        };
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Value.Secret)),
            ValidateIssuer = true,
            ValidIssuer = _config.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = _config.Value.Audience,
            ValidateLifetime = false, // Ignore token expiration
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JsonWebTokenHandler();
        var validationResult = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);

        if (validationResult.IsValid && validationResult.ClaimsIdentity is not null)
        {
            return new ClaimsPrincipal(validationResult.ClaimsIdentity);
        }

        return null;
    }
}