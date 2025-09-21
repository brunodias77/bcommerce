
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Abstractions;
using UserService.Application.Commands.Users.CreateUser;
using UserService.Application.Commands.Users.LoginUser;
using UserService.Application.Dtos.Requests;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services.Interfaces;
using UserService.Domain.Exceptions;

namespace UserService.Api.Endpoints;
//
// /// <summary>
// /// Endpoints de autenticação e gerenciamento de usuários
// /// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", CreateUser)
        .WithName("CreateUser")
        .WithSummary("Cria um novo usuário no sistema")
        .WithDescription("Registra um novo usuário no sistema, criando primeiro no Keycloak e depois localmente")
        .Produces<CreateUserResponse>(201)
        .ProducesValidationProblem(400)
        .Produces(409)
        .Produces(500);

        // Endpoint de login de usuário
        group.MapPost("/login", LoginUser)
            .WithName("LoginUser")
            .WithSummary("Autentica um usuário no sistema")
            .WithDescription("Realiza a autenticação do usuário via Keycloak e retorna tokens JWT")
            .Produces<LoginUserResponse>(200)
            .ProducesValidationProblem(400)
            .Produces(401)
            .Produces(500);
        
        group.MapPost("/resend", ResendVerificationEmailAsync)
            .WithName("ResendVerificationEmail")
            .WithSummary("Resend verification email")
            .WithDescription("Resend email verification to user's email address")
            .Produces<EmailVerificationSentResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(409)
            .Produces<ProblemDetails>(500);

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


    internal static async Task<IResult> ResendVerificationEmailAsync(
        ResendVerificationEmailRequest request,
        IKeycloakService keycloakService,
        ILogger<Program> logger)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Solicitação inválida",
                    Detail = "Endereço de e-mail é obrigatório",
                    Status = 400
                });
            }
            
            logger.LogInformation("Reenviando e-mail de verificação para {Email}", request.Email);

            
            var user = await keycloakService.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                // Não revele se o usuário existe por razões de segurança
                logger.LogInformation("Solicitação de reenvio de e-mail de verificação para e-mail inexistente {Email}", request.Email);
                return Results.Ok(new EmailVerificationSentResponse
                (
                    Email: request.Email,
                    Message:"Se existir uma conta com este e-mail, um e-mail de verificação foi enviado",
                    SentAt: DateTimeOffset.UtcNow
                ));
            }
            
            if (user.EmailVerified)
            {
                logger.LogInformation("Solicitação de reenvio de e-mail de verificação para e-mail já verificado {Email}", request.Email);
                return Results.Conflict(new ProblemDetails
                {
                    Title = "E-mail já verificado",
                    Detail = "Este endereço de e-mail já foi verificado",
                    Status = 409
                });
            }
            
            var result = await keycloakService.SendEmailVerificationAsync(user.Id);
            if (!result)
            {
                logger.LogError("Falha ao enviar e-mail de verificação para {Email}", request.Email);
                return Results.Problem(
                    title: "Falha ao enviar e-mail",
                    detail: "Falha ao enviar e-mail de verificação",
                    statusCode: 500);
            }
            
            
            logger.LogInformation("E-mail de verificação enviado com sucesso para {Email}", request.Email);

            return Results.Ok(new EmailVerificationSentResponse
            (
                Email: request.Email,
                Message: "E-mail de verificação enviado com sucesso",
                SentAt:  DateTimeOffset.UtcNow
            ));
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resending verification email");
            return Results.Problem(
                title: "Resend Email Error",
                detail: "An error occurred while resending verification email",
                statusCode: 500);
        }
    }
}

