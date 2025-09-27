
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using Microsoft.Extensions.Logging;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Dtos.Requests;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Commands.Users.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IKeycloakService _keycloakService;
    private readonly UserManagementDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IKeycloakService keycloakService,
        UserManagementDbContext context,
        ITokenService tokenService,
        ILogger<LoginUserCommandHandler> logger)
    {
        _keycloakService = keycloakService;
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }
    
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando processo de login para email: {Email}", request.Email);

            // Busca o usuário no banco de dados local pelo email
            var user = await _context.Users
                .Where(u => u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado no banco local para email: {Email}", request.Email);
                return Result<LoginUserResponse>.Failure("Credenciais inválidas");
            }

            _logger.LogDebug("Usuário encontrado no banco local: {UserId}", user.UserId);

            // Autentica o usuário via Keycloak
            var loginRequestKeycloak = new LoginUserKeycloak(
                Email: request.Email,
                Password: request.Password
            );
            var keycloakResponse = await _keycloakService.LoginAsync(loginRequestKeycloak);
            
            _logger.LogInformation("Login realizado com sucesso no Keycloak para email: {Email}", request.Email);

            // Calcula a data de expiração do refresh token
            var refreshTokenExpiresAt = DateTime.UtcNow.AddSeconds(keycloakResponse.RefreshExpiresIn);

            // Salva o refresh token inicial no banco de dados
            await _tokenService.SaveRefreshTokenAsync(user.UserId, keycloakResponse.RefreshToken, refreshTokenExpiresAt);
            _logger.LogInformation("Refresh token inicial salvo no banco de dados para UserId: {UserId}", user.UserId);

            // Mapeia a resposta do Keycloak para o formato da aplicação
            var response = new LoginUserResponse
            {
                AccessToken = keycloakResponse.AccessToken,
                RefreshToken = keycloakResponse.RefreshToken,
                ExpiresIn = keycloakResponse.ExpiresIn,
                TokenType = keycloakResponse.TokenType
            };

            _logger.LogInformation("Login concluído com sucesso para UserId: {UserId}", user.UserId);
            return Result<LoginUserResponse>.Success(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Falha na autenticação para email: {Email}", request.Email);
            return Result<LoginUserResponse>.Failure("Credenciais inválidas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado durante o login para email: {Email}", request.Email);
            return Result<LoginUserResponse>.Failure("Erro interno do servidor");
        }    
    }
}