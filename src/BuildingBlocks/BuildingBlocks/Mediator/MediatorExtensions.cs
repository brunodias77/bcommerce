using System.Reflection;
using BuildingBlocks.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Mediator;

/// <summary>
/// Extensões para configuração do padrão Mediator no container de DI
/// Registra automaticamente handlers, behaviors e serviços necessários
/// </summary>
public static class MediatorExtensions
{
    /// <summary>
    /// Configura o padrão Mediator no container de dependências
    /// 1. Registra os serviços principais (IMediator, IPublisher, IDomainEventDispatcher)
    /// 2. Escaneia assemblies em busca de handlers e behaviors
    /// 3. Registra automaticamente todos os componentes encontrados
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <param name="assemblies">Assemblies para escanear (usa assembly chamador se vazio)</param>
    /// <returns>Container de serviços para fluent interface</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Registra os serviços principais do Mediator
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<IPublisher>(provider => provider.GetRequiredService<IMediator>() as IPublisher 
            ?? throw new InvalidOperationException("Mediator deve implementar IPublisher"));
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Se nenhum assembly for fornecido, usa o assembly que está chamando
        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Registra todos os handlers e behaviors dos assemblies especificados
        foreach (var assembly in assemblies)
        {
            RegisterRequestHandlers(services, assembly);
            RegisterNotificationHandlers(services, assembly);
            RegisterPipelineBehaviors(services, assembly);
            RegisterRequestWrappers(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registra automaticamente todos os request handlers encontrados no assembly
    /// Busca classes que implementam IRequestHandler&lt;T&gt; ou IRequestHandler&lt;T,R&gt;
    /// </summary>
    private static void RegisterRequestHandlers(IServiceCollection services, Assembly assembly)
    {
        // Encontra todas as classes concretas que implementam interfaces de request handler
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
            .Where(t => ImplementsRequestHandlerInterface(t))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            // Obtém todas as interfaces de request handler implementadas
            var interfaces = handlerType.GetInterfaces()
                .Where(IsRequestHandlerInterface)
                .ToList();

            // Registra cada interface com sua implementação
            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }

    /// <summary>
    /// Registra automaticamente todos os notification handlers encontrados no assembly
    /// Busca classes que implementam INotificationHandler&lt;T&gt;
    /// </summary>
    private static void RegisterNotificationHandlers(IServiceCollection services, Assembly assembly)
    {
        // Encontra todas as classes concretas que implementam interfaces de notification handler
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
            .Where(t => ImplementsNotificationHandlerInterface(t))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            // Obtém todas as interfaces de notification handler implementadas
            var interfaces = handlerType.GetInterfaces()
                .Where(IsNotificationHandlerInterface)
                .ToList();

            // Registra cada interface com sua implementação
            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }

    /// <summary>
    /// Registra automaticamente todos os pipeline behaviors encontrados no assembly
    /// Busca classes que implementam IPipelineBehavior&lt;T,R&gt;
    /// </summary>
    private static void RegisterPipelineBehaviors(IServiceCollection services, Assembly assembly)
    {
        // Encontra todas as classes concretas que implementam interfaces de pipeline behavior
        var behaviorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
            .Where(t => ImplementsPipelineBehaviorInterface(t))
            .ToList();

        foreach (var behaviorType in behaviorTypes)
        {
            // Obtém todas as interfaces de pipeline behavior implementadas
            var interfaces = behaviorType.GetInterfaces()
                .Where(IsPipelineBehaviorInterface)
                .ToList();

            // Registra cada interface com sua implementação
            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, behaviorType);
            }
        }
    }

    /// <summary>
    /// Registra wrappers para requests sem resposta (IRequest)
    /// Permite que requests void passem pelo pipeline de behaviors
    /// Cria handlers que encapsulam o request original em RequestWrapper
    /// </summary>
    private static void RegisterRequestWrappers(IServiceCollection services, Assembly assembly)
    {
        // Encontra todos os tipos que implementam IRequest (sem resposta)
        var requestTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition)
            .Where(t => t.GetInterfaces().Any(i => i == typeof(IRequest)))
            .ToList();

        foreach (var requestType in requestTypes)
        {
            // Cria os tipos genéricos necessários para o wrapper
            var wrapperType = typeof(RequestWrapper<>).MakeGenericType(requestType);
            var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
            var wrapperHandlerType = typeof(RequestWrapperHandler<>).MakeGenericType(requestType);
            var wrapperHandlerInterface = typeof(IRequestHandler<,>).MakeGenericType(wrapperType, typeof(Unit));
            
            // Registra o wrapper handler que delega para o handler original
            services.AddScoped(wrapperHandlerInterface, provider =>
            {
                var originalHandler = provider.GetService(handlerType);
                if (originalHandler != null)
                {
                    return Activator.CreateInstance(wrapperHandlerType, originalHandler)!;
                }
                return null!;
            });
        }
    }

    /// <summary>
    /// Verifica se um tipo implementa alguma interface de request handler
    /// </summary>
    private static bool ImplementsRequestHandlerInterface(Type type)
    {
        return type.GetInterfaces().Any(IsRequestHandlerInterface);
    }

    /// <summary>
    /// Verifica se um tipo é uma interface de request handler
    /// Aceita IRequestHandler&lt;T&gt; (sem resposta) e IRequestHandler&lt;T,R&gt; (com resposta)
    /// </summary>
    private static bool IsRequestHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(IRequestHandler<,>) || 
               genericTypeDefinition == typeof(IRequestHandler<>);
    }

    /// <summary>
    /// Verifica se um tipo implementa alguma interface de notification handler
    /// </summary>
    private static bool ImplementsNotificationHandlerInterface(Type type)
    {
        return type.GetInterfaces().Any(IsNotificationHandlerInterface);
    }

    /// <summary>
    /// Verifica se um tipo é uma interface de notification handler
    /// Aceita apenas INotificationHandler&lt;T&gt;
    /// </summary>
    private static bool IsNotificationHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(INotificationHandler<>);
    }

    /// <summary>
    /// Verifica se um tipo implementa alguma interface de pipeline behavior
    /// </summary>
    private static bool ImplementsPipelineBehaviorInterface(Type type)
    {
        return type.GetInterfaces().Any(IsPipelineBehaviorInterface);
    }

    /// <summary>
    /// Verifica se um tipo é uma interface de pipeline behavior
    /// Aceita apenas IPipelineBehavior&lt;T,R&gt;
    /// </summary>
    private static bool IsPipelineBehaviorInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(IPipelineBehavior<,>);
    }
}