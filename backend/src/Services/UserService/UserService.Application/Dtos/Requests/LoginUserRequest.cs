using System.ComponentModel.DataAnnotations;

namespace UserService.Application.Dtos.Requests;

public class LoginUserRequest
{
    [Required(ErrorMessage = "O email e obrigatorio !")]
    [EmailAddress(ErrorMessage = "Email invalido !")]
    [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "A senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
}