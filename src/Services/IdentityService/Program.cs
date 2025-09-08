using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços de health check
builder.Services.AddHealthChecks();

// Configurar autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configuração básica para desenvolvimento
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "BCommerce",
            ValidAudience = "BCommerce-API",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres"))
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

// 🔐 NOVA ROTA AUTENTICADA
app.MapGet("/api/identity/profile", [Authorize] (HttpContext context) => new
{
    Service = "Identity Service",
    Status = "Authenticated",
    Timestamp = DateTime.UtcNow,
    User = context.User.Identity?.Name ?? "Unknown",
    Claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
    Message = "Acesso autorizado! Esta rota requer autenticação JWT."
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
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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