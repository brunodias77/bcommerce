using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Dtos.Requests;

/// <summary>
/// DTO para solicitação de esqueci minha senha
/// </summary>
public class ForgetPasswordRequest
{
    /// <summary>
    /// Email do usuário que esqueceu a senha
    /// </summary>
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
    public string? Email { get; set; }
}