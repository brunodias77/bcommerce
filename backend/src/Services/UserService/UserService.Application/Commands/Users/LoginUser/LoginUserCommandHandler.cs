
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using Microsoft.Extensions.Logging;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Dtos.Requests;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Application.Commands.Users.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly UserManagementDbContext _context;
    private readonly IPasswordEncripter _passwordEncripter;
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(UserManagementDbContext context, IPasswordEncripter passwordEncripter, IKeycloakService keycloakService, ILogger<LoginUserCommandHandler> logger)
    {
        _context = context;
        _passwordEncripter = passwordEncripter;
        _keycloakService = keycloakService;
        _logger = logger;
    }
    
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var loginRequestKeycloak = new LoginUserKeycloak(
              Email: request.Email,  
              Password: request.Password
            );

            var response = await _keycloakService.LoginAsync(loginRequestKeycloak);

            if (response != null && !string.IsNullOrEmpty(response.AccessToken))
            {
                _logger.LogInformation("Usuário {Email} autenticado com sucesso", request.Email);
                var loginUserResponse = new LoginUserResponse()
                {
                    AccessToken = response.AccessToken,
                    ExpiresIn = response.ExpiresIn,
                    RefreshToken = response.RefreshToken,
                    TokenType = response.TokenType, 
                    Scope = response.Scope,
                    RefreshExpiresIn = response.RefreshExpiresIn,
                };
                
                return Result<LoginUserResponse>.Success(loginUserResponse); 
            }
            
            _logger.LogWarning("Falha de login para o usuário {Email}: Credenciais inválidas", request.Email);
            return Result<LoginUserResponse>.Failure("Email ou senha inválidos");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Falha de login para o usuário {Email}: {Message}", request.Email, ex.Message);
            return Result<LoginUserResponse>.Failure("Credenciais inválidas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o login para o usuário {Email}", request.Email);
            return Result<LoginUserResponse>.Failure("Erro interno do servidor. Tente novamente mais tarde.");
        }    
    }
}