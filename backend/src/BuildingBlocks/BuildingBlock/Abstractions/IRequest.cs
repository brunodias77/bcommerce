namespace BuildingBlocks.Abstractions;

/// <summary>
/// Interface base para requests que retornam um resultado do tipo TResponse
/// Implementada por queries e commands que precisam retornar dados
/// Exemplo: GetUserByIdQuery, CreateUserCommand
/// </summary>
/// <typeparam name="TResponse">Tipo do resultado retornado pelo request</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Interface base para requests que não retornam resultado (void)
/// Implementada por commands que apenas executam uma ação
/// Exemplo: DeleteUserCommand, UpdateUserPasswordCommand
/// </summary>
public interface IRequest
{
}