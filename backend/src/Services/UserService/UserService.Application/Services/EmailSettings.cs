namespace UserService.Application.Services;

/// <summary>
/// Configurações para o serviço de email
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Seção de configuração no appsettings.json
    /// </summary>
    public const string SectionName = "EmailSettings";

    /// <summary>
    /// Servidor SMTP
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// Porta do servidor SMTP
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Indica se deve usar SSL/TLS
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Nome de usuário para autenticação SMTP
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha para autenticação SMTP
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Email do remetente
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nome do remetente
    /// </summary>
    public string FromName { get; set; } = "B-Commerce";

    /// <summary>
    /// URL base da aplicação para links nos emails
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timeout para envio de email em segundos
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Indica se o serviço de email está habilitado
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Tempo de expiração do token de ativação em horas
    /// </summary>
    public int ActivationTokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// Tempo de expiração do token de redefinição de senha em horas
    /// </summary>
    public int PasswordResetTokenExpirationHours { get; set; } = 2;
}