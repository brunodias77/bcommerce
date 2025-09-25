using System.Net;
using System.Net.Mail;
using System.Text;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Application.Services.Interfaces;

namespace UserService.Application.Services;

/// <summary>
/// Implementação do serviço de envio de emails
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Envia email de ativação de conta para o usuário
    /// </summary>
    public async Task<Result> SendAccountActivationEmailAsync(
        string email, 
        string firstName, 
        string activationToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_emailSettings.Enabled)
            {
                _logger.LogWarning("Serviço de email está desabilitado. Email de ativação não enviado para: {Email}", email);
                return Result.Success(); // Retorna sucesso para não quebrar o fluxo
            }

            _logger.LogInformation("Enviando email de ativação para: {Email}", email);

            var subject = "Ative sua conta - B-Commerce";
            var htmlBody = GenerateActivationEmailTemplate(firstName, activationToken);
            var textBody = GenerateActivationEmailTextTemplate(firstName, activationToken);

            var result = await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Email de ativação enviado com sucesso para: {Email}", email);
            }
            else
            {
                _logger.LogError("Falha ao enviar email de ativação para: {Email}. Erro: {Error}", email, result.FirstError);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar email de ativação para: {Email}", email);
            return Result.Failure($"Erro interno ao enviar email de ativação: {ex.Message}");
        }
    }

    /// <summary>
    /// Envia email de boas-vindas após ativação da conta
    /// </summary>
    public async Task<Result> SendWelcomeEmailAsync(
        string email, 
        string firstName, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_emailSettings.Enabled)
            {
                _logger.LogWarning("Serviço de email está desabilitado. Email de boas-vindas não enviado para: {Email}", email);
                return Result.Success();
            }

            _logger.LogInformation("Enviando email de boas-vindas para: {Email}", email);

            var subject = "Bem-vindo ao B-Commerce!";
            var htmlBody = GenerateWelcomeEmailTemplate(firstName);
            var textBody = GenerateWelcomeEmailTextTemplate(firstName);

            var result = await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Email de boas-vindas enviado com sucesso para: {Email}", email);
            }
            else
            {
                _logger.LogError("Falha ao enviar email de boas-vindas para: {Email}. Erro: {Error}", email, result.FirstError);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar email de boas-vindas para: {Email}", email);
            return Result.Failure($"Erro interno ao enviar email de boas-vindas: {ex.Message}");
        }
    }

    /// <summary>
    /// Envia email de redefinição de senha
    /// </summary>
    public async Task<Result> SendPasswordResetEmailAsync(
        string email, 
        string firstName, 
        string resetToken, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_emailSettings.Enabled)
            {
                _logger.LogWarning("Serviço de email está desabilitado. Email de redefinição não enviado para: {Email}", email);
                return Result.Success();
            }

            _logger.LogInformation("Enviando email de redefinição de senha para: {Email}", email);

            var subject = "Redefinir sua senha - B-Commerce";
            var htmlBody = GeneratePasswordResetEmailTemplate(firstName, resetToken);
            var textBody = GeneratePasswordResetEmailTextTemplate(firstName, resetToken);

            var result = await SendEmailAsync(email, subject, htmlBody, textBody, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Email de redefinição de senha enviado com sucesso para: {Email}", email);
            }
            else
            {
                _logger.LogError("Falha ao enviar email de redefinição de senha para: {Email}. Erro: {Error}", email, result.FirstError);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar email de redefinição de senha para: {Email}", email);
            return Result.Failure($"Erro interno ao enviar email de redefinição: {ex.Message}");
        }
    }

    /// <summary>
    /// Método privado para envio de email via SMTP
    /// </summary>
    private async Task<Result> SendEmailAsync(
        string toEmail, 
        string subject, 
        string htmlBody, 
        string textBody, 
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                Timeout = _emailSettings.TimeoutSeconds * 1000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                IsBodyHtml = true,
                Body = htmlBody,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            message.To.Add(toEmail);
            
            // Adiciona versão em texto plano como alternativa
            var textView = AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, "text/plain");
            var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
            
            message.AlternateViews.Add(textView);
            message.AlternateViews.Add(htmlView);

            await client.SendMailAsync(message, cancellationToken);
            
            return Result.Success();
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "Erro SMTP ao enviar email para: {Email}. StatusCode: {StatusCode}", 
                toEmail, smtpEx.StatusCode);
            return Result.Failure($"Erro SMTP: {smtpEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar email para: {Email}", toEmail);
            return Result.Failure($"Erro ao enviar email: {ex.Message}");
        }
    }

    #region Email Templates

    /// <summary>
    /// Gera template HTML para email de ativação
    /// </summary>
    private string GenerateActivationEmailTemplate(string firstName, string activationToken)
    {
        var activationUrl = $"{_emailSettings.BaseUrl}/api/auth/activate?token={activationToken}";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Ative sua conta - B-Commerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>B-Commerce</h1>
        </div>
        <div class=""content"">
            <h2>Olá, {firstName}!</h2>
            <p>Obrigado por se cadastrar no B-Commerce. Para ativar sua conta e começar a usar nossos serviços, clique no botão abaixo:</p>
            <p style=""text-align: center;"">
                <a href=""{activationUrl}"" class=""button"">Ativar Conta</a>
            </p>
            <p>Se o botão não funcionar, copie e cole o seguinte link no seu navegador:</p>
            <p style=""word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 3px;"">
                {activationUrl}
            </p>
            <p><strong>Este link expira em 24 horas.</strong></p>
            <p>Se você não criou uma conta no B-Commerce, ignore este email.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 B-Commerce. Todos os direitos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Gera template de texto plano para email de ativação
    /// </summary>
    private string GenerateActivationEmailTextTemplate(string firstName, string activationToken)
    {
        var activationUrl = $"{_emailSettings.BaseUrl}/api/auth/activate?token={activationToken}";
        
        return $@"B-Commerce - Ative sua conta

Olá, {firstName}!

Obrigado por se cadastrar no B-Commerce. Para ativar sua conta e começar a usar nossos serviços, acesse o seguinte link:

{activationUrl}

Este link expira em 24 horas.

Se você não criou uma conta no B-Commerce, ignore este email.

© 2024 B-Commerce. Todos os direitos reservados.";
    }

    /// <summary>
    /// Gera template HTML para email de boas-vindas
    /// </summary>
    private string GenerateWelcomeEmailTemplate(string firstName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Bem-vindo ao B-Commerce!</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Bem-vindo ao B-Commerce!</h1>
        </div>
        <div class=""content"">
            <h2>Olá, {firstName}!</h2>
            <p>Sua conta foi ativada com sucesso! Agora você pode aproveitar todos os recursos do B-Commerce.</p>
            <p>Explore nossa plataforma e descubra tudo o que temos a oferecer:</p>
            <ul>
                <li>Produtos de alta qualidade</li>
                <li>Ofertas exclusivas</li>
                <li>Atendimento personalizado</li>
                <li>Entrega rápida e segura</li>
            </ul>
            <p style=""text-align: center;"">
                <a href=""{_emailSettings.BaseUrl}"" class=""button"">Começar a Comprar</a>
            </p>
            <p>Se você tiver alguma dúvida, nossa equipe de suporte está sempre pronta para ajudar.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 B-Commerce. Todos os direitos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Gera template de texto plano para email de boas-vindas
    /// </summary>
    private string GenerateWelcomeEmailTextTemplate(string firstName)
    {
        return $@"B-Commerce - Bem-vindo!

Olá, {firstName}!

Sua conta foi ativada com sucesso! Agora você pode aproveitar todos os recursos do B-Commerce.

Explore nossa plataforma e descubra tudo o que temos a oferecer:
- Produtos de alta qualidade
- Ofertas exclusivas
- Atendimento personalizado
- Entrega rápida e segura

Acesse: {_emailSettings.BaseUrl}

Se você tiver alguma dúvida, nossa equipe de suporte está sempre pronta para ajudar.

© 2024 B-Commerce. Todos os direitos reservados.";
    }

    /// <summary>
    /// Gera template HTML para email de redefinição de senha
    /// </summary>
    private string GeneratePasswordResetEmailTemplate(string firstName, string resetToken)
    {
        var resetUrl = $"{_emailSettings.BaseUrl}/reset-password?token={resetToken}";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Redefinir senha - B-Commerce</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>B-Commerce</h1>
        </div>
        <div class=""content"">
            <h2>Olá, {firstName}!</h2>
            <p>Recebemos uma solicitação para redefinir a senha da sua conta. Se foi você quem fez esta solicitação, clique no botão abaixo:</p>
            <p style=""text-align: center;"">
                <a href=""{resetUrl}"" class=""button"">Redefinir Senha</a>
            </p>
            <p>Se o botão não funcionar, copie e cole o seguinte link no seu navegador:</p>
            <p style=""word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 3px;"">
                {resetUrl}
            </p>
            <p><strong>Este link expira em 2 horas.</strong></p>
            <p>Se você não solicitou a redefinição de senha, ignore este email. Sua senha permanecerá inalterada.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2024 B-Commerce. Todos os direitos reservados.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Gera template de texto plano para email de redefinição de senha
    /// </summary>
    private string GeneratePasswordResetEmailTextTemplate(string firstName, string resetToken)
    {
        var resetUrl = $"{_emailSettings.BaseUrl}/reset-password?token={resetToken}";
        
        return $@"B-Commerce - Redefinir senha

Olá, {firstName}!

Recebemos uma solicitação para redefinir a senha da sua conta. Se foi você quem fez esta solicitação, acesse o seguinte link:

{resetUrl}

Este link expira em 2 horas.

Se você não solicitou a redefinição de senha, ignore este email. Sua senha permanecerá inalterada.

© 2024 B-Commerce. Todos os direitos reservados.";
    }

    #endregion
}