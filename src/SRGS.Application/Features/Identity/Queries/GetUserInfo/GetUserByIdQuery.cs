using MediatR;
using SRGS.Application.Features.Identity.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Identity.Queries.GetUserInfo;

public sealed record GetUserByIdQuery(string? UserId) : IRequest<Result<AppUserDto>>;