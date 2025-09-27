using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Commands.Users.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IKeycloakService _keycloakService;
    private readonly UserManagementDbContext _context;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IKeycloakService keycloakService,
        UserManagementDbContext context,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _keycloakService = keycloakService;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando processo de redefinição de senha com token: {Token}", request.Token);

        // Validações básicas
        if (string.IsNullOrEmpty(request.Token))
        {
            return Result<ResetPasswordResponse>.Failure("Token é obrigatório");
        }

        if (string.IsNullOrEmpty(request.NewPassword))
        {
            return Result<ResetPasswordResponse>.Failure("Nova senha é obrigatória");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return Result<ResetPasswordResponse>.Failure("A confirmação da senha não confere");
        }

        // Validar força da senha
        if (request.NewPassword.Length < 8)
        {
            return Result<ResetPasswordResponse>.Failure("A senha deve ter pelo menos 8 caracteres");
        }

        try
        {
            // Buscar o token no banco de dados
            var userToken = await _context.UserTokens
                .Include(ut => ut.User)
                .Where(ut => ut.TokenValue == request.Token && 
                           ut.TokenType == UserTokenType.PasswordReset &&
                           ut.ExpiresAt > DateTime.UtcNow &&
                           !ut.RevokedAt.HasValue)
                .FirstOrDefaultAsync(cancellationToken);

            if (userToken == null)
            {
                _logger.LogWarning("Token de redefinição inválido ou expirado: {Token}", request.Token);
                return Result<ResetPasswordResponse>.Failure("Token inválido ou expirado");
            }

            var user = userToken.User;
            if (user == null)
            {
                _logger.LogError("Usuário não encontrado para o token: {Token}", request.Token);
                return Result<ResetPasswordResponse>.Failure("Usuário não encontrado");
            }

            // Verificar se o usuário tem KeycloakId
            if (!user.KeycloakId.HasValue)
            {
                _logger.LogError("Usuário {UserId} não possui KeycloakId", user.UserId);
                return Result<ResetPasswordResponse>.Failure("Erro interno do usuário");
            }

            // Redefinir senha no Keycloak
            var resetResult = await _keycloakService.UpdatePasswordAsync(user.KeycloakId.Value.ToString(), request.NewPassword);
            if (!resetResult)
            {
                _logger.LogError("Falha ao redefinir senha no Keycloak para o usuário {UserId}", user.UserId);
                return Result<ResetPasswordResponse>.Failure("Falha ao redefinir senha");
            }

            // Marcar token como usado
            userToken.RevokedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Senha redefinida com sucesso para usuário: {UserId}", user.UserId);

            return Result<ResetPasswordResponse>.Success(new ResetPasswordResponse(true, "Senha redefinida com sucesso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao redefinir senha com token: {Token}", request.Token);
            return Result<ResetPasswordResponse>.Failure("Erro interno do servidor");
        }
    }
}