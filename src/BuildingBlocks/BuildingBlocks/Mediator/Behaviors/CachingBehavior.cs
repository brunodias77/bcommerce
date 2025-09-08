using System.Text.Json;
using BuildingBlocks.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that caches query results
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(5);

    public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Only cache queries
        if (!IsQuery(requestName))
        {
            return await next();
        }

        // Check if request implements ICacheable for custom cache settings
        if (request is not ICacheable cacheableRequest)
        {
            return await next();
        }

        var cacheKey = GenerateCacheKey(request);
        
        if (_cache.TryGetValue(cacheKey, out TResponse cachedResponse))
        {
            _logger.LogDebug("Cache hit for {RequestName} with key {CacheKey}", requestName, cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for {RequestName} with key {CacheKey}", requestName, cacheKey);
        
        var response = await next();
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheableRequest.CacheDuration ?? _defaultCacheDuration
        };
        
        _cache.Set(cacheKey, response, cacheOptions);
        _logger.LogDebug("Response cached for {RequestName} with key {CacheKey}", requestName, cacheKey);
        
        return response;
    }

    private static bool IsQuery(string requestName)
    {
        return requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateCacheKey(TRequest request)
    {
        var requestType = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        var hash = requestJson.GetHashCode();
        return $"{requestType}_{hash}";
    }
}

/// <summary>
/// Interface for cacheable requests
/// </summary>
public interface ICacheable
{
    TimeSpan? CacheDuration { get; }
}