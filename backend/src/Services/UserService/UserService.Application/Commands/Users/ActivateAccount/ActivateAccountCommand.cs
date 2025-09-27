using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;

namespace UserService.Application.Commands.Users.ActivateAccount;

/// <summary>
/// Comando para ativar conta de usuário usando token de ativação
/// </summary>
/// <param name="Token">Token de ativação enviado por email</param>
public record ActivateAccountCommand(string Token) : IRequest<Result>;