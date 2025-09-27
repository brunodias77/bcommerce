
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Abstractions;
using UserService.Application.Commands.Users.CreateUser;
using UserService.Application.Commands.Users.LoginUser;
using UserService.Application.Commands.Users.ActivateAccount;
using UserService.Application.Dtos.Requests;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using UserService.Api.DTOs.Requests;

namespace UserService.Api.Endpoints;
//
// /// <summary>
// /// Endpoints de autenticação e gerenciamento de usuários
// /// </summary>
public static class AuthEndpoints
{
    

    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/api/auth"
        // e adiciona metadados como tags e suporte ao OpenAPI/Swagger
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");
        
        // ============================
        // 🔓 Endpoints Públicos (sem autenticação)
        // ============================

        // Endpoint de registro: cria uma nova conta de usuário
        group.MapPost("/register", CreateUser)
        .WithName("CreateUser")
        .WithSummary("Cria um novo usuário no sistema")
        .WithDescription("Registra um novo usuário no sistema, criando primeiro no Keycloak e depois localmente")
        .Produces<CreateUserResponse>(201)
        .ProducesValidationProblem(400)
        .Produces(409)
        .Produces(500);

        // Endpoint de login: recebe email e senha, autentica o usuário e retorna tokens JWT
        group.MapPost("/login", LoginUser)
            .WithName("LoginUser")
            .WithSummary("Autentica um usuário no sistema")
            .WithDescription("Realiza a autenticação do usuário via Keycloak e retorna tokens JWT")
            .Produces<LoginUserResponse>(200)
            .ProducesValidationProblem(400)
            .Produces(401)
            .Produces(500);

