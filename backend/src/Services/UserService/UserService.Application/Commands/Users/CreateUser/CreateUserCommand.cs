using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions;

namespace UserService.Application.Commands.Users.CreateUser;

/// <summary>
/// Command para criar um novo usuário no sistema
/// Implementa IRequest<Unit> para commands sem retorno
/// </summary>
public class CreateUserCommand : IRequest<Unit>
{
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário (deve ser único no sistema)
    /// </summary>
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário (será criptografada antes de ser armazenada)
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do usuário
    /// </summary>
    [Required(ErrorMessage = "O telefone é obrigatório")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "O telefone deve ter entre 10 e 20 caracteres")]
    public string Phone { get; set; } = string.Empty;
}
