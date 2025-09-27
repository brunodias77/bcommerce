using System.ComponentModel.DataAnnotations;

namespace UserService.Api.DTOs.Requests;

/// <summary>
/// Request para ativação de conta de usuário
/// </summary>
public class ActivateAccountRequest
{
    /// <summary>
    /// Token de ativação enviado por email
    /// </summary>
    [Required(ErrorMessage = "Token de ativação é obrigatório.")]
    [StringLength(500, ErrorMessage = "Token deve ter no máximo 500 caracteres.")]
    public string Token { get; set; } = string.Empty;
}