namespace BuildingBlocks.Abstractions;

/// <summary>
/// Interface para handlers que processam requests com resposta
/// Cada request deve ter exatamente um handler correspondente
/// Implementa a lógica de negócio para queries e commands que retornam dados
/// </summary>
/// <typeparam name="TRequest">Tipo do request a ser processado</typeparam>
/// <typeparam name="TResponse">Tipo da resposta retornada</typeparam>
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Processa o request e retorna uma resposta
    /// </summary>
    /// <param name="request">Request a ser processado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Resposta do processamento</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Interface para handlers que processam requests sem resposta (void)
/// Cada request deve ter exatamente um handler correspondente
/// Implementa a lógica de negócio para commands que apenas executam ações
/// </summary>
/// <typeparam name="TRequest">Tipo do request a ser processado</typeparam>
public interface IRequestHandler<in TRequest> 
    where TRequest : IRequest
{
    /// <summary>
    /// Processa o request sem retornar resposta
    /// </summary>
    /// <param name="request">Request a ser processado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    Task Handle(TRequest request, CancellationToken cancellationToken);
}