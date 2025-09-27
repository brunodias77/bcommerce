using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using UserService.Application.Dtos.Responses;

namespace UserService.Application.Commands.Users.ResetPassword;

public class ResetPasswordCommand : IRequest<Result<ResetPasswordResponse>>
{
    /// <summary>
    /// Token de redefinição de senha recebido por email
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Nova senha do usuário
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Confirmação da nova senha
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}