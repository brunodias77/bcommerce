namespace BuildingBlocks.Abstractions;

/// <summary>
/// Interface para despachador de eventos de domínio dos aggregate roots
/// Responsável por extrair e publicar todos os eventos acumulados nos agregados
/// Geralmente chamado após operações de persistência para garantir consistência
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Despacha todos os eventos de domínio de um único aggregate root
    /// Extrai os eventos do agregado, limpa a lista e publica cada evento
    /// </summary>
    /// <param name="aggregate">Aggregate root que contém os eventos a serem despachados</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    Task DispatchEventsAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Despacha todos os eventos de domínio de múltiplos aggregate roots
    /// Coleta eventos de todos os agregados e os publica em sequência
    /// </summary>
    /// <param name="aggregates">Coleção de aggregate roots que contêm eventos</param>
    /// <param name="cancellationToken">Token para cancelamento da operação</param>
    Task DispatchEventsAsync(IEnumerable<AggregateRoot> aggregates, CancellationToken cancellationToken = default);
}