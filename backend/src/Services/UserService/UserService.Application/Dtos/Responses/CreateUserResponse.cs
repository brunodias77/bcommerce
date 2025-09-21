namespace UserService.Application.Dtos.Responses;

/// <summary>
/// DTO para resposta de criação de usuário
/// </summary>
public class CreateUserResponse
{
    /// <summary>
    /// ID único do usuário criado
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email do usuário criado
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Primeiro nome do usuário
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Último nome do usuário
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem de sucesso
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora da criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool Success { get; set; } = true;
}