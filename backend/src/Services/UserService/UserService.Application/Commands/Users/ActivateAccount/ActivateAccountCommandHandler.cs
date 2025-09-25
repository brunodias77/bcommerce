using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Commands.Users.ActivateAccount;

/// <summary>
/// Handler para processar a ativação de conta de usuário
/// </summary>
public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, Result>
{
    private readonly UserManagementDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ActivateAccountCommandHandler> _logger;

    /// <summary>
    /// Construtor com injeção de dependências
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="emailService">Serviço para envio de emails</param>
    /// <param name="logger">Logger para registrar eventos</param>
    public ActivateAccountCommandHandler(
        UserManagementDbContext context,
        IEmailService emailService,
        ILogger<ActivateAccountCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Processa o comando de ativação de conta
    /// Fluxo: validar token → verificar expiração → ativar usuário → enviar email de boas-vindas
    /// </summary>
    /// <param name="request">Comando com o token de ativação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Result indicando sucesso ou falha da ativação</returns>
    public async Task<Result> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando ativação de conta com token: {Token}", request.Token);

        try
        {
            // Validação do token
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                _logger.LogWarning("Tentativa de ativação com token vazio ou nulo");
                return Result.Failure("Token de ativação é obrigatório.");
            }

            // Busca o token no banco de dados
            var userToken = await _context.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.TokenValue == request.Token && 
                                         ut.TokenType == "activation" && 
                                         ut.RevokedAt == null, 
                                   cancellationToken);

            if (userToken == null)
            {
                _logger.LogWarning("Token de ativação não encontrado ou já foi usado: {Token}", request.Token);
                return Result.Failure("Token de ativação inválido ou já utilizado.");
            }

            // Verifica se o token expirou
            if (userToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Token de ativação expirado. Token: {Token}, Expirou em: {ExpiresAt}", 
                    request.Token, userToken.ExpiresAt);
                
                // Revoga o token expirado
                userToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                
                return Result.Failure("Token de ativação expirado. Solicite um novo token de ativação.");
            }

            var user = userToken.User;
            
            // Verifica se o usuário já está ativo
            if (user.Status == UserStatus.Ativo)
            {
                _logger.LogInformation("Usuário já está ativo: {Email}", user.Email);
                return Result.Failure("Esta conta já foi ativada anteriormente.");
            }

            // Ativa o usuário
            user.Status = UserStatus.Ativo;
            user.EmailVerifiedAt = DateTime.UtcNow;
            
            // Revoga o token de ativação
            userToken.RevokedAt = DateTime.UtcNow;

            // Salva as alterações
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Conta ativada com sucesso para usuário: {Email}, UserId: {UserId}", 
                user.Email, user.UserId);

            // Envia email de boas-vindas
            try
            {
                var emailResult = await _emailService.SendWelcomeEmailAsync(
                    user.Email, 
                    user.FirstName, 
                    cancellationToken);
                
                if (!emailResult.IsSuccess)
                {
                    _logger.LogWarning("Falha ao enviar email de boas-vindas para: {Email}. Erro: {Error}", 
                        user.Email, emailResult.FirstError);
                    // Não falha a ativação se o email não for enviado
                }
                else
                {
                    _logger.LogInformation("Email de boas-vindas enviado com sucesso para: {Email}", user.Email);
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Erro inesperado ao enviar email de boas-vindas para: {Email}", user.Email);
                // Não falha a ativação se houver erro no envio do email
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao ativar conta com token: {Token}", request.Token);
            return Result.Failure("Erro interno do servidor ao ativar conta. Tente novamente.");
        }
    }
}