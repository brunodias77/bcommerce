using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Dtos.Requests;

/// <summary>
/// DTO para solicitação de redefinição de senha
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Token de redefinição de senha recebido por email
    /// </summary>
    [Required(ErrorMessage = "O token é obrigatório")]
    public string? Token { get; set; }

    /// <summary>
    /// Nova senha do usuário
    /// </summary>
    [Required(ErrorMessage = "A nova senha é obrigatória")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "A senha deve ter no mínimo 8 caracteres e incluir pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.")]
    public string? NewPassword { get; set; }

    /// <summary>
    /// Confirmação da nova senha
    /// </summary>
    [Required(ErrorMessage = "A confirmação da senha é obrigatória")]
    [Compare("NewPassword", ErrorMessage = "A confirmação da senha deve ser igual à nova senha")]
    public string? ConfirmPassword { get; set; }
}