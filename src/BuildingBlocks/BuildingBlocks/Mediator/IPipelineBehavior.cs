using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Mediator;

/// <summary>
/// Interface para behaviors do pipeline que implementam preocupações transversais
/// Permite interceptar e processar requests antes/depois da execução do handler
/// Exemplos: validação, logging, cache, transações, tratamento de exceções
/// </summary>
/// <typeparam name="TRequest">Tipo do request sendo processado</typeparam>
/// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Executa o behavior no pipeline
    /// Pode executar lógica antes e/ou depois de chamar o próximo handler
    /// </summary>
    /// <param name="request">Request sendo processado</param>
    /// <param name="next">Delegate para o próximo handler no pipeline</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta do processamento</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Delegate que representa o próximo handler no pipeline
/// Usado pelos behaviors para continuar a execução da cadeia
/// </summary>
/// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();