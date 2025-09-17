using BuildingBlocks.Abstractions;

namespace BuildingBlocks.Mediator;

/// <summary>
/// Serviço responsável por despachar eventos de domínio dos aggregate roots
/// Extrai eventos acumulados nos agregados e os publica através do IPublisher
/// Garante que eventos sejam processados após operações de persistência
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    /// <summary>
    /// Construtor que recebe o publisher para publicação dos eventos
    /// </summary>
    /// <param name="publisher">Publisher responsável por distribuir os eventos</param>
    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <summary>
    /// Despacha todos os eventos de um único aggregate root
    /// 1. Extrai todos os eventos acumulados no agregado
    /// 2. Limpa a lista de eventos do agregado
    /// 3. Publica cada evento sequencialmente
    /// </summary>
    public async Task DispatchEventsAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        // Cria uma cópia da lista de eventos para evitar modificações durante iteração
        var events = aggregate.Events.ToList();

        // Limpa os eventos do agregado para evitar republicação
        aggregate.ClearEvents();

        // Publica cada evento sequencialmente
        foreach (var domainEvent in events)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Despacha todos os eventos de múltiplos aggregate roots
    /// 1. Coleta todos os eventos de todos os agregados
    /// 2. Limpa os eventos de todos os agregados
    /// 3. Publica todos os eventos sequencialmente
    /// </summary>
    public async Task DispatchEventsAsync(IEnumerable<AggregateRoot> aggregates,
        CancellationToken cancellationToken = default)
    {
        var allEvents = new List<DomainEvent>();

        // Coleta todos os eventos de todos os agregados
        foreach (var aggregate in aggregates)
        {
            allEvents.AddRange(aggregate.Events);
            aggregate.ClearEvents();
        }

        // Publica todos os eventos coletados sequencialmente
        foreach (var domainEvent in allEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}