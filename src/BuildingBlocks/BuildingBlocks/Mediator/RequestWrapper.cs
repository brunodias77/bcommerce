using BuildingBlocks.Abstractions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Mediator;

/// <summary>
/// Wrapper que encapsula requests sem resposta para suportar pipeline behaviors
/// Converte IRequest (sem resposta) em IRequest&lt;Unit&gt; (com resposta Unit)
/// Isso permite que requests void passem pelo pipeline de behaviors
/// que esperam requests com tipo de retorno definido
/// </summary>
public class RequestWrapper<TRequest> : IRequest<Unit>
    where TRequest : IRequest
{
    /// <summary>
    /// O request original que está sendo encapsulado
    /// </summary>
    public TRequest Request { get; }

    /// <summary>
    /// Construtor que recebe o request original para encapsulamento
    /// </summary>
    /// <param name="request">O request original sem resposta</param>
    public RequestWrapper(TRequest request)
    {
        Request = request;
    }
}

/// <summary>
/// Handler que encapsula handlers de requests sem resposta
/// Permite que requests void (IRequest) passem pelo pipeline de behaviors
/// Converte a execução para retornar Unit em vez de void
/// </summary>
internal class RequestWrapperHandler<TRequest> : IRequestHandler<RequestWrapper<TRequest>, Unit>
    where TRequest : IRequest
{
    private readonly IRequestHandler<TRequest> _handler;

    /// <summary>
    /// Construtor que recebe o handler original para delegação
    /// </summary>
    /// <param name="handler">Handler original que processa o request</param>
    public RequestWrapperHandler(IRequestHandler<TRequest> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    /// <summary>
    /// Executa o handler original e retorna Unit
    /// 1. Extrai o request original do wrapper
    /// 2. Delega a execução para o handler original
    /// 3. Retorna Unit.Value para satisfazer a interface
    /// </summary>
    public async Task<Unit> Handle(RequestWrapper<TRequest> request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Delega a execução para o handler original
        await _handler.Handle(request.Request, cancellationToken);
        
        // Retorna Unit para satisfazer a interface IRequestHandler<,>
        return Unit.Value;
    }
}
