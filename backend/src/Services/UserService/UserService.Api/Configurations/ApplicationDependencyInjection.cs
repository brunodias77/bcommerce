using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BuildingBlocks.Mediator;
using UserService.Application.Services;

namespace UserService.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddMediator(services);
        AddSwagger(services);
        AddCors(services, configuration);
        AddAuthentication(services, configuration);
        AddAuthorization(services);
        AddHealthChecks(services);
    }
    
    private static void AddMediator(IServiceCollection services)
    {
        // Register our custom Mediator implementation
        services.AddMediator(
            typeof(UserService.Application.Commands.Users.CreateUser.CreateUserCommand).Assembly, // Application assembly
            typeof(BuildingBlocks.Abstractions.IMediator).Assembly, // BuildingBlocks assembly
            typeof(BuildingBlocks.Mediator.Mediator).Assembly // Mediator implementation assembly
        );
    }
    
    /// <summary>
    /// Configura a documentação Swagger/OpenAPI para a API
    /// </summary>
    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            // Define informações básicas da documentação da API
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "B-Commerce User Management API",
                Version = "v1",
                Description = "API for user authentication and management using Keycloak"
            });
            
            // Configura autenticação JWT no Swagger UI
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            // Aplica o esquema de segurança JWT a todos os endpoints documentados
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
    
    /// <summary>
    /// Configura CORS (Cross-Origin Resource Sharing) para permitir acesso de diferentes origens
    /// </summary>
    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                // Obtém origens permitidas da configuração ou usa valores padrão para desenvolvimento
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                   ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:4200" };
                
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()      // Permite qualquer cabeçalho HTTP
                    .AllowAnyMethod()      // Permite qualquer método HTTP
                    .AllowCredentials();   // Permite envio de cookies e credenciais
            });
        });
    }
    
    /// <summary>
    /// Configura autenticação JWT baseada em tokens do Keycloak
    /// </summary>
    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // Carrega configurações do Keycloak
        var keycloakSettings = configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
                              ?? throw new InvalidOperationException("Configurações do Keycloak não encontradas");
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Define a autoridade que emite os tokens (servidor Keycloak)
                options.Authority = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}";
                
                // Define o público-alvo esperado nos tokens (client ID do backend)
                options.Audience = keycloakSettings.BackendClientId;
                
                // Configura se HTTPS é obrigatório (desabilitado em desenvolvimento)
                options.RequireHttpsMetadata = configuration.GetValue<bool>("Security:RequireHttps");
                
                // Parâmetros de validação do token JWT
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,  // Valida se o token foi emitido pelo Keycloak correto
                    ValidIssuer = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}",
                    ValidateAudience = true,  // Valida se o token é destinado a esta API
                    ValidAudience = keycloakSettings.BackendClientId,
                    ValidateLifetime = true,  // Verifica se o token não expirou
                    ValidateIssuerSigningKey = true,  // Valida a assinatura do token
                    ClockSkew = TimeSpan.FromMinutes(5),  // Tolerância para diferenças de relógio
                    RoleClaimType = ClaimTypes.Role,  // Define o claim usado para roles
                    NameClaimType = "preferred_username"  // Define o claim usado para nome do usuário
                };

                // Eventos de autenticação para logging e debugging
                options.Events = new JwtBearerEvents
                {
                    // Executado quando um token é validado com sucesso
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var userId = context.Principal?.FindFirst("sub")?.Value;
                        logger.LogDebug("Token validated for user: {UserId}", userId);
                        return Task.CompletedTask;
                    },
                    // Executado quando a autenticação falha
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("JWT authentication failed: {Exception}", context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });
    }
    
    /// <summary>
    /// Configura políticas de autorização baseadas em roles
    /// </summary>
    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Política para administradores do sistema
            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("admin", "realm-admin");
            });
            
            // Política para usuários comuns
            options.AddPolicy("UserPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("user", "admin", "realm-admin");
            });
            
            // Política para gerentes
            options.AddPolicy("ManagerPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("manager", "admin", "realm-admin");
            });
        });
    }
    
    /// <summary>
    /// Configura health checks para monitoramento da aplicação
    /// </summary>
    private static void AddHealthChecks(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("keycloak", () =>
            {
                // Health check básico do Keycloak
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Keycloak connection configured");
            });
    }
}