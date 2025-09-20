// ============================================================================
// B-Commerce User Management API - Program.cs
// ============================================================================
// Este arquivo configura e inicializa a API de gerenciamento de usuários
// do sistema B-Commerce, incluindo autenticação via Keycloak, autorização,
// CORS, documentação Swagger e endpoints da aplicação.
// ============================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.Api.Configurations;
using UserService.Api.Endpoints;
using UserService.Application.Services;
using UserService.Application.Services.Interfaces;

// ============================================================================
// CONFIGURAÇÃO DO BUILDER DA APLICAÇÃO
// ============================================================================
// Cria o builder da aplicação web com configurações padrão do ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CONFIGURAÇÃO DE SERVIÇOS DO CONTAINER DE DEPENDÊNCIAS
// ============================================================================

// Adiciona suporte a controllers MVC (caso necessário para endpoints futuros)
builder.Services.AddControllers();

// Habilita a exploração de endpoints para documentação automática da API
builder.Services.AddEndpointsApiExplorer();
// Configuração do Swagger/OpenAPI para documentação interativa da API
builder.Services.AddSwaggerGen(c =>
{
    // Define informações básicas da documentação da API
    c.SwaggerDoc("v1", new() 
    { 
        Title = "B-Commerce User Management API", 
        Version = "v1",
        Description = "API for user authentication and management using Keycloak"
    });
    
    // Configura autenticação JWT no Swagger UI
    // Permite que usuários insiram tokens JWT diretamente na interface
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    // Aplica o esquema de segurança JWT a todos os endpoints documentados
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================================================
// CONFIGURAÇÃO DE CORS (Cross-Origin Resource Sharing)
// ============================================================================
// Permite que aplicações frontend de diferentes origens acessem esta API
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        // Obtém origens permitidas da configuração ou usa valores padrão para desenvolvimento
        // Suporta React (3000), Vite (5173) e Angular (4200)
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                             ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:4200" };
            
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()      // Permite qualquer cabeçalho HTTP
            .AllowAnyMethod()      // Permite qualquer método HTTP (GET, POST, PUT, DELETE, etc.)
            .AllowCredentials();   // Permite envio de cookies e credenciais
    });
});


// ============================================================================
// CONFIGURAÇÃO DO KEYCLOAK
// ============================================================================
// Carrega as configurações do Keycloak do appsettings.json
// Keycloak é usado como provedor de identidade e autenticação
var keycloakSettings = builder.Configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
                       ?? throw new InvalidOperationException("Configurações do Keycloak não encontradas");

// ============================================================================
// REGISTRO DE CONFIGURAÇÕES CUSTOMIZADAS
// ============================================================================
// Adiciona configurações de infraestrutura (banco de dados, cache, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Adiciona configurações da camada de aplicação (serviços, validações, etc.)
builder.Services.AddApplication(builder.Configuration);

// Registra as configurações do Keycloak no container de DI
builder.Services.Configure<KeycloakSettings>(
    builder.Configuration.GetSection(KeycloakSettings.SectionName));

// Configura HttpClient para comunicação com o servidor Keycloak
// Usado para operações administrativas como criação de usuários
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>();

// Registra o serviço do Keycloak com escopo por requisição
builder.Services.AddScoped<IKeycloakService, KeycloakService>();

// ============================================================================
// CONFIGURAÇÃO DE AUTENTICAÇÃO JWT
// ============================================================================
// Configura autenticação baseada em tokens JWT emitidos pelo Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Define a autoridade que emite os tokens (servidor Keycloak)
        options.Authority = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}";
        
        // Define o público-alvo esperado nos tokens (client ID do backend)
        options.Audience = keycloakSettings.BackendClientId;
        
        // Configura se HTTPS é obrigatório (desabilitado em desenvolvimento)
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Security:RequireHttps");
        
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

// ============================================================================
// CONFIGURAÇÃO DE POLÍTICAS DE AUTORIZAÇÃO
// ============================================================================
// Define políticas baseadas em roles para controle de acesso granular
builder.Services.AddAuthorization(options =>
{
    // Política para administradores do sistema
    // Permite acesso total a funcionalidades administrativas
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();  // Usuário deve estar autenticado
        policy.RequireRole("admin", "realm-admin");  // Deve ter role admin ou realm-admin
    });
    
    // Política para usuários comuns
    // Permite acesso a funcionalidades básicas do sistema
    options.AddPolicy("UserPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("user", "admin", "realm-admin");  // Usuário, admin ou realm-admin
    });
    
    // Política para gerentes
    // Permite acesso a funcionalidades de gerenciamento intermediário
    options.AddPolicy("ManagerPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("manager", "admin", "realm-admin");  // Manager, admin ou realm-admin
    });
});

// ============================================================================
// CONFIGURAÇÃO DE HEALTH CHECKS
// ============================================================================
// Monitora a saúde da aplicação e suas dependências
builder.Services.AddHealthChecks()
    .AddCheck("keycloak", () =>
    {
        // Health check básico do Keycloak
        // Em produção, considere fazer ping real no servidor Keycloak
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Keycloak connection configured");
    });

// ============================================================================
// CONFIGURAÇÃO DE LOGGING
// ============================================================================
// Configura provedores de log para monitoramento e debugging
builder.Logging.ClearProviders();  // Remove provedores padrão
builder.Logging.AddConsole();       // Adiciona logging no console

