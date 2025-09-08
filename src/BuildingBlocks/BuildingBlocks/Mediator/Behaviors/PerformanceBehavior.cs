using System.Diagnostics;
using BuildingBlocks.Abstractions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that logs performance metrics and warns about slow requests
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly int _slowRequestThresholdMs;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _slowRequestThresholdMs = 5000; // 5 seconds default
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > _slowRequestThresholdMs)
            {
                _logger.LogWarning("Slow request detected: {RequestName} took {ElapsedMilliseconds}ms", 
                    requestName, elapsedMs);
            }
            else
            {
                _logger.LogDebug("Request {RequestName} completed in {ElapsedMilliseconds}ms", 
                    requestName, elapsedMs);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestName} failed after {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}