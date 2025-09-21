using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildingBlocks.Mediator;

/// <summary>
/// Classe base para representar o resultado de uma operação
/// Encapsula sucesso/falha e lista de erros
/// </summary>
public class Result
{
    protected Result(bool isSuccess, List<string> errors)
    {
        if (isSuccess && errors.Any())
        {
            throw new InvalidOperationException("Um resultado de sucesso não pode ter erros.");
        }
        
        if (!isSuccess && !errors.Any())
        {
            throw new InvalidOperationException("Um resultado de falha deve ter pelo menos um erro.");
        }

        IsSuccess = isSuccess;
        Errors = errors ?? new List<string>();
    }

    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação falhou
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Lista de erros da operação
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Primeiro erro da lista (para conveniência)
    /// </summary>
    public string FirstError => Errors.FirstOrDefault() ?? string.Empty;

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    public static Result Success() => new(true, new List<string>());

    /// <summary>
    /// Cria um resultado de falha com um erro
    /// </summary>
    public static Result Failure(string error) => new(false, new List<string> { error });

    /// <summary>
    /// Cria um resultado de falha com múltiplos erros
    /// </summary>
    public static Result Failure(List<string> errors) => new(false, errors);

    /// <summary>
    /// Conversão implícita de string para Result (falha)
    /// </summary>
    public static implicit operator Result(string error) => Failure(error);

    /// <summary>
    /// Conversão implícita de lista de erros para Result (falha)
    /// </summary>
    public static implicit operator Result(List<string> errors) => Failure(errors);
}

/// <summary>
/// Classe genérica para representar o resultado de uma operação com valor de retorno
/// Herda de Result e adiciona suporte a valores tipados
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected Result(bool isSuccess, TValue? value, List<string> errors)
        : base(isSuccess, errors)
    {
        if (isSuccess && value is null)
        {
            throw new InvalidOperationException("Um resultado de sucesso deve ter um valor.");
        }
        
        if (!isSuccess && value is not null && !EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
        {
            throw new InvalidOperationException("Um resultado de falha não pode ter um valor.");
        }

        _value = value;
    }

    /// <summary>
    /// Valor do resultado (disponível apenas em caso de sucesso)
    /// </summary>
    public TValue Value => _value!;

    /// <summary>
    /// Cria um resultado de sucesso com valor
    /// </summary>
    public static Result<TValue> Success(TValue value) => new(true, value, new List<string>());

    /// <summary>
    /// Cria um resultado de falha com um erro
    /// </summary>
    public static new Result<TValue> Failure(string error) => new(false, default(TValue), new List<string> { error });

    /// <summary>
    /// Cria um resultado de falha com múltiplos erros
    /// </summary>
    public static new Result<TValue> Failure(List<string> errors) => new(false, default(TValue), errors);

    /// <summary>
    /// Conversão implícita de valor para Result<TValue> (sucesso)
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Conversão implícita de string para Result<TValue> (falha)
    /// </summary>
    public static implicit operator Result<TValue>(string error) => Failure(error);

    /// <summary>
    /// Conversão implícita de lista de erros para Result<TValue> (falha)
    /// </summary>
    public static implicit operator Result<TValue>(List<string> errors) => Failure(errors);


}