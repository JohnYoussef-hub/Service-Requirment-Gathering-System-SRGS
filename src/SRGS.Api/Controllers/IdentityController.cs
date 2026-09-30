using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MechanicShop.Application.Common.Interfaces;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Identity;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Application.Features.Identity.Queries.GenerateTokens;
using SRGS.Application.Features.Identity.Queries.GetUserInfo;
using SRGS.Application.Features.Identity.Queries.RefreshTokens;

namespace SRGS.Api.Controllers;


[Route("identity")]
[ApiVersionNeutral]
public sealed class IdentityController(ISender sender, ICurrentUser currentUser) : ApiController
{
    private readonly ICurrentUser _currentUser = currentUser;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GenerateToken([FromBody] GenerateTokenQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);

        return result.Match(
            response => Ok(response),
            Problem
        );
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);

        return result.Match(
            response => Ok(response),
            Problem
        );
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AppUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]

    public async Task<IActionResult> GetCurrentUserInfo(CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId)
        {
            return Unauthorized();
        }

        var result = await sender.Send(new GetUserByIdQuery(userId.ToString()), ct);

        return result.Match(
            response => Ok(response),
            Problem
        );
    }
}