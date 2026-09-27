using MediatR;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Identity.Queries.GenerateTokens;

public record GenerateTokenQuery(
    string Username,
    string Password) : IRequest<Result<TokenResponse>>;