namespace UserService.Domain.Enums;

/// <summary>
/// Tipos de tokens de usuário suportados pelo sistema
/// </summary>
public enum UserTokenType
{
    /// <summary>
    /// Token de refresh para renovação de acesso
    /// </summary>
    Refresh,
    
    /// <summary>
    /// Token para verificação de email
    /// </summary>
    EmailVerification,
    
    /// <summary>
    /// Token para reset de senha
    /// </summary>
    PasswordReset
}

/// <summary>
/// Extensões para o enum UserTokenType
/// </summary>
public static class UserTokenTypeExtensions
{
    /// <summary>
    /// Converte o enum para string no formato usado no banco de dados
    /// </summary>
    public static string ToDbString(this UserTokenType tokenType)
    {
        return tokenType switch
        {
            UserTokenType.Refresh => "refresh",
            UserTokenType.EmailVerification => "email_verification",
            UserTokenType.PasswordReset => "password_reset",
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, "Tipo de token não suportado")
        };
    }
    
    /// <summary>
    /// Converte string do banco de dados para o enum
    /// </summary>
    public static UserTokenType FromDbString(string dbValue)
    {
        return dbValue switch
        {
            "refresh" => UserTokenType.Refresh,
            "email_verification" => UserTokenType.EmailVerification,
            "password_reset" => UserTokenType.PasswordReset,
            _ => throw new ArgumentException($"Valor de token type não reconhecido: {dbValue}", nameof(dbValue))
        };
    }
}