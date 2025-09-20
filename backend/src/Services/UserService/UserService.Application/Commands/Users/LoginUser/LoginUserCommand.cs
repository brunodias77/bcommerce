using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Abstractions;

namespace UserService.Application.Commands.Users.LoginUser;

/// <summary>
/// Command para autenticar um usuário no sistema
/// Implementa IRequest<LoginUserResponse> para retornar dados de autenticação
/// </summary>
public class LoginUserCommand : IRequest<LoginUserResponse>
{
    /// <summary>
    /// Email do usuário para autenticação
    /// </summary>
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário para autenticação
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "A senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO de resposta para o comando de login
/// Contém os dados de autenticação JWT retornados pelo Keycloak
/// </summary>
public class LoginUserResponse
{
    /// <summary>
    /// Token de acesso JWT
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token de refresh para renovação
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Tempo de expiração do token em segundos
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Tipo do token (geralmente "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Escopo do token
    /// </summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a autenticação foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem de resultado da autenticação
    /// </summary>
    public string Message { get; set; } = string.Empty;
}