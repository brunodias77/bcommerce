using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using IdentityService.Data;
using IdentityService.Commands.Register;
using IdentityService.Endpoints;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework com PostgreSQL
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
        npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
    });
    
    // Configurações para desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

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

// Configurar MediatR customizado
builder.Services.AddMediator(
    typeof(RegisterUserCommand).Assembly, // IdentityService assembly
    typeof(IMediator).Assembly, 
    typeof(Mediator).Assembly// BuildingBlocks assembly
);

var app = builder.Build();

// Configurar middleware de autenticação
app.UseAuthentication();
app.UseAuthorization();

// Registrar todos os endpoints
app.MapIdentityEndpoints();

app.Run();