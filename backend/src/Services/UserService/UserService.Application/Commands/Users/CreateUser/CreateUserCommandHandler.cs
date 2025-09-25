
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Commands.Users.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly UserManagementDbContext _context;
    private readonly IPasswordEncripter _passwordEncripter;
    private readonly IKeycloakService _keycloakService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    
    /// <summary>
    /// Construtor com injeção de dependências
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="passwordEncripter">Serviço para criptografia de senhas</param>
    /// <param name="keycloakService">Serviço de integração com Keycloak</param>
    /// <param name="emailService">Serviço para envio de emails</param>
    /// <param name="logger">Logger para registrar eventos</param>
    public CreateUserCommandHandler(
        UserManagementDbContext context,
        IPasswordEncripter passwordEncripter,
        IKeycloakService keycloakService,
        IEmailService emailService,
        ILogger<CreateUserCommandHandler> logger)
    {
        _context = context;
        _passwordEncripter = passwordEncripter;
        _keycloakService = keycloakService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Processa o comando de criação de usuário
    /// Fluxo: validar campos → validar email → criar no Keycloak → criar localmente
    /// </summary>
    /// <param name="request">Comando com os dados do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Result com o ID do usuário criado ou mensagem de erro</returns>
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando criação de usuário para email: {Email}", request.Email);
        
        // Declaração da variável fora do try-catch para uso posterior
        string userKeycloakId = null;
        
        try
        {
            // Validação de campos obrigatórios
            if (string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password) ||
                string.IsNullOrEmpty(request.FirstName) ||
                string.IsNullOrEmpty(request.LastName))
            {
                _logger.LogWarning("Tentativa de criar usuário com campos obrigatórios vazios");
                return Result<Guid>.Failure("Todos os campos obrigatórios devem ser preenchidos (FirstName, LastName, Email, Password).");
            }
            
            // Verifique se o usuário já existe
            var existingUser = await _keycloakService.GetUserByEmailAsync(request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Usuário já existe com email: {Email}", request.Email);
                return Result<Guid>.Failure("Um usuário com este endereço de e-mail já existe");
            }
            
                      
            var createUserRequest = new CreateUserKeycloak
            (
                Username: request.Email,
                Email: request.Email,
                FirstName: request.FirstName,
                LastName: request.LastName,
                Password: request.Password,
                Enabled: true,
                EmailVerified: false,
                Roles: new List<string> { "user" } // Default role
            );
            
            userKeycloakId = await _keycloakService.CreateUserAsync(createUserRequest);
            if (string.IsNullOrEmpty(userKeycloakId))
            {
                _logger.LogError("Keycloak não retornou ID do usuário: {Email}", request.Email);
                return Result<Guid>.Failure("Falha na criação do usuário no sistema de autenticação. Tente novamente.");
            }
            var user = await _keycloakService.GetUserByIdAsync(userKeycloakId);
            _logger.LogInformation("Usuário criado no Keycloak com sucesso. ID: {KeycloakUserId}", userKeycloakId);
        }
        catch (Exception keycloakEx)
        {
            _logger.LogError(keycloakEx, "Falha ao criar usuário no Keycloak: {Email}", request.Email);
            return Result<Guid>.Failure("Falha na criação do usuário no sistema de autenticação. Tente novamente.");
        }

        try
        {
            // Valida se o email já existe no sistema local
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            
            if (existingUser != null)
            {
                _logger.LogWarning("Tentativa de criar usuário com email já existente: {Email}", request.Email);
                return Result<Guid>.Failure($"Já existe um usuário cadastrado com o email {request.Email}");
            }
            
            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = string.Empty, // TODO: Adicionar campo Phone no command se necessário
                PasswordHash = _passwordEncripter.Encrypt(request.Password),
                Cpf = string.Empty, // TODO: Implementar validação de CPF
                DateOfBirth = null, // TODO: Adicionar campo no command se necessário
                NewsletterOptIn = request.NewsletterOptIn,
                Status = UserStatus.Inativo, // Usuário inativo até ativar a conta
                Role = UserRole.Customer,
                KeycloakId = Guid.Parse(userKeycloakId) // Armazena o ID do Keycloak (sempre disponível neste ponto)
            };
            
            // Gera token de ativação
            var activationToken = GenerateActivationToken();
            var userToken = new UserToken
            {
                TokenId = Guid.NewGuid(),
                UserId = user.UserId,
                TokenType = UserTokenType.EmailVerification,
                TokenValue = activationToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Token expira em 24 horas
                CreatedAt = DateTime.UtcNow,
                Version = 1
            };
            
            // Adiciona o usuário e token ao contexto
            _context.Users.Add(user);
            _context.UserTokens.Add(userToken);
            
            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync(cancellationToken);
            
            // Envia email de ativação
            try
            {
                var emailResult = await _emailService.SendAccountActivationEmailAsync(
                    user.Email, 
                    user.FirstName, 
                    activationToken, 
                    cancellationToken);
                
                if (!emailResult.IsSuccess)
                {
                    _logger.LogWarning("Falha ao enviar email de ativação para: {Email}. Erro: {Error}", 
                        user.Email, emailResult.FirstError);
                    // Não falha a criação do usuário se o email não for enviado
                }
                else
                {
                    _logger.LogInformation("Email de ativação enviado com sucesso para: {Email}", user.Email);
                }
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Erro inesperado ao enviar email de ativação para: {Email}", user.Email);
                // Não falha a criação do usuário se houver erro no envio do email
            }
            
            _logger.LogInformation("Usuário criado com sucesso localmente e no Keycloak. Email: {Email}, KeycloakId: {KeycloakUserId}, UserId: {UserId}. Status: Inativo (aguardando ativação)", 
                request.Email, userKeycloakId, user.UserId);
            
            return Result<Guid>.Success(user.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar usuário: {Email}", request.Email);
            
            // Se falhou na criação local, faz rollback do Keycloak
            if (!string.IsNullOrEmpty(userKeycloakId))
            {
                _logger.LogWarning("Tentando fazer rollback do usuário no Keycloak: {KeycloakUserId}", userKeycloakId);
                try
                {
                    await _keycloakService.DeleteUserAsync(userKeycloakId);
                    _logger.LogInformation("Rollback do Keycloak realizado com sucesso: {KeycloakUserId}", userKeycloakId);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Falha no rollback do Keycloak para usuário: {KeycloakUserId}", userKeycloakId);
                }
            }
            
            return Result<Guid>.Failure("Erro interno do servidor ao criar usuário. Tente novamente.");
        }
    }
    
    /// <summary>
    /// Gera um token de ativação seguro
    /// </summary>
    /// <returns>Token de ativação como string</returns>
    private static string GenerateActivationToken()
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