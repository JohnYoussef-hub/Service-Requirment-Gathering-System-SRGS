using MediatR;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Requests.Queries.GetRequestById;

public sealed record GetRequestByIdQuery(int RequestId) : ICachedQuery<Result<RequestDto>>
{
    public string CacheKey => $"Request-{RequestId}";

    public string[] Tags => ["request"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(15);
}