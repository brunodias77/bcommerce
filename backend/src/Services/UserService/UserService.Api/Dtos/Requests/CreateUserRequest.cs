using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Dtos.Requests;

/// <summary>
/// DTO para requisição de criação de usuário
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Primeiro nome do usuário
    /// </summary>
    [Required(ErrorMessage = "O primeiro nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O primeiro nome deve ter entre 2 e 100 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Último nome do usuário
    /// </summary>
    [Required(ErrorMessage = "O último nome é obrigatório")]
    [StringLength(155, MinimumLength = 2, ErrorMessage = "O último nome deve ter entre 2 e 155 caracteres")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 255 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "A senha deve ter no mínimo 8 caracteres e incluir pelo menos uma letra maiúscula, uma minúscula, um número e um caractere especial.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o usuário optou por receber newsletter
    /// </summary>
    [Required(ErrorMessage = "A opção de newsletter é obrigatória")]
    public bool NewsletterOptIn { get; set; }
}