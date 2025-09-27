using System.Security.Cryptography;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Commands.Users.LoginUser;
using UserService.Application.Services;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Commands.Users.ForgetPassword;

public class ForgetPasswordCommandHandler : IRequestHandler<ForgetPasswordCommand, Result<string>>
{
    public ForgetPasswordCommandHandler(
        IKeycloakService keycloakService, 
        UserManagementDbContext context, 
        ITokenService tokenService, 
        IEmailService emailService,
        ILogger<ForgetPasswordCommandHandler> logger)
    {
        _keycloakService = keycloakService;
        _context = context;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    private readonly IKeycloakService _keycloakService;
    private readonly UserManagementDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgetPasswordCommandHandler> _logger;
    public async Task<Result<string>> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Buscar o email no banco de dados
        // se encontrar gerar um token
        // Salva em UserTokens:
        //Envia um email com o link -> https://meusistema.com/reset-password?token=abc123...
        // Retorna 200 OK com mensagem genérica -> "Se esse email estiver cadastrado, você receberá as instruções para redefinir a senha."
        // Observação de segurança: nunca informar se o email existe ou não no sistema → evita enumerar contas.
        
        _logger.LogInformation("Iniciando processo do esqueci a senha para o email: {Email}", request.Email);

        if (string.IsNullOrEmpty(request.Email))
        {
            return Result<string>.Failure("O email e obrigatorio");
        }
         // Buscar o email no banco de dados
         
         var user = await _context.Users
             .Where(u => u.Email == request.Email)
             .FirstOrDefaultAsync();
        
         if (user == null)
         {
             _logger.LogWarning("Usuário não encontrado no banco local para email: {Email}", request.Email);
             return Result<string>.Failure("Se houver uma conta associada a este e-mail, um link de redefinição será enviado.");
         }
        
         // Criar o token
         var resetToken = GenerateToken();
         var userToken = new UserToken
         {
             TokenId = Guid.NewGuid(),
             UserId = user.UserId,
             TokenType = UserTokenType.PasswordReset,
             TokenValue = resetToken,
             ExpiresAt = DateTime.UtcNow.AddHours(1), 
             CreatedAt = DateTime.UtcNow,
             Version = 1
         };
        
         _context.UserTokens.Add(userToken);
         await _context.SaveChangesAsync(cancellationToken);

         // Enviar email de redefinição de senha
         var emailResult = await _emailService.SendPasswordResetEmailAsync(
             user.Email, 
             user.FirstName, 
             resetToken, 
             cancellationToken);

         if (!emailResult.IsSuccess)
         {
             _logger.LogError("Falha ao enviar email de redefinição para: {Email}. Erro: {Error}", 
                 user.Email, emailResult.FirstError);
         }
         else
         {
             _logger.LogInformation("Email de redefinição enviado com sucesso para: {Email}", user.Email);
         }

         // Retorna sempre a mesma mensagem por segurança (não revela se o email existe)
         return Result<string>.Success("Se houver uma conta associada a este e-mail, um link de redefinição será enviado.");
    }
    
    private static string GenerateToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32]; // 256 bits
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", ""); // URL-safe base64
    }
}

