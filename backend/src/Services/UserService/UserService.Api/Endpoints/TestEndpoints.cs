using Microsoft.AspNetCore.Mvc;
using UserService.Infrastructure.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace UserService.Api.Endpoints
{
    /// <summary>
    /// Endpoints de teste para verificar comunicação com serviços externos
    /// </summary>
    public static class TestEndpoints
    {
        /// <summary>
        /// Registra os endpoints de teste
        /// </summary>
        /// <param name="app">WebApplication instance</param>
        public static void MapTestEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/test")
                .WithTags("Test");

            // Endpoint para testar criação de usuário no Keycloak
            group.MapPost("/keycloak/create-user", CreateKeycloakUserTest)
                .WithName("TestKeycloakCreateUser")
                .WithSummary("Testa a criação de usuário no Keycloak")
                .WithDescription("Endpoint de teste para verificar a comunicação com o Keycloak ao criar um usuário");

            // Endpoint para testar login de usuário no Keycloak
            group.MapPost("/keycloak/login", LoginKeycloakUserTest)
                .WithName("TestKeycloakLogin")
                .WithSummary("Testa o login de usuário no Keycloak")
                .WithDescription("Endpoint de teste para verificar a autenticação com o Keycloak");
        }

        /// <summary>
        /// Testa a criação de um usuário no Keycloak
        /// </summary>
        /// <param name="request">Dados do usuário para teste</param>
        /// <param name="keycloakService">Serviço do Keycloak</param>
        /// <param name="logger">Logger para debug</param>
        /// <returns>Resultado do teste</returns>
        private static async Task<IResult> CreateKeycloakUserTest(
            [FromBody] CreateUserTestRequest request,
            [FromServices] IKeycloakService keycloakService,
            [FromServices] ILogger<Program> logger)
        {
            try
            {
                logger.LogInformation("[TEST] Iniciando teste de criação de usuário no Keycloak");
                logger.LogInformation("[TEST] Email: {Email}, Nome: {FirstName} {LastName}", 
                    request.Email, request.FirstName, request.LastName);

                // Validar dados de entrada
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    logger.LogWarning("[TEST] Dados de entrada inválidos");
                    return Results.BadRequest(new TestResult
                    {
                        Success = false,
                        Message = "Todos os campos são obrigatórios",
                        Details = "Email, FirstName, LastName e Password devem ser preenchidos"
                    });
                }

                // Verificar se usuário já existe
                logger.LogInformation("[TEST] Verificando se usuário já existe no Keycloak");
                var existingUser = await keycloakService.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    logger.LogWarning("[TEST] Usuário já existe no Keycloak: {UserId}", existingUser.Id);
                    return Results.Conflict(new TestResult
                    {
                        Success = false,
                        Message = "Usuário já existe no Keycloak",
                        Details = $"Usuário com email {request.Email} já está cadastrado (ID: {existingUser.Id})",
                        KeycloakUserId = existingUser.Id
                    });
                }

                // Tentar criar o usuário no Keycloak
                logger.LogInformation("[TEST] Tentando criar usuário no Keycloak");
                var keycloakUserId = await keycloakService.CreateUserAsync(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName,
                    true);

                if (!string.IsNullOrEmpty(keycloakUserId))
                {
                    logger.LogInformation("[TEST] Usuário criado com sucesso no Keycloak: {UserId}", keycloakUserId);
                    
                    // Verificar se o usuário foi realmente criado
                    var createdUser = await keycloakService.GetUserByIdAsync(keycloakUserId);
                    
                    return Results.Ok(new TestResult
                    {
                        Success = true,
                        Message = "Usuário criado com sucesso no Keycloak",
                        Details = $"Usuário {request.FirstName} {request.LastName} foi criado e está ativo",
                        KeycloakUserId = keycloakUserId,
                        UserData = createdUser
                    });
                }
                else
                {
                    logger.LogError("[TEST] Falha ao criar usuário no Keycloak - ID retornado vazio");
                    return Results.Problem(
                        detail: "O Keycloak não retornou um ID válido para o usuário criado",
                        title: "Erro na criação do usuário",
                        statusCode: 500);
                }
            }
            catch (HttpRequestException httpEx)
            {
                logger.LogError(httpEx, "[TEST] Erro de comunicação HTTP com o Keycloak");
                return Results.Problem(
                    detail: $"Erro de comunicação com o Keycloak: {httpEx.Message}",
                    title: "Erro de comunicação",
                    statusCode: 503);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[TEST] Erro inesperado ao testar criação de usuário no Keycloak");
                return Results.Problem(
                    detail: $"Erro inesperado: {ex.Message}",
                    title: "Erro interno",
                    statusCode: 500);
            }
        }

        /// <summary>
        /// Testa o login de um usuário no Keycloak
        /// </summary>
        /// <param name="request">Dados de login para teste</param>
        /// <param name="keycloakService">Serviço do Keycloak</param>
        /// <param name="logger">Logger para debug</param>
        /// <returns>Resultado do teste de login</returns>
        private static async Task<IResult> LoginKeycloakUserTest(
            [FromBody] LoginTestRequest request,
            [FromServices] IKeycloakService keycloakService,
            [FromServices] ILogger<Program> logger)
        {
            try
            {
                logger.LogInformation("[TEST] Iniciando teste de login no Keycloak");
                logger.LogInformation("[TEST] Email: {Email}", request.Email);

                // Validar dados de entrada
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    logger.LogWarning("[TEST] Dados de entrada inválidos para login");
                    return Results.BadRequest(new LoginTestResult
                    {
                        Success = false,
                        Message = "Email e Password são obrigatórios"
                    });
                }

                // Tentar fazer login no Keycloak
                logger.LogInformation("[TEST] Tentando autenticar usuário no Keycloak");
                var tokenResponse = await keycloakService.AuthenticateUserAsync(request.Email, request.Password);

                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    logger.LogInformation("[TEST] Login realizado com sucesso no Keycloak");
                    
                    return Results.Ok(new LoginTestResult
                    {
                        Success = true,
                        Message = "Login realizado com sucesso",
                        AccessToken = tokenResponse.AccessToken,
                        RefreshToken = tokenResponse.RefreshToken,
                        ExpiresIn = tokenResponse.ExpiresIn,
                        TokenType = tokenResponse.TokenType ?? "Bearer"
                    });
                }
                else
                {
                    logger.LogWarning("[TEST] Credenciais inválidas para o usuário: {Email}", request.Email);
                    return Results.Unauthorized();
                }
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("[TEST] Credenciais inválidas para login: {Email}", request.Email);
                return Results.Problem(
                    detail: "Email ou senha inválidos",
                    title: "Credenciais inválidas",
                    statusCode: 401);
            }
            catch (HttpRequestException httpEx)
            {
                logger.LogError(httpEx, "[TEST] Erro de comunicação HTTP com o Keycloak durante login");
                return Results.Problem(
                    detail: $"Erro de comunicação com o Keycloak: {httpEx.Message}",
                    title: "Erro de comunicação",
                    statusCode: 503);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[TEST] Erro inesperado ao testar login no Keycloak");
                return Results.Problem(
                    detail: $"Erro inesperado: {ex.Message}",
                    title: "Erro interno",
                    statusCode: 500);
            }
        }
    }

    /// <summary>
    /// Request para teste de criação de usuário
    /// </summary>
    public class CreateUserTestRequest
    {
        /// <summary>
        /// Primeiro nome do usuário
        /// </summary>
        [Required(ErrorMessage = "FirstName é obrigatório")]
        [StringLength(50, ErrorMessage = "FirstName deve ter no máximo 50 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Sobrenome do usuário
        /// </summary>
        [Required(ErrorMessage = "LastName é obrigatório")]
        [StringLength(50, ErrorMessage = "LastName deve ter no máximo 50 caracteres")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        [Required(ErrorMessage = "Password é obrigatório")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter entre 6 e 100 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request para teste de login
    /// </summary>
    public class LoginTestRequest
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        [Required(ErrorMessage = "Password é obrigatório")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter entre 6 e 100 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resultado do teste
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Indica se o teste foi bem-sucedido
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem do resultado
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detalhes adicionais do resultado
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// ID do usuário no Keycloak (se criado)
        /// </summary>
        public string? KeycloakUserId { get; set; }

        /// <summary>
        /// Dados do usuário retornados pelo Keycloak
        /// </summary>
        public object? UserData { get; set; }

        /// <summary>
        /// Timestamp do teste
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Resultado do teste de login
    /// </summary>
    public class LoginTestResult
    {
        /// <summary>
        /// Indica se o login foi bem-sucedido
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem do resultado
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Token de acesso JWT
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Token de refresh
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Tempo de expiração do token em segundos
        /// </summary>
        public int? ExpiresIn { get; set; }

        /// <summary>
        /// Tipo do token (geralmente Bearer)
        /// </summary>
        public string? TokenType { get; set; }

        /// <summary>
        /// Timestamp do teste
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}