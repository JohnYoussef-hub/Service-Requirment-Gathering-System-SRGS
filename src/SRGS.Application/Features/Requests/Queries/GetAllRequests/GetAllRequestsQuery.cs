using MediatR;
using SRGS.Application.Common.Interfaces;
using SRGS.Application.Features.Requests.Dtos;
using SRGS.Domain.Common.Results;

namespace SRGS.Application.Features.Requests.Queries.GetAllRequests;

public sealed record GetAllRequestsQuery : ICachedQuery<Result<List<RequestDto>>>
{
    public string CacheKey => "Requests-All";

    public string[] Tags => ["request"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
