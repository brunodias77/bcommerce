using System;

namespace UserService.Domain.Exceptions;

/// <summary>
/// Exception customizada para erros relacionados a operações de banco de dados
/// </summary>
public class DatabaseException : Exception
{
    /// <summary>
    /// Código do erro específico
    /// </summary>
    public string? ErrorCode { get; }
    
    /// <summary>
    /// Nome da tabela onde ocorreu o erro
    /// </summary>
    public string? TableName { get; }
    
    /// <summary>
    /// Operação que estava sendo executada quando o erro ocorreu
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// Construtor padrão
    /// </summary>
    public DatabaseException() : base("Erro de banco de dados")
    {
    }

    /// <summary>
    /// Construtor com mensagem personalizada
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public DatabaseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Construtor com mensagem e exceção interna
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    /// <param name="innerException">Exceção interna que causou este erro</param>
    public DatabaseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Construtor completo com todas as propriedades
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    /// <param name="errorCode">Código do erro</param>
    /// <param name="tableName">Nome da tabela</param>
    /// <param name="operation">Operação sendo executada</param>
    /// <param name="innerException">Exceção interna</param>
    public DatabaseException(string message, string? errorCode = null, string? tableName = null, string? operation = null, Exception? innerException = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        TableName = tableName;
        Operation = operation;
    }

    /// <summary>
    /// Cria uma DatabaseException para erros de inserção
    /// </summary>
    /// <param name="tableName">Nome da tabela onde ocorreu o erro</param>
    /// <param name="details">Detalhes do erro</param>
    /// <returns>Nova instância de DatabaseException</returns>
    public static DatabaseException ForInsertError(string tableName, string details)
    {
        return new DatabaseException(
            $"Erro ao inserir dados na tabela '{tableName}': {details}",
            "INSERT_ERROR",
            tableName,
            "INSERT"
        );
    }

    /// <summary>
    /// Cria uma DatabaseException para erros de atualização
    /// </summary>
    /// <param name="tableName">Nome da tabela onde ocorreu o erro</param>
    /// <param name="details">Detalhes do erro</param>
    /// <returns>Nova instância de DatabaseException</returns>
    public static DatabaseException ForUpdateError(string tableName, string details)
    {
        return new DatabaseException(
            $"Erro ao atualizar dados na tabela '{tableName}': {details}",
            "UPDATE_ERROR",
            tableName,
            "UPDATE"
        );
    }

    /// <summary>
    /// Cria uma DatabaseException para erros de exclusão
    /// </summary>
    /// <param name="tableName">Nome da tabela onde ocorreu o erro</param>
    /// <param name="details">Detalhes do erro</param>
    /// <returns>Nova instância de DatabaseException</returns>
    public static DatabaseException ForDeleteError(string tableName, string details)
    {
        return new DatabaseException(
            $"Erro ao excluir dados da tabela '{tableName}': {details}",
            "DELETE_ERROR",
            tableName,
            "DELETE"
        );
    }

    /// <summary>
    /// Cria uma DatabaseException para erros de conexão
    /// </summary>
    /// <param name="details">Detalhes do erro de conexão</param>
    /// <returns>Nova instância de DatabaseException</returns>
    public static DatabaseException ForConnectionError(string details)
    {
        return new DatabaseException(
            $"Erro de conexão com o banco de dados: {details}",
            "CONNECTION_ERROR",
            operation: "CONNECTION"
        );
    }

    /// <summary>
    /// Cria uma DatabaseException para violações de constraint
    /// </summary>
    /// <param name="tableName">Nome da tabela onde ocorreu a violação</param>
    /// <param name="constraint">Nome da constraint violada</param>
    /// <param name="details">Detalhes da violação</param>
    /// <returns>Nova instância de DatabaseException</returns>
    public static DatabaseException ForConstraintViolation(string tableName, string constraint, string details)
    {
        return new DatabaseException(
            $"Violação de constraint '{constraint}' na tabela '{tableName}': {details}",
            "CONSTRAINT_VIOLATION",
            tableName,
            "CONSTRAINT_CHECK"
        );
    }

    /// <summary>
    /// Retorna uma representação em string da exceção com informações detalhadas
    /// </summary>
    /// <returns>String formatada com detalhes da exceção</returns>
    public override string ToString()
    {
        var details = base.ToString();
        
        if (!string.IsNullOrEmpty(ErrorCode))
            details += $"\nCódigo do Erro: {ErrorCode}";
            
        if (!string.IsNullOrEmpty(TableName))
            details += $"\nTabela: {TableName}";
            
        if (!string.IsNullOrEmpty(Operation))
            details += $"\nOperação: {Operation}";
            
        return details;
    }
}