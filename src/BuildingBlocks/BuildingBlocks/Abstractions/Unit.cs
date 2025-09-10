namespace BuildingBlocks.Abstractions;

/// <summary>
/// Representa um tipo void, já que Void não é um tipo de retorno válido em C#
/// Utilizado pelo padrão Mediator para requests que não retornam valor
/// Permite que commands sem retorno sejam tratados de forma consistente no pipeline
/// </summary>
public struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Valor padrão e único do tipo Unit
    /// Usado como retorno para operações que não precisam retornar dados
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Task pré-criada do tipo Unit para evitar alocações desnecessárias
    /// Utilizada por handlers que retornam Task&lt;Unit&gt;
    /// </summary>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

    /// <summary>
    /// Compara duas instâncias de Unit (sempre retorna true pois só existe um valor)
    /// </summary>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Compara com qualquer objeto (retorna true apenas se for Unit)
    /// </summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Retorna hash code constante (0) pois todas as instâncias são iguais
    /// </summary>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Operador de igualdade (sempre retorna true)
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Operador de desigualdade (sempre retorna false)
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;

    /// <summary>
    /// Representação textual do Unit (parênteses vazios)
    /// </summary>
    public override string ToString() => "()";
}