using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Dtos.Requests;

public class LoginUserRequest
{
    [Required(ErrorMessage = "O email e obrigatorio !")]
    [EmailAddress(ErrorMessage = "Email invalido !")]
    [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "A senha deve ter no mínimo 8 caracteres e incluir pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.")]
    public string Password { get; set; } = string.Empty;
}