        // Endpoint para ativação de conta
        group.MapGet("/activate", ActivateAccount)
            .WithName("ActivateAccount")
            .WithSummary("Ativa uma conta de usuário")
            .WithDescription("Ativa a conta do usuário usando o token de ativação enviado por email")
            .Produces(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .Produces(409)
            .Produces(500);
        
        // Endpoint de refresh token: gera um novo access token a partir do refresh token
        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token using refresh token")
            .Produces<LoginUserResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);
        
        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Reset access token using reset password")
            .Produces<ResetPasswordResponse>(201)
            .Produces(400)
            .Produces(401)
            .Produces(410)
            .Produces(500);

    }


    private static async Task<IResult> CreateUser(
    [FromBody] CreateUserRequest request,
    IMediator mediator,
    ILogger<CreateUserRequest> logger)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Tenta validar o objeto usando as DataAnnotations
        bool isValid = Validator.TryValidateObject(request, context, results, true);

        if (!isValid)
        {
            // Retorna 400 com os erros de validação
            return Results.BadRequest(new
            {
                Errors = results.Select(r => r.ErrorMessage).ToList()
            });
        }

        try
        {
            logger.LogInformation("Iniciando criação de usuário via endpoint para email: {Email}", request.Email);

            // Validação básica de entrada
            if (request == null)
            {
                logger.LogWarning("Request nulo recebido no endpoint CreateUser");
                return Results.BadRequest("Dados da requisição são obrigatórios");
            }

            // Mapear CreateUserRequest para CreateUserCommand
            var command = new CreateUserCommand(
                FirstName: request.FirstName,
                LastName: request.LastName,
                Email: request.Email,
                Password: request.Password,
                NewsletterOptIn: request.NewsletterOptIn
            );

            // Enviar comando via MediatR
            var result = await mediator.Send(command);

            // Tratar o resultado das REGRAS DE NEGÓCIO
            if (result.IsSuccess)
            {
                logger.LogInformation("Usuário criado com sucesso. ID: {UserId}, Email: {Email}", result.Value, request.Email);
                return Results.Created($"/api/users/{result.Value}", new CreateUserResponse
                {
                    UserId = result.Value,
                    Message = "Usuário criado com sucesso",
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                });
            }

            // Tratar diferentes tipos de erro de REGRAS DE NEGÓCIO baseado na mensagem
            var errorMessage = result.FirstError;

            if (errorMessage.Contains("já existe") || errorMessage.Contains("já existente"))
            {
                logger.LogWarning("Tentativa de criar usuário com email já existente: {Email}", request.Email);
                return Results.Conflict(new { Message = errorMessage });
            }

            if (errorMessage.Contains("campos obrigatórios") || errorMessage.Contains("validação"))
            {
                logger.LogWarning("Erro de validação na criação de usuário: {Error}", errorMessage);
                return Results.BadRequest(new { Message = errorMessage });
            }

            // Outros erros de regras de negócio são tratados como Bad Request
            logger.LogWarning("Erro de regra de negócio na criação de usuário: {Error}", errorMessage);
            return Results.BadRequest(new { Message = errorMessage });
        }
        // TRATAMENTO DE ERROS INESPERADOS - Exceptions de Infraestrutura
        catch (KeycloakException keycloakEx)
        {
            logger.LogError(keycloakEx, "Erro do Keycloak ao criar usuário: {Email}, Operação: {Operation}, ErrorCode: {ErrorCode}",
                request?.Email ?? "N/A", keycloakEx.Operation, keycloakEx.ErrorCode);

            return Results.Problem(
                detail: $"Falha no sistema de autenticação: {keycloakEx.Message}",
                statusCode: 500,
                title: "Erro do Sistema de Autenticação"
            );
        }
        catch (DatabaseException dbEx)
        {
            logger.LogError(dbEx, "Erro de banco de dados ao criar usuário: {Email}, Operação: {Operation}, Tabela: {Table}",
                request?.Email ?? "N/A", dbEx.Operation, dbEx.TableName);

            return Results.Problem(
                detail: $"Falha no banco de dados: {dbEx.Message}",
                statusCode: 500,
                title: "Erro de Banco de Dados"
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exceção não tratada no endpoint CreateUser para email: {Email}", request?.Email ?? "N/A");
            return Results.Problem(
                detail: "Erro interno do servidor",
                statusCode: 500,
                title: "Erro Interno"
            );
        }
    }

    private static async Task<IResult> LoginUser(
    [FromBody] LoginUserRequest request,
    IMediator mediator,
    ILogger<LoginUserResponse> logger)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Tenta validar o objeto usando as DataAnnotations
        bool isValid = Validator.TryValidateObject(request, context, results, true);

        if (!isValid)
        {
            // Retorna 400 com os erros de validação
            return Results.BadRequest(new
            {
                Errors = results.Select(r => r.ErrorMessage).ToList()
            });
        }

        try
        {
            var command = new LoginUserCommand{
                Email = request.Email,
                Password = request.Password
            };

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                logger.LogInformation("User login successful for email: {Email}", request.Email);
                return Results.Ok(result.Value);
            }
            else
            {
                logger.LogWarning("User login failed for email: {Email}. Error: {Error}", request.Email, result.FirstError);
                return Results.BadRequest(new { message = result.FirstError });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during user login for email: {Email}", request.Email);
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An unexpected error occurred during login",
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// Ativa uma conta de usuário usando token de ativação
    /// </summary>
    /// <param name="request">Request com o token de ativação</param>
    /// <param name="mediator">Mediator para envio de commands</param>
    /// <param name="logger">Logger para registrar eventos</param>
    /// <returns>Resultado da ativação da conta</returns>
    private static async Task<IResult> ActivateAccount(
        [FromQuery] string token,
        IMediator mediator,
        ILogger<ActivateAccountRequest> logger)
    {
        // Validação básica do token
        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(new
            {
                Errors = new[] { "Token de ativação é obrigatório." }
            });
        }

        if (token.Length > 500)
        {
            return Results.BadRequest(new
            {
                Errors = new[] { "Token deve ter no máximo 500 caracteres." }
            });
        }

        try
        {
            logger.LogInformation("Iniciando ativação de conta via endpoint com token: {Token}", token);

            // Criar o command
            var command = new ActivateAccountCommand(token);

            // Enviar command via MediatR
            var result = await mediator.Send(command);

            // Tratar o resultado
            if (result.IsSuccess)
            {
                logger.LogInformation("Conta ativada com sucesso via endpoint");
                return Results.Ok(new { Message = "Conta ativada com sucesso! Você já pode fazer login." });
            }

            // Tratar diferentes tipos de erro baseado na mensagem
            var errorMessage = result.FirstError;

            if (errorMessage.Contains("inválido") || errorMessage.Contains("já utilizado"))
            {
                logger.LogWarning("Token de ativação inválido ou já utilizado: {Token}", token);
                return Results.NotFound(new { Message = errorMessage });
            }

            if (errorMessage.Contains("expirado"))
            {
                logger.LogWarning("Token de ativação expirado: {Token}", token);
                return Results.BadRequest(new { Message = errorMessage });
            }

            if (errorMessage.Contains("já foi ativada"))
            {
                logger.LogWarning("Tentativa de ativar conta já ativa: {Token}", token);
                return Results.Conflict(new { Message = errorMessage });
            }

            // Outros erros são tratados como Bad Request
            logger.LogWarning("Erro na ativação de conta: {Error}", errorMessage);
            return Results.BadRequest(new { Message = errorMessage });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exceção não tratada no endpoint ActivateAccount para token: {Token}", token ?? "N/A");
            return Results.Problem(
                detail: "Erro interno do servidor",
                statusCode: 500,
                title: "Erro Interno"
            );
        }
    }


    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        IKeycloakService keycloakService,
        ITokenService tokenService,
        ILogger<RefreshTokenRequest> logger)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Tenta validar o objeto usando as DataAnnotations
        bool isValid = Validator.TryValidateObject(request, context, results, true);

        if (!isValid)
        {
            // Retorna 400 com os erros de validação
            return Results.BadRequest(new
            {
                Errors = results.Select(r => r.ErrorMessage).ToList()
            });
        }

        try
        {
            logger.LogInformation("Iniciando renovação de token para refresh token: {RefreshTokenPrefix}", 
                request.RefreshToken[..Math.Min(10, request.RefreshToken.Length)]);

            // Extrai o user_id do refresh token atual
            var userId = await tokenService.ExtractUserIdFromRefreshTokenAsync(request.RefreshToken);
            if (userId == null)
            {
                logger.LogWarning("Não foi possível extrair UserId do refresh token fornecido");
                return Results.Problem(
                    title: "Token inválido",
                    detail: "O refresh token fornecido é inválido",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            logger.LogDebug("UserId extraído do refresh token: {UserId}", userId);

            // Chama o Keycloak para renovar o token
            var keycloakResponse = await keycloakService.RefreshTokenAsync(request.RefreshToken);
            
            logger.LogInformation("Token renovado com sucesso no Keycloak. Novo access token expira em: {ExpiresIn}s", 
                keycloakResponse.ExpiresIn);

            // Calcula a data de expiração do novo refresh token
            var refreshTokenExpiresAt = DateTime.UtcNow.AddSeconds(keycloakResponse.RefreshExpiresIn);

            // Revoga o refresh token anterior
            await tokenService.RevokeRefreshTokensAsync(userId.Value, keycloakResponse.RefreshToken);
            logger.LogDebug("Refresh tokens anteriores revogados para UserId: {UserId}", userId);

            // Salva o novo refresh token no banco de dados
            await tokenService.SaveRefreshTokenAsync(userId.Value, keycloakResponse.RefreshToken, refreshTokenExpiresAt);
            logger.LogInformation("Novo refresh token salvo no banco de dados para UserId: {UserId}", userId);

            // Mapeia a resposta do Keycloak para o formato da API
            var response = new
            {
                AccessToken = keycloakResponse.AccessToken,
                RefreshToken = keycloakResponse.RefreshToken,
                ExpiresIn = keycloakResponse.ExpiresIn,
                TokenType = keycloakResponse.TokenType
            };

            logger.LogInformation("Renovação de token concluída com sucesso para UserId: {UserId}", userId);
            return Results.Ok(response);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            logger.LogWarning("Refresh token inválido ou expirado: {Error}", ex.Message);
            return Results.Problem(
                title: "Token inválido",
                detail: "O refresh token fornecido é inválido ou expirou",
                statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro interno ao renovar token");
            return Results.Problem(
                title: "Erro interno",
                detail: "Ocorreu um erro interno ao processar a renovação do token",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }


    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        IKeycloakService keycloakService,
        ITokenService tokenService,
        ILogger<RefreshTokenRequest> logger)
    {
        return null;
    }
    
    
    
    
    
    
  
}

