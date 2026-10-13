using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SRGS.Application.Common.Interfaces;
using SRGS.Domain.Common.Results.Abstractions;

namespace SRGS.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest) return await next(ct);

        _logger.LogInformation("Checking cache for {RequestType} with key {CacheKey}", typeof(TRequest).Name, ((ICachedQuery)request).CacheKey);

        var result = await _cache.GetOrCreateAsync<TResponse>(
            key: cachedRequest.CacheKey,
            factory: _ => new ValueTask<TResponse>((TResponse)(object)null!),
            options: new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            },
            cancellationToken: ct
        );

        if (result is null)
        {
            result = await next(ct);

            if (result is IResult res && res.IsSuccess)
            {
                _logger.LogInformation("Caching result for {RequestName}", typeof(TRequest).Name);

                await _cache.SetAsync(
                    key: cachedRequest.CacheKey,
                    value: result,
                    options: new HybridCacheEntryOptions
                    {
                        Expiration = cachedRequest.Expiration
                    },
                    tags: cachedRequest.Tags,
                    cancellationToken: ct
                );
            }
        }

        return result;
    }
}