//     {
//         var group = app.MapGroup("/auth")
//             .WithTags("Authentication");
//
//         // Endpoint de registro de usuário
//         group.MapPost("/register", CreateUser)
//             .WithName("CreateUser")
//             .WithSummary("Cria um novo usuário no sistema")
//             .WithDescription("Registra um novo usuário no sistema, criando primeiro no Keycloak e depois localmente")
//             .Produces<CreateUserResponse>(201)
//             .ProducesValidationProblem(400)
//             .Produces(409)
//             .Produces(500);
//
//         // Endpoint de login de usuário
//         group.MapPost("/login", LoginUser)
//             .WithName("LoginUser")
//             .WithSummary("Autentica um usuário no sistema")
//             .WithDescription("Realiza a autenticação do usuário via Keycloak e retorna tokens JWT")
//             .Produces<LoginUserResponse>(200)
//             .ProducesValidationProblem(400)
//             .Produces(401)
//             .Produces(500);
//     }
//
//     /// <summary>
//     /// Cria um novo usuário no sistema
//     /// </summary>
//     /// <param name="request">Dados do usuário a ser criado</param>
//     /// <param name="mediator">Mediator para envio de commands</param>
//     /// <param name="logger">Logger para registrar eventos</param>
//     /// <returns>Resultado da criação do usuário</returns>
//     private static async Task<IResult> CreateUser(
//         [FromBody] CreateUserRequest request,
//         IMediator mediator,
//         ILogger<CreateUserRequest> logger)
//     {
//         try
//         {
//             logger.LogInformation("Iniciando criação de usuário via API: {Email}", request.Email);
//
//             // Validar dados de entrada
//             var validationResults = new List<ValidationResult>();
//             var validationContext = new ValidationContext(request);
//             
//             if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
//             {
//                 var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
//                 logger.LogWarning("Dados de entrada inválidos para criação de usuário: {Errors}", string.Join(", ", errors));
//                 
//                 return Results.ValidationProblem(validationResults.ToDictionary(
//                     vr => vr.MemberNames.FirstOrDefault() ?? "Unknown",
//                     vr => new[] { vr.ErrorMessage ?? "Erro de validação" }
//                 ));
//             }
//
//             // Criar o command
//             var command = new CreateUserCommand
//             {
//                 Name = request.Name,
//                 Email = request.Email,
//                 Password = request.Password,
//                 Phone = request.Phone
//             };
//
//             // Enviar command via Mediator
//             await mediator.Send(command);
//
//             logger.LogInformation("Usuário criado com sucesso via API: {Email}", request.Email);
//
//             // Retornar resposta de sucesso
//             return Results.Created($"/users/{request.Email}", new CreateUserResponse
//             {
//                 Success = true,
//                 Message = "Usuário criado com sucesso",
//                 Email = request.Email,
//                 Name = request.Name
//             });
//         }
//         catch (InvalidOperationException ex)
//         {
//             logger.LogWarning(ex, "Erro de negócio ao criar usuário: {Email}", request.Email);
//             
//             return Results.Conflict(new CreateUserResponse
//             {
//                 Success = false,
//                 Message = ex.Message,
//                 Email = request.Email
//             });
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Erro inesperado ao criar usuário via API: {Email}", request.Email);
//             
//             return Results.Problem(
//                 detail: "Erro interno do servidor ao criar usuário",
//                 title: "Erro interno",
//                 statusCode: 500);
//         }
//     }
//
//     /// <summary>
//     /// Autentica um usuário no sistema
//     /// </summary>
//     /// <param name="request">Dados de login do usuário</param>
//     /// <param name="mediator">Mediator para envio de commands</param>
//     /// <param name="logger">Logger para registrar eventos</param>
//     /// <returns>Resultado da autenticação com tokens JWT</returns>
//     private static async Task<IResult> LoginUser(
//         [FromBody] LoginUserRequest request,
//         IMediator mediator,
//         ILogger<LoginUserRequest> logger)
//     {
//         try
//         {
//             logger.LogInformation("Iniciando autenticação de usuário via API: {Email}", request.Email);
//
//             // Validar dados de entrada
//             var validationResults = new List<ValidationResult>();
//             var validationContext = new ValidationContext(request);
//             
//             if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
//             {
//                 var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
//                 logger.LogWarning("Dados de entrada inválidos para login: {Errors}", string.Join(", ", errors));
//                 
//                 return Results.ValidationProblem(validationResults.ToDictionary(
//                     vr => vr.MemberNames.FirstOrDefault() ?? "Unknown",
//                     vr => new[] { vr.ErrorMessage ?? "Erro de validação" }
//                 ));
//             }
//
//             // Criar o command
//             var command = new LoginUserCommand
//             {
//                 Email = request.Email,
//                 Password = request.Password
//             };
//
//             // Enviar command via Mediator
//             var response = await mediator.Send(command);
//
//             if (response.Success)
//             {
//                 logger.LogInformation("Usuário autenticado com sucesso via API: {Email}", request.Email);
//                 return Results.Ok(response);
//             }
//             else
//             {
//                 logger.LogWarning("Falha na autenticação do usuário: {Email} - {Message}", request.Email, response.Message);
//                 return Results.Unauthorized();
//             }
//         }
//         catch (UnauthorizedAccessException ex)
//         {
//             logger.LogWarning(ex, "Credenciais inválidas para login: {Email}", request.Email);
//             return Results.Unauthorized();
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "Erro inesperado ao autenticar usuário via API: {Email}", request.Email);
//             
//             return Results.Problem(
//                 detail: "Erro interno do servidor ao autenticar usuário",
//                 title: "Erro interno",
//                 statusCode: 500);
//         }
//     }
// }
//
// /// <summary>
// /// Request para login de usuário
// /// </summary>
// public class LoginUserRequest
// {
//     /// <summary>
//     /// Email do usuário
//     /// </summary>
//     [Required(ErrorMessage = "O email é obrigatório")]
//     [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
//     public string Email { get; set; } = string.Empty;
//
//     /// <summary>
//     /// Senha do usuário
//     /// </summary>
//     [Required(ErrorMessage = "A senha é obrigatória")]
//     public string Password { get; set; } = string.Empty;
// }
//
// /// <summary>
// /// Request para criação de usuário
// /// </summary>
// public class CreateUserRequest
// {
//     /// <summary>
//     /// Nome completo do usuário
//     /// </summary>
//     [Required(ErrorMessage = "O nome é obrigatório")]
//     [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
//     public string Name { get; set; } = string.Empty;
//
//     /// <summary>
//     /// Email do usuário (deve ser único no sistema)
//     /// </summary>
//     [Required(ErrorMessage = "O email é obrigatório")]
//     [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
//     [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
//     public string Email { get; set; } = string.Empty;
//
//     /// <summary>
//     /// Senha do usuário
//     /// </summary>
//     [Required(ErrorMessage = "A senha é obrigatória")]
//     [KeycloakPasswordPolicy(ErrorMessage = "A senha deve atender aos critérios de segurança: mínimo 8 caracteres, pelo menos 1 letra maiúscula, 1 minúscula, 1 dígito e 1 caractere especial")]
//     public string Password { get; set; } = string.Empty;
//
//     /// <summary>
//     /// Telefone do usuário
//     /// </summary>
//     [Required(ErrorMessage = "O telefone é obrigatório")]
//     [StringLength(20, MinimumLength = 10, ErrorMessage = "O telefone deve ter entre 10 e 20 caracteres")]
//     public string Phone { get; set; } = string.Empty;
// }
//
// /// <summary>
// /// Atributo de validação que implementa as políticas de senha do Keycloak
// /// </summary>
// public class KeycloakPasswordPolicyAttribute : ValidationAttribute
// {
//     public override bool IsValid(object? value)
//     {
//         if (value is not string password || string.IsNullOrEmpty(password))
//             return false;
//
//         // Mínimo 8 caracteres
//         if (password.Length < 8)
//             return false;
//
//         // Pelo menos 1 letra maiúscula
//         if (!Regex.IsMatch(password, @"[A-Z]"))
//             return false;
//
//         // Pelo menos 1 letra minúscula
//         if (!Regex.IsMatch(password, @"[a-z]"))
//             return false;
//
//         // Pelo menos 1 dígito
//         if (!Regex.IsMatch(password, @"[0-9]"))
//             return false;
//
//         // Pelo menos 1 caractere especial
//         if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
//             return false;
//
//         return true;
//     }
//
//     protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
//     {
//         if (value is not string password || string.IsNullOrEmpty(password))
//             return new ValidationResult(ErrorMessage ?? "A senha é obrigatória");
//
//         var errors = new List<string>();
//
//         // Verificar comprimento mínimo
//         if (password.Length < 8)
//             errors.Add("mínimo 8 caracteres");
//
//         // Verificar letra maiúscula
//         if (!Regex.IsMatch(password, @"[A-Z]"))
//             errors.Add("pelo menos 1 letra maiúscula");
//
//         // Verificar letra minúscula
//         if (!Regex.IsMatch(password, @"[a-z]"))
//             errors.Add("pelo menos 1 letra minúscula");
//
//         // Verificar dígito
//         if (!Regex.IsMatch(password, @"[0-9]"))
//             errors.Add("pelo menos 1 dígito");
//
//         // Verificar caractere especial
//         if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
//             errors.Add("pelo menos 1 caractere especial (!@#$%^&*()_+-=[]{};\':\"\\|,.<>/?)");
//
//         // Verificar se não é igual ao email (se disponível no contexto)
//         if (validationContext.ObjectInstance is CreateUserRequest request)
//         {
//             if (!string.IsNullOrEmpty(request.Email) && 
//                 string.Equals(password, request.Email, StringComparison.OrdinalIgnoreCase))
//             {
//                 errors.Add("não pode ser igual ao email");
//             }
//
//             if (!string.IsNullOrEmpty(request.Name) && 
//                 string.Equals(password, request.Name, StringComparison.OrdinalIgnoreCase))
//             {
//                 errors.Add("não pode ser igual ao nome");
//             }
//         }
//
//         if (errors.Any())
//         {
//             var errorMessage = $"A senha deve atender aos seguintes critérios: {string.Join(", ", errors)}";
//             return new ValidationResult(errorMessage, new[] { validationContext.MemberName ?? "Password" });
//         }
//
//         return ValidationResult.Success;
//     }
// }
//
// /// <summary>
// /// Response da criação de usuário
// /// </summary>
// public class CreateUserResponse
// {
//     /// <summary>
//     /// Indica se a operação foi bem-sucedida
//     /// </summary>
//     public bool Success { get; set; }
//
//     /// <summary>
//     /// Mensagem descritiva do resultado
//     /// </summary>
//     public string Message { get; set; } = string.Empty;
//
//     /// <summary>
//     /// Email do usuário criado
//     /// </summary>
//     public string? Email { get; set; }
//
//     /// <summary>
//     /// Nome do usuário criado
//     /// </summary>
//     public string? Name { get; set; }
// }