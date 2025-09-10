using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços de health check
builder.Services.AddHealthChecks();

// Configurar autenticação JWT com Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configuração baseada no setup-keycloak.sh
        options.Authority = "http://localhost:8080/realms/bcommerce";
        options.Audience = "bcommerce-client";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            // Configurações específicas do Keycloak
            ValidIssuers = new[] { "http://localhost:8080/realms/bcommerce" },
            ValidAudiences = new[] { "bcommerce-client", "account" }
        };
    });

// Adicionar autorização
builder.Services.AddAuthorization();

var app = builder.Build();

// Configurar middleware de autenticação
app.UseAuthentication();
app.UseAuthorization();

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
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("sub", request.Username),
                new System.Security.Claims.Claim("name", request.Username),
                new System.Security.Claims.Claim("role", "user")
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

app.Run();

// Modelo para login
public record LoginRequest(string Username, string Password);