using BuildingBlocks.Abstractions;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command handlers in database transactions
/// </summary>
// public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//     where TRequest : IRequest<TResponse>
// {
//     private readonly DbContext _dbContext;
//     private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
//
//     public TransactionBehavior(DbContext dbContext, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
//     {
//         _dbContext = dbContext;
//         _logger = logger;
//     }
//
//     public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//     {
//         var requestName = typeof(TRequest).Name;
//         
//         // Only apply transactions to commands (not queries)
//         if (!IsCommand(requestName))
//         {
//             return await next();
//         }
//
//         // Check if we're already in a transaction
//         if (_dbContext.Database.CurrentTransaction != null)
//         {
//             return await next();
//         }
//
//         using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
//         
//         try
//         {
//             _logger.LogDebug("Starting transaction for {RequestName}", requestName);
//             
//             var response = await next();
//             
//             await transaction.CommitAsync(cancellationToken);
//             _logger.LogDebug("Transaction committed for {RequestName}", requestName);
//             
//             return response;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Transaction rolled back for {RequestName}", requestName);
//             await transaction.RollbackAsync(cancellationToken);
//             throw;
//         }
//     }
//
//     private static bool IsCommand(string requestName)
//     {
//         return requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase);
//     }
// }