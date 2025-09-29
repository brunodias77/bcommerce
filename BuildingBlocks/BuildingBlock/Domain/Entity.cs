using BuildingBlock.Mediator;
using BuildingBlock.Validations;

namespace BuildingBlock.Abstractions;

public abstract class Entity
{
    private readonly List<INotification> _domainEvents = new();

    /// <summary>
    /// Coleção somente leitura dos domain events pendentes
    /// </summary>
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adiciona um domain event à coleção
    /// </summary>
    /// <param name="domainEvent">O domain event a ser adicionado</param>
    protected void AddDomainEvent(INotification domainEvent)
    {
        if (domainEvent != null)
            _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove um domain event específico da coleção
    /// </summary>
    /// <param name="domainEvent">O domain event a ser removido</param>
    protected void RemoveDomainEvent(INotification domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Limpa todos os domain events da coleção
    /// </summary>
    protected internal void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Valida a entidade e retorna um ValidationHandler com os erros encontrados
    /// </summary>
    /// <returns>ValidationHandler com os erros de validação</returns>
    public virtual ValidationHandler Validate()
    {
        var validation = new ValidationHandler();
        
        // Chama validações específicas da entidade derivada
        ValidateEntity(validation);
        
        return validation;
    }

    /// <summary>
    /// Método virtual para validações específicas da entidade.
    /// Deve ser implementado pelas classes derivadas para adicionar validações customizadas.
    /// </summary>
    /// <param name="validation">ValidationHandler para adicionar erros</param>
    protected virtual void ValidateEntity(ValidationHandler validation)
    {
        // Implementação padrão vazia - classes derivadas podem sobrescrever
    }

    /// <summary>
    /// Valida a entidade e lança exceção se houver erros
    /// </summary>
    /// <exception cref="ValidationException">Lançada quando há erros de validação</exception>
    public virtual void ValidateAndThrow()
    {
        var validation = Validate();
        validation.ThrowIfHasErrors();
    }
}