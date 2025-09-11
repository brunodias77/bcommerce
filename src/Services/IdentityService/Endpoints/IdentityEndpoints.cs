using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Commands.Register;
using IdentityService.dtos;
using BuildingBlocks.Abstractions;
using System.Text;
using System.Security.Claims;

namespace IdentityService.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this WebApplication app)
    {
        // Rota principal
        app.MapGet("/", () => "Hello World!");

        // Rota de health check
        app.MapHealthChecks("/health");

        // Rota de teste para API Gateway
        app.MapGet("/api/identity", () => new
        {
            Service = "Identity Service",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Message = "Rota de teste acessada com sucesso através do API Gateway!"
        });

        // Rota adicional com parâmetro
        app.MapGet("/api/identity/test/{id}", (string id) => new
        {
            Service = "Identity Service",
            TestId = id,
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            Message = $"Teste executado para ID: {id}"
        });

        // 🔐 NOVA ROTA PROTEGIDA POR KEYCLOAK
        app.MapGet("/api/identity/keycloak-protected", [Authorize](HttpContext context) => new
        {
            Service = "Identity Service",
            Status = "Authenticated via Keycloak",
            Timestamp = DateTime.UtcNow,
            User = context.User.Identity?.Name ?? "Unknown",
            Subject = context.User.FindFirst("sub")?.Value,
            Email = context.User.FindFirst("email")?.Value,
            PreferredUsername = context.User.FindFirst("preferred_username")?.Value,
            Roles = context.User.FindAll("realm_access.roles").Select(c => c.Value).ToList(),
            Claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            Message = "Acesso autorizado via Keycloak! Esta rota requer token JWT válido do Keycloak."
        }).RequireAuthorization();

        // Rota com autorização baseada em role do Keycloak
        app.MapGet("/api/identity/admin-only", [Authorize(Roles = "admin")](HttpContext context) => new
        {
            Service = "Identity Service",
            Status = "Admin Access Granted",
            Timestamp = DateTime.UtcNow,
            User = context.User.Identity?.Name ?? "Unknown",
            Message = "Acesso de administrador autorizado via Keycloak!"
        }).RequireAuthorization();

        // Rota para gerar token JWT (para testes)
        app.MapPost("/api/identity/login", (LoginRequest request) =>
        {
            // Validação simples para demonstração
            if (request.Username == "admin" && request.Password == "password")
            {
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes("sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim("sub", request.Username),
                        new Claim("name", request.Username),
                        new Claim("role", "user")
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = "BCommerce",
                    Audience = "BCommerce-API",
                    SigningCredentials =
                        new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Results.Ok(new
                {
                    Token = tokenString,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Message = "Login realizado com sucesso!"
                });
            }

            return Results.Unauthorized();
        });

        // Endpoint para registrar usuário
        app.MapPost("/api/users/register", async (
            RegisterUserCommand command,
            IRequestHandler<RegisterUserCommand, RegisterUserResponse> handler) =>
        {
            try
            {
                var response = await handler.Handle(command, CancellationToken.None);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Erro interno do servidor"
                );
            }
        });
    }
}

// Modelo para login
public record LoginRequest(string Username, string Password);