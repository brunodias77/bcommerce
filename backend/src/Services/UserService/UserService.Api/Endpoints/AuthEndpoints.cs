using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Abstractions;
using UserService.Application.Commands.Users.CreateUser;

namespace UserService.Api.Endpoints;

/// <summary>
/// Endpoints de autenticação e gerenciamento de usuários
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Registra os endpoints de autenticação
    /// </summary>
    /// <param name="app">WebApplication instance</param>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Authentication");

        // Endpoint de registro de usuário
        group.MapPost("/register", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Cria um novo usuário no sistema")
            .WithDescription("Registra um novo usuário no sistema, criando primeiro no Keycloak e depois localmente")
            .Produces<CreateUserResponse>(201)
            .ProducesValidationProblem(400)
            .Produces(409)
            .Produces(500);
    }

    /// <summary>
    /// Cria um novo usuário no sistema
    /// </summary>
    /// <param name="request">Dados do usuário a ser criado</param>
    /// <param name="mediator">Mediator para envio de commands</param>
    /// <param name="logger">Logger para registrar eventos</param>
    /// <returns>Resultado da criação do usuário</returns>
    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        IMediator mediator,
        ILogger<CreateUserRequest> logger)
    {
        try
        {
            logger.LogInformation("Iniciando criação de usuário via API: {Email}", request.Email);

            // Validar dados de entrada
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                logger.LogWarning("Dados de entrada inválidos para criação de usuário: {Errors}", string.Join(", ", errors));
                
                return Results.ValidationProblem(validationResults.ToDictionary(
                    vr => vr.MemberNames.FirstOrDefault() ?? "Unknown",
                    vr => new[] { vr.ErrorMessage ?? "Erro de validação" }
                ));
            }

            // Criar o command
            var command = new CreateUserCommand
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone
            };

            // Enviar command via Mediator
            await mediator.Send(command);

            logger.LogInformation("Usuário criado com sucesso via API: {Email}", request.Email);

            // Retornar resposta de sucesso
            return Results.Created($"/users/{request.Email}", new CreateUserResponse
            {
                Success = true,
                Message = "Usuário criado com sucesso",
                Email = request.Email,
                Name = request.Name
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Erro de negócio ao criar usuário: {Email}", request.Email);
            
            return Results.Conflict(new CreateUserResponse
            {
                Success = false,
                Message = ex.Message,
                Email = request.Email
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado ao criar usuário via API: {Email}", request.Email);
            
            return Results.Problem(
                detail: "Erro interno do servidor ao criar usuário",
                title: "Erro interno",
                statusCode: 500);
        }
    }
}

/// <summary>
/// Request para criação de usuário
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário (deve ser único no sistema)
    /// </summary>
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "A senha é obrigatória")]
    [KeycloakPasswordPolicy(ErrorMessage = "A senha deve atender aos critérios de segurança: mínimo 8 caracteres, pelo menos 1 letra maiúscula, 1 minúscula, 1 dígito e 1 caractere especial")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do usuário
    /// </summary>
    [Required(ErrorMessage = "O telefone é obrigatório")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "O telefone deve ter entre 10 e 20 caracteres")]
    public string Phone { get; set; } = string.Empty;
}

/// <summary>
/// Atributo de validação que implementa as políticas de senha do Keycloak
/// </summary>
public class KeycloakPasswordPolicyAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string password || string.IsNullOrEmpty(password))
            return false;

        // Mínimo 8 caracteres
        if (password.Length < 8)
            return false;

        // Pelo menos 1 letra maiúscula
        if (!Regex.IsMatch(password, @"[A-Z]"))
            return false;

        // Pelo menos 1 letra minúscula
        if (!Regex.IsMatch(password, @"[a-z]"))
            return false;

        // Pelo menos 1 dígito
        if (!Regex.IsMatch(password, @"[0-9]"))
            return false;

        // Pelo menos 1 caractere especial
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            return false;

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || string.IsNullOrEmpty(password))
            return new ValidationResult(ErrorMessage ?? "A senha é obrigatória");

        var errors = new List<string>();

        // Verificar comprimento mínimo
        if (password.Length < 8)
            errors.Add("mínimo 8 caracteres");

        // Verificar letra maiúscula
        if (!Regex.IsMatch(password, @"[A-Z]"))
            errors.Add("pelo menos 1 letra maiúscula");

        // Verificar letra minúscula
        if (!Regex.IsMatch(password, @"[a-z]"))
            errors.Add("pelo menos 1 letra minúscula");

        // Verificar dígito
        if (!Regex.IsMatch(password, @"[0-9]"))
            errors.Add("pelo menos 1 dígito");

        // Verificar caractere especial
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            errors.Add("pelo menos 1 caractere especial (!@#$%^&*()_+-=[]{};\':\"\\|,.<>/?)");

        // Verificar se não é igual ao email (se disponível no contexto)
        if (validationContext.ObjectInstance is CreateUserRequest request)
        {
            if (!string.IsNullOrEmpty(request.Email) && 
                string.Equals(password, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("não pode ser igual ao email");
            }

            if (!string.IsNullOrEmpty(request.Name) && 
                string.Equals(password, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("não pode ser igual ao nome");
            }
        }

        if (errors.Any())
        {
            var errorMessage = $"A senha deve atender aos seguintes critérios: {string.Join(", ", errors)}";
            return new ValidationResult(errorMessage, new[] { validationContext.MemberName ?? "Password" });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Response da criação de usuário
/// </summary>
public class CreateUserResponse
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem descritiva do resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário criado
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Nome do usuário criado
    /// </summary>
    public string? Name { get; set; }
}