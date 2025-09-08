namespace BuildingBlocks.Abstractions;

/// <summary>
/// Interface principal do padrão Mediator que define o contrato para envio de requests
/// O Mediator atua como um intermediário entre os controllers e os handlers,
/// promovendo baixo acoplamento e separação de responsabilidades
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Envia um request que retorna uma resposta do tipo TResponse
    /// Utilizado para queries e commands que precisam retornar dados
    /// </summary>
    /// <typeparam name="TResponse">Tipo da resposta esperada</typeparam>
    /// <param name="request">Request a ser processado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    /// <returns>Resposta do tipo TResponse</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Envia um request que não retorna resposta (void)
    /// Utilizado para commands que apenas executam uma ação sem retornar dados
    /// </summary>
    /// <param name="request">Request a ser processado</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}
