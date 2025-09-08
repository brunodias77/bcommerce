using BuildingBlocks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Mediator;

/// <summary>
/// Implementação principal do padrão Mediator
/// Atua como intermediário entre controllers e handlers, gerenciando o pipeline de execução
/// Implementa tanto IMediator (para requests) quanto IPublisher (para eventos)
/// </summary>
public class Mediator : IMediator, IPublisher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Construtor que recebe o service provider para resolução de dependências
    /// </summary>
    /// <param name="serviceProvider">Provider para resolução de handlers e behaviors</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Envia um request que retorna uma resposta, executando o pipeline completo
    /// 1. Resolve o handler apropriado para o request
    /// 2. Obtém todos os pipeline behaviors registrados
    /// 3. Constrói o pipeline de execução (behaviors + handler)
    /// 4. Executa o pipeline e retorna a resposta
    /// </summary>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        
        // Resolve o handler específico para este tipo de request
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handler = _serviceProvider.GetService(handlerType);
        
        if (handler == null)
            throw new InvalidOperationException($"Nenhum handler encontrado para o request do tipo {requestType.Name}");

        // Obtém todos os pipeline behaviors registrados para este tipo de request/response
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = _serviceProvider.GetServices(behaviorType).Reverse().ToArray();

        // Constrói o pipeline começando com o handler final
        RequestHandlerDelegate<TResponse> handlerDelegate = async () =>
        {
            var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));
            
            if (handleMethod == null)
                throw new InvalidOperationException($"Método Handle não encontrado no handler para {requestType.Name}");

            // Invoca o método Handle do handler via reflection
            var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
            
            // Trata diferentes tipos de retorno do handler
            return result switch
            {
                Task<TResponse> taskResult => await taskResult,
                TResponse directResult => directResult,
                _ => throw new InvalidOperationException($"Handler para {requestType.Name} retornou tipo inesperado: {result?.GetType()}")
            };
        };

        // Envolve o handler com os behaviors (em ordem reversa para manter sequência correta)
        // Cada behavior pode executar lógica antes e depois do próximo na cadeia
        foreach (var behavior in behaviors)
        {
            var currentHandler = handlerDelegate;
            var behaviorInstance = behavior;
            
            handlerDelegate = () =>
            {
                var handleMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle));
                
                if (handleMethod == null)
                    throw new InvalidOperationException($"Método Handle não encontrado no behavior {behavior.GetType().Name}");

                // Invoca o behavior passando o request e o próximo handler na cadeia
                var result = handleMethod.Invoke(behaviorInstance, new object[] { request, currentHandler, cancellationToken });
                
                return result switch
                {
                    Task<TResponse> taskResult => taskResult,
                    _ => throw new InvalidOperationException($"Behavior {behavior.GetType().Name} retornou tipo inesperado")
                };
            };
        }

        // Executa o pipeline completo (behaviors + handler)
        return await handlerDelegate();
    }

    /// <summary>
    /// Envia um request que não retorna resposta (void)
    /// Para suportar pipeline behaviors, o request é envolvido em um RequestWrapper&lt;Unit&gt;
    /// Isso permite que commands sem retorno passem pelo mesmo pipeline que requests com retorno
    /// </summary>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        
        // Resolve o handler específico para este tipo de request
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        var handler = _serviceProvider.GetService(handlerType);
        
        if (handler == null)
            throw new InvalidOperationException($"Nenhum handler encontrado para o request do tipo {requestType.Name}");

        // Para requests void, precisamos envolvê-los para suportar pipeline behaviors
        // O wrapper converte IRequest em IRequest<Unit> permitindo uso do pipeline
        var wrapperType = typeof(RequestWrapper<>).MakeGenericType(requestType);
        var wrapper = Activator.CreateInstance(wrapperType, request);
        
        if (wrapper is IRequest<Unit> unitRequest)
        {
            // Usa o método Send com retorno, aproveitando todo o pipeline
            await Send(unitRequest, cancellationToken);
        }
        else
        {
            // Fallback para execução direta (sem pipeline behaviors)
            var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest>.Handle));
            
            if (handleMethod == null)
                throw new InvalidOperationException($"Método Handle não encontrado no handler para {requestType.Name}");

            var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
            
            if (result is Task task)
                await task;
            else
                throw new InvalidOperationException($"Handler para {requestType.Name} não retornou uma Task");
        }
    }

    /// <summary>
    /// Publica uma notificação tipada para todos os handlers registrados
    /// Versão genérica que garante type safety em tempo de compilação
    /// </summary>
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        await PublishInternal(notification, cancellationToken);
    }

    /// <summary>
    /// Publica uma notificação não-tipada para todos os handlers registrados
    /// Versão não-genérica para casos onde o tipo é determinado em runtime
    /// Valida se o objeto implementa INotification antes de publicar
    /// </summary>
    public async Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification is INotification domainNotification)
        {
            await PublishInternal(domainNotification, cancellationToken);
        }
        else
        {
            throw new ArgumentException("A notificação deve implementar INotification", nameof(notification));
        }
    }

    /// <summary>
    /// Método interno que executa a publicação da notificação
    /// 1. Resolve todos os handlers registrados para o tipo da notificação
    /// 2. Invoca cada handler de forma assíncrona
    /// 3. Aguarda a conclusão de todos os handlers em paralelo
    /// </summary>
    private async Task PublishInternal(INotification notification, CancellationToken cancellationToken)
    {
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        
        // Resolve todos os handlers registrados para este tipo de notificação
        var handlers = _serviceProvider.GetServices(handlerType);
        
        var tasks = new List<Task>();
        
        // Invoca cada handler de forma assíncrona
        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<INotification>.Handle));
            
            if (handleMethod != null)
            {
                // Invoca o método Handle do handler via reflection
                var result = handleMethod.Invoke(handler, new object[] { notification, cancellationToken });
                
                if (result is Task task)
                {
                    tasks.Add(task);
                }
            }
        }
        
        // Aguarda a conclusão de todos os handlers em paralelo
        // Isso permite que múltiplos handlers processem o evento simultaneamente
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }
}