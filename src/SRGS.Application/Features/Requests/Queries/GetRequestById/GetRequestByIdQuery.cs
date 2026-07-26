using MediatR;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Requests.Queries.GetRequestById;

public sealed record GetRequestByIdQuery(int RequestId) : IRequest<Result<RequestDto>>;
