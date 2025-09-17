namespace BuildingBlocks.Validations;

public static class ValidationHandlerExtensions
{
    /// <summary>
    /// Adiciona um erro com código e mensagem ao handler de validação
    /// </summary>
    /// <param name="handler">O handler de validação</param>
    /// <param name="code">Código do erro</param>
    /// <param name="message">Mensagem do erro</param>
    /// <returns>O handler de validação para encadeamento</returns>
    public static IValidationHandler AddError(this IValidationHandler handler, string code, string message)
    {
        return handler.Add(new Error(code, message));
    }

    /// <summary>
    /// Adiciona um erro apenas com mensagem ao handler de validação
    /// </summary>
    /// <param name="handler">O handler de validação</param>
    /// <param name="message">Mensagem do erro</param>
    /// <returns>O handler de validação para encadeamento</returns>
    public static IValidationHandler AddError(this IValidationHandler handler, string message)
    {
        return handler.Add(new Error("VALIDATION_ERROR", message));
    }

    /// <summary>
    /// Lança uma exceção se houver erros no handler de validação
    /// </summary>
    /// <param name="handler">O handler de validação</param>
    /// <exception cref="ValidationException">Lançada quando há erros de validação</exception>
    public static void ThrowIfHasErrors(this IValidationHandler handler)
    {
        if (handler.HasErrors)
        {
            throw new ValidationException(handler.Errors);
        }
    }
}