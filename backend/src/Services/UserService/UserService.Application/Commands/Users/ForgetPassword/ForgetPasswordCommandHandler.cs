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
    public ForgetPasswordCommandHandler(IKeycloakService keycloakService, UserManagementDbContext context, ITokenService tokenService, ILogger<LoginUserCommandHandler> logger)
    {
        _keycloakService = keycloakService;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    private readonly IKeycloakService _keycloakService;
    private readonly UserManagementDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginUserCommandHandler> _logger;
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
         var activationToken = GenerateToken();
         var userToken = new UserToken
         {
             TokenId = Guid.NewGuid(),
             UserId = user.UserId,
             TokenType = UserTokenType.PasswordReset,
             TokenValue = activationToken,
             ExpiresAt = DateTime.UtcNow.AddHours(1), 
             CreatedAt = DateTime.UtcNow,
             Version = 1
         };
        
         _context.UserTokens.Add(userToken);
         await _context.SaveChangesAsync(cancellationToken);

         // TODO: criar e enviar o email com o token para que ele possa redefinir a senha
        
        
        throw new NotImplementedException();
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

