using BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Services.Interfaces;

namespace UserService.Application.Commands.Users.CreateUser;

/// <summary>
/// Handler responsável por processar o comando de criação de usuário
/// Implementa IRequestHandler<CreateUserCommand, Unit> para commands sem retorno
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Unit>
{
    private readonly UserManagementDbContext _context;
    private readonly IPasswordEncripter _passwordEncripter;
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    /// <summary>
    /// Construtor com injeção de dependências
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    /// <param name="passwordEncripter">Serviço para criptografia de senhas</param>
    /// <param name="keycloakService">Serviço de integração com Keycloak</param>
    /// <param name="logger">Logger para registrar eventos</param>
    public CreateUserCommandHandler(
        UserManagementDbContext context,
        IPasswordEncripter passwordEncripter,
        IKeycloakService keycloakService,
        ILogger<CreateUserCommandHandler> logger)
    {
        _context = context;
        _passwordEncripter = passwordEncripter;
        _keycloakService = keycloakService;
        _logger = logger;
    }

    /// <summary>
    /// Processa o comando de criação de usuário
    /// Fluxo: validar email → criar no Keycloak → criar localmente
    /// </summary>
    /// <param name="request">Comando com os dados do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Unit.Value indicando sucesso da operação</returns>
    /// <exception cref="InvalidOperationException">Quando email já existe no sistema ou falha na criação no Keycloak</exception>
    public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando criação de usuário para email: {Email}", request.Email);

        // Valida se o email já existe no sistema local
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            _logger.LogWarning("Tentativa de criar usuário com email já existente: {Email}", request.Email);
            throw new InvalidOperationException($"Já existe um usuário cadastrado com o email {request.Email}");
        }

        // Separa o nome completo em primeiro e último nome
        var nameParts = request.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : firstName; // Se não há sobrenome, usa o primeiro nome

        // 1. Cria o usuário no Keycloak (obrigatório)
        string keycloakUserId;
        try
        {
            _logger.LogInformation("Criando usuário no Keycloak: {Email}", request.Email);
            keycloakUserId = await _keycloakService.CreateUserAsync(
                request.Email,
                request.Password,
                firstName,
                lastName,
                enabled: true);

            if (string.IsNullOrEmpty(keycloakUserId))
            {
                _logger.LogError("Keycloak não retornou ID do usuário: {Email}", request.Email);
                throw new InvalidOperationException("Falha na criação do usuário no sistema de autenticação. Tente novamente.");
            }

            _logger.LogInformation("Usuário criado no Keycloak com sucesso. ID: {KeycloakUserId}", keycloakUserId);
        }
        catch (Exception keycloakEx) when (keycloakEx is not InvalidOperationException)
        {
            _logger.LogError(keycloakEx, "Falha ao criar usuário no Keycloak: {Email}", request.Email);
            throw new InvalidOperationException("Falha na criação do usuário no sistema de autenticação. Tente novamente.", keycloakEx);
        }
        
        try
        {
            // 2. Cria o usuário localmente (apenas após sucesso no Keycloak)
            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = request.Email,
                Phone = request.Phone, // Mapeia o telefone do command
                PasswordHash = _passwordEncripter.Encrypt(request.Password),
                Cpf = string.Empty, // TODO: Implementar validação de CPF
                DateOfBirth = null, // TODO: Adicionar campo no command se necessário
                NewsletterOptIn = false, // TODO: Adicionar campo no command se necessário
                Status = UserStatus.Ativo,
                Role = UserRole.Customer,
                KeycloakId = Guid.Parse(keycloakUserId) // Armazena o ID do Keycloak (sempre disponível neste ponto)
            };

            // Adiciona o usuário ao contexto
            _context.Users.Add(user);

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuário criado com sucesso localmente e no Keycloak. Email: {Email}, KeycloakId: {KeycloakUserId}", 
                request.Email, keycloakUserId);

            return Unit.Value;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Erro inesperado ao criar usuário: {Email}", request.Email);
            
            // Se falhou na criação local, faz rollback do Keycloak
            _logger.LogWarning("Tentando fazer rollback do usuário no Keycloak: {KeycloakUserId}", keycloakUserId);
            try
            {
                await _keycloakService.DeleteUserAsync(keycloakUserId);
                _logger.LogInformation("Rollback do Keycloak realizado com sucesso: {KeycloakUserId}", keycloakUserId);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Falha no rollback do Keycloak para usuário: {KeycloakUserId}", keycloakUserId);
            }
            
            // Re-lança a exceção original sem envolver em InvalidOperationException
            throw;
        }
    }
}