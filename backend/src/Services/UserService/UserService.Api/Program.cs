// ============================================================================
// B-Commerce User Management API - Program.cs
// ============================================================================
// Este arquivo configura e inicializa a API de gerenciamento de usuários
// do sistema B-Commerce. As configurações específicas foram organizadas
// em arquivos de injeção de dependência para melhor separação de responsabilidades.
// ============================================================================

using System.ComponentModel.DataAnnotations;
using UserService.Api.Configurations;
using UserService.Api.Endpoints;
using UserService.Application.Services;

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

// ============================================================================
// REGISTRO DE CONFIGURAÇÕES CUSTOMIZADAS
// ============================================================================
// Adiciona configurações de infraestrutura (banco de dados, Keycloak, logging, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Adiciona configurações da camada de aplicação (JWT, autorização, Swagger, CORS, health checks, etc.)
builder.Services.AddApplication(builder.Configuration);

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
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    // Previne que a página seja exibida em frames (proteção contra clickjacking)
    context.Response.Headers["X-Frame-Options"] = "DENY";

    // Habilita proteção XSS do navegador
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    // Controla informações de referrer enviadas
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Em produção, força uso de HTTPS por 1 ano
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
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
app.MapAuthEndpoints();

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
app.MapGet("/api", (IConfiguration configuration) =>
{
    // Carrega configurações do Keycloak para informações da API
    var keycloakSettings = configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>()
                          ?? throw new InvalidOperationException("Configurações do Keycloak não encontradas");

    return new
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
    };
}).WithName("ApiInfo").WithTags("General");

// ============================================================================
// INICIALIZAÇÃO DA APLICAÇÃO
// ============================================================================

// Logging de inicialização para monitoramento
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("B-Commerce User Management API starting up...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

// Carrega configurações do Keycloak para logging de inicialização
var keycloakConfig = builder.Configuration.GetSection(KeycloakSettings.SectionName).Get<KeycloakSettings>();
if (keycloakConfig != null)
{
    logger.LogInformation("Keycloak URL: {KeycloakUrl}", keycloakConfig.Url);
    logger.LogInformation("Keycloak Realm: {Realm}", keycloakConfig.Realm);
}

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