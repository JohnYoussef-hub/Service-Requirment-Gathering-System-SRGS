using MechanicShop.Application.Features.Identity;

using MediatR;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Identity.Queries.GenerateTokens;

public record GenerateTokenQuery(
    string Email,
    string Password) : IRequest<Result<TokenResponse>>;