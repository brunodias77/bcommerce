using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that handles and logs exceptions
/// </summary>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        try
        {
            return await next();
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions without logging as they are expected
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request {RequestName} was cancelled", requestName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while handling {RequestName}: {ExceptionMessage}",
                requestName, ex.Message);

            // You could wrap the exception here or add correlation IDs
            throw new ApplicationException($"An error occurred while processing {requestName}", ex);
        }
    }
}