using BuildingBlocks.Abstractions;
using Microsoft.Extensions.Logging;
using UserService.Infrastructure.Services.Interfaces;

namespace UserService.Application.Commands.Users.LoginUser;

/// <summary>
/// Handler responsável por processar o comando de login de usuário
/// Implementa IRequestHandler<LoginUserCommand, LoginUserResponse> para autenticação
/// </summary>
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    /// <summary>
    /// Construtor com injeção de dependências
    /// </summary>
    /// <param name="keycloakService">Serviço de integração com Keycloak</param>
    /// <param name="logger">Logger para registrar eventos</param>
    public LoginUserCommandHandler(
        IKeycloakService keycloakService,
        ILogger<LoginUserCommandHandler> logger)
    {
        _keycloakService = keycloakService;
        _logger = logger;
    }

    /// <summary>
    /// Processa o comando de login de usuário
    /// Autentica as credenciais via Keycloak e retorna tokens JWT
    /// </summary>
    /// <param name="request">Comando com as credenciais do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>LoginUserResponse com tokens de autenticação ou erro</returns>
    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando autenticação para usuário: {Email}", request.Email);

        try
        {
            // Autentica o usuário via Keycloak
            var authResult = await _keycloakService.AuthenticateUserAsync(request.Email, request.Password);

            if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
            {
                _logger.LogInformation("Autenticação realizada com sucesso para usuário: {Email}", request.Email);
                
                return new LoginUserResponse
                {
                    Success = true,
                    Message = "Login realizado com sucesso",
                    AccessToken = authResult.AccessToken,
                    RefreshToken = authResult.RefreshToken,
                    ExpiresIn = authResult.ExpiresIn,
                    TokenType = authResult.TokenType,
                    Scope = authResult.Scope
                };
            }
            else
            {
                _logger.LogWarning("Falha na autenticação para usuário: {Email} - Credenciais inválidas", request.Email);
                
                return new LoginUserResponse
                {
                    Success = false,
                    Message = "Email ou senha inválidos",
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    ExpiresIn = 0,
                    TokenType = "Bearer",
                    Scope = string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante autenticação do usuário: {Email}", request.Email);
            
            return new LoginUserResponse
            {
                Success = false,
                Message = "Erro interno do servidor. Tente novamente mais tarde.",
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                ExpiresIn = 0,
                TokenType = "Bearer",
                Scope = string.Empty
            };
        }
    }
}