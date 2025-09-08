using BuildingBlocks.Abstractions;
using BuildingBlocks.Validations;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests before handling
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var notification = new ValidationHandler();

        // Se o request implementa validação, validar
        if (request is IValidatable validatable)
        {
            validatable.Validate(notification);
        }
        
        // Se o request é um ValueObject ou Entity, validar também
        if (request is ValueObject valueObject)
        {
            valueObject.Validate(notification);
        }

        if (notification.HasErrors)
        {
            var errors = string.Join(", ", notification.Errors.Select(e => $"{e.Code}: {e.Message}"));
            _logger.LogWarning("Validation failed for {RequestName}: {Errors}", requestName, errors);
            throw new ValidationException(notification.Errors);
        }

        return await next();
    }
}

/// <summary>
/// Interface for validatable requests
/// </summary>
public interface IValidatable
{
    void Validate(IValidationHandler handler);
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyList<Error> Errors { get; }

    public ValidationException(IReadOnlyList<Error> errors) 
        : base($"Validation failed: {string.Join(", ", errors.Select(e => e.Message))}")
    {
        Errors = errors;
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<Error> { new("VALIDATION_ERROR", message) };
    }
}