using BuildingBlock.Mediator;
using BuildingBlock.Validations;

namespace BuildingBlock.Abstractions;

public abstract class AggregateRoot : Entity
{
    /// <summary>
    /// Publica todos os domain events pendentes através do mediator
    /// </summary>
    /// <param name="mediator">Instância do mediator para publicação dos eventos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task PublishDomainEventsAsync(IMediator mediator, CancellationToken cancellationToken = default)
    {
        if (mediator == null)
            throw new ArgumentNullException(nameof(mediator));

        // Valida o agregado antes de publicar eventos
        ValidateAndThrow();

        var events = DomainEvents.ToList();

        ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            await mediator.PublishAsync(domainEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Sobrescreve o método de validação para incluir validações específicas de agregado
    /// </summary>
    /// <returns>ValidationHandler com os erros encontrados</returns>
    public override ValidationHandler Validate()
    {
        var validation = base.Validate();

        // Chama validações específicas do agregado
        ValidateAggregate(validation);

        return validation;
    }

    /// <summary>
    /// Método virtual para validações específicas do agregado.
    /// Deve ser implementado pelas classes derivadas para adicionar validações customizadas.
    /// </summary>
    /// <param name="validation">ValidationHandler para adicionar erros</param>
    protected virtual void ValidateAggregate(ValidationHandler validation)
    {
        // Implementação padrão vazia - classes derivadas podem sobrescrever
    }
}