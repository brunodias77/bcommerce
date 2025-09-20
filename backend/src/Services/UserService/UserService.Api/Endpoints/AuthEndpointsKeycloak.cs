using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Services.Interfaces;


namespace UserService.Api.Endpoints;

public static class AuthEndpointsKeycloak
{
    public static void MapAuthEndpointsKeycloak(this WebApplication app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();
        
        auth.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register new user account")
            .Produces<UserResponse>(201)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(409);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IKeycloakService keycloakService,
        ILogger<Program> logger)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password) ||
                string.IsNullOrEmpty(request.FirstName) ||
                string.IsNullOrEmpty(request.LastName))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Solicitação inválida",
                    Detail = "E-mail, senha, nome e sobrenome são obrigatórios",
                    Status = 400
                });
            }
            
            // Verifique se o usuário já existe
            var existingUser = await keycloakService.GetUserByEmailAsync(request.Email);
            
            if (existingUser != null)
            {
                return Results.Conflict(new ProblemDetails
                {
                    Title = "Usuário já existe",
                    Detail = "Um usuário com este endereço de e-mail já existe",
                    Status = 409
                });
            }
            
            var createUserRequest = new CreateUserRequest
            {
                Username = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                Enabled = true,
                EmailVerified = false,
                Roles = new List<string> { "user" } // Default role
            };
            
            var userId = await keycloakService.CreateUserAsync(createUserRequest);
            var user = await keycloakService.GetUserByIdAsync(userId);
            
            logger.LogInformation("Novo usuário registrado: {Email}", request.Email);
            return Results.Created($"/api/users/{userId}", user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante o registro do usuário para {Email}", request.Email);
            return Results.Problem(
                title: "Erro de Registro",
                detail: "Ocorreu um erro durante o registro",
                statusCode: 500);
        }
    }

}

// Additional DTOs for auth endpoints
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}