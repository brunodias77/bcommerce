namespace BuildingBlocks.Validations;

/// <summary>
/// Exceção lançada quando há erros de validação
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Lista de erros de validação
    /// </summary>
    public IReadOnlyList<Error> Errors { get; }

    /// <summary>
    /// Inicializa uma nova instância da ValidationException
    /// </summary>
    /// <param name="errors">Lista de erros de validação</param>
    public ValidationException(IReadOnlyList<Error> errors) 
        : base(CreateMessage(errors))
    {
        Errors = errors;
    }

    /// <summary>
    /// Inicializa uma nova instância da ValidationException com um único erro
    /// </summary>
    /// <param name="error">Erro de validação</param>
    public ValidationException(Error error) 
        : base(error.Message)
    {
        Errors = new List<Error> { error };
    }

    /// <summary>
    /// Inicializa uma nova instância da ValidationException com uma mensagem
    /// </summary>
    /// <param name="message">Mensagem do erro</param>
    public ValidationException(string message) 
        : base(message)
    {
        Errors = new List<Error> { new Error("VALIDATION_ERROR", message) };
    }

    /// <summary>
    /// Cria uma mensagem concatenada a partir da lista de erros
    /// </summary>
    /// <param name="errors">Lista de erros</param>
    /// <returns>Mensagem concatenada</returns>
    private static string CreateMessage(IReadOnlyList<Error> errors)
    {
        if (errors == null || errors.Count == 0)
            return "Erro de validação";

        if (errors.Count == 1)
            return errors[0].Message;

        return $"Múltiplos erros de validação: {string.Join("; ", errors.Select(e => e.Message))}";
    }
}