// Em desenvolvimento, adiciona logging de debug para mais detalhes
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}



// ============================================================================
// CONSTRUÇÃO DA APLICAÇÃO
// ============================================================================
var app = builder.Build();

// ============================================================================
// CONFIGURAÇÃO DO PIPELINE DE MIDDLEWARE (AMBIENTE DE DESENVOLVIMENTO)
// ============================================================================
if (app.Environment.IsDevelopment())
{
    // Habilita a documentação Swagger apenas em desenvolvimento
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "B-Commerce User Management API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI na raiz (/) para facilitar acesso
    });
    
    // Página de exceções detalhadas para debugging
    app.UseDeveloperExceptionPage();
}
else
{
    // ============================================================================
    // CONFIGURAÇÃO DO PIPELINE DE MIDDLEWARE (AMBIENTE DE PRODUÇÃO)
    // ============================================================================
    // Tratamento genérico de exceções em produção (sem detalhes sensíveis)
    app.UseExceptionHandler("/error");
    
    // HTTP Strict Transport Security - força uso de HTTPS
    app.UseHsts();
}

// ============================================================================
// MIDDLEWARE DE CABEÇALHOS DE SEGURANÇA
// ============================================================================
// Adiciona cabeçalhos HTTP de segurança para proteger contra ataques comuns
app.Use(async (context, next) =>
{
    // Previne ataques de MIME type sniffing
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    
    // Previne que a página seja exibida em frames (proteção contra clickjacking)
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    
    // Habilita proteção XSS do navegador
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    
    // Controla informações de referrer enviadas
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Em produção, força uso de HTTPS por 1 ano
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    
    await next();
});


// ============================================================================
// PIPELINE DE MIDDLEWARE - ORDEM CRÍTICA
// ============================================================================
// A ordem dos middlewares é importante para o funcionamento correto

// 1. Redirecionamento HTTPS (deve vir antes de outros middlewares)
app.UseHttpsRedirection();

// 2. CORS - permite requisições cross-origin
app.UseCors("DefaultPolicy");

// 3. Autenticação - identifica o usuário através do token JWT
app.UseAuthentication();

// 4. Autorização - verifica se o usuário tem permissão para acessar o recurso
app.UseAuthorization();

// ============================================================================
// TRATAMENTO GLOBAL DE EXCEÇÕES
// ============================================================================
// Captura e trata exceções não tratadas em toda a aplicação
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        
        // Registra a exceção nos logs para monitoramento
        if (exceptionFeature?.Error != null)
        {
            logger.LogError(exceptionFeature.Error, "Unhandled exception occurred");
        }
        
        // Configura resposta de erro padronizada
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        // Retorna detalhes da exceção apenas em desenvolvimento
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Internal Server Error",
            status = 500,
            detail = app.Environment.IsDevelopment() 
                ? exceptionFeature?.Error?.Message  // Detalhes em desenvolvimento
                : "An error occurred while processing your request"  // Mensagem genérica em produção
        }));
    });
});

// ============================================================================
// MAPEAMENTO DE ENDPOINTS
// ============================================================================

// Endpoint de health check para monitoramento da aplicação
app.MapHealthChecks("/health");

// Endpoints de autenticação do Keycloak
// Inclui registro, login, logout e gerenciamento de usuários
app.MapAuthEndpointsKeycloak();

// ============================================================================
// ENDPOINTS DE INFORMAÇÃO DA API
// ============================================================================

// Endpoint raiz - fornece informações básicas sobre a API
app.MapGet("/", () => new
{
    service = "B-Commerce User Management API",
    version = "1.0.0",
    status = "running",
    timestamp = DateTimeOffset.UtcNow,
    endpoints = new
    {
        swagger = "/swagger",      // Documentação interativa
        health = "/health",        // Status de saúde da aplicação
        auth = "/api/auth",        // Endpoints de autenticação
        users = "/api/users",      // Endpoints de usuários
        admin = "/api/admin"       // Endpoints administrativos
    }
}).WithName("Root").WithTags("General");

// Endpoint de informações detalhadas da API
app.MapGet("/api", () => new
{
    name = "B-Commerce User Management API",
    version = "1.0.0",
    description = "RESTful API for user authentication and management using Keycloak",
    documentation = "/swagger",
    keycloak = new
    {
        realm = keycloakSettings.Realm,
        // URLs do Keycloak para integração frontend
        authUrl = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}/protocol/openid-connect/auth",
        tokenUrl = $"{keycloakSettings.Url}/realms/{keycloakSettings.Realm}/protocol/openid-connect/token"
    }
}).WithName("ApiInfo").WithTags("General");

// ============================================================================
// INICIALIZAÇÃO DA APLICAÇÃO
// ============================================================================

// Logging de inicialização para monitoramento
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("B-Commerce User Management API starting up...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Keycloak URL: {KeycloakUrl}", keycloakSettings.Url);
logger.LogInformation("Keycloak Realm: {Realm}", keycloakSettings.Realm);

// Inicia a aplicação com tratamento de exceções de inicialização
try
{
    app.Run();
}
catch (Exception ex)
{
    // Registra falhas críticas de inicialização
    logger.LogCritical(ex, "Application startup failed");
    throw;
}