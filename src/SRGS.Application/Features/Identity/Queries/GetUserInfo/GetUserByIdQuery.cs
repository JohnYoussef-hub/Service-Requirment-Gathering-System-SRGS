using MechanicShop.Application.Features.Identity.Dtos;

using MediatR;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Identity.Queries.GetUserInfo;

public sealed record GetUserByIdQuery(string? UserId) : IRequest<Result<AppUserDto>>;