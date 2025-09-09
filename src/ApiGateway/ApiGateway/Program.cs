// Importa o namespace para autenticação JWT Bearer

using Microsoft.AspNetCore.Authentication.JwtBearer;
// Importa o namespace para validação de tokens JWT
using Microsoft.IdentityModel.Tokens;
// Importa o namespace para manipulação de texto e encoding
using System.Text;

// Cria uma instância do builder para configurar a aplicação web
var builder = WebApplication.CreateBuilder(args);

// Configurar CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", // Angular
                "http://localhost:3000", // React (se usar)
                "http://localhost:8080" // Outras apps
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    // Política mais permissiva para desenvolvimento
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configurar autenticação JWT (JSON Web Token)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    // Adiciona o esquema de autenticação JWT Bearer
    .AddJwtBearer(options =>
    {
        // Obtém as configurações JWT do arquivo appsettings.json
        var jwtSettings = builder.Configuration.GetSection("Jwt");

        // Define a autoridade que emite os tokens (servidor de identidade)
        options.Authority = jwtSettings["Authority"];
        // Define a audiência esperada no token
        options.Audience = jwtSettings["Audience"];
        // Define se HTTPS é obrigatório para metadados (padrão: true)
        options.RequireHttpsMetadata = bool.Parse(jwtSettings["RequireHttpsMetadata"] ?? "true");

        // Configura os parâmetros de validação do token
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Valida se o emissor do token é confiável
            ValidateIssuer = true,
            // Valida se a audiência do token está correta
            ValidateAudience = true,
            // Valida se o token não expirou
            ValidateLifetime = true,
            // Valida a chave de assinatura do token
            ValidateIssuerSigningKey = true,
            // Remove a tolerância de tempo para expiração (mais rigoroso)
            ClockSkew = TimeSpan.Zero
        };

        // Configura eventos personalizados para autenticação
        options.Events = new JwtBearerEvents
        {
            // Evento disparado quando a autenticação falha
            OnAuthenticationFailed = context =>
            {
                // Registra no console o erro de autenticação
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                // Retorna uma tarefa completada
                return Task.CompletedTask;
            },
            // Evento disparado quando o token é validado com sucesso
            OnTokenValidated = context =>
            {
                // Registra no console o usuário autenticado
                Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
                // Retorna uma tarefa completada
                return Task.CompletedTask;
            }
        };
    });

// Adiciona serviços de autorização ao container DI
builder.Services.AddAuthorization();

// Configurar YARP (Yet Another Reverse Proxy)
builder.Services.AddReverseProxy()
    // Carrega a configuração do YARP do arquivo appsettings.json
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Adicionar health checks para monitoramento da saúde da aplicação
builder.Services.AddHealthChecks();

// Adicionar e configurar logging
builder.Services.AddLogging(logging =>
{
    // Adiciona logging para o console
    logging.AddConsole();
    // Adiciona logging para debug (Visual Studio Output)
    logging.AddDebug();
});

// Constrói a aplicação web com todas as configurações
var app = builder.Build();

// Configure the HTTP request pipeline
// Verifica se está em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    // Adiciona página de exceções detalhadas para desenvolvimento
    app.UseDeveloperExceptionPage();
}

// Middleware pipeline - ordem é importante!
// Aplica a política CORS configurada anteriormente
app.UseCors("AllowFrontend");

// Adiciona middleware de autenticação
app.UseAuthentication();
// Adiciona middleware de autorização (deve vir após autenticação)
app.UseAuthorization();

// Health check endpoint - mapeia rota para verificação de saúde
app.MapHealthChecks("/health");

// Endpoint de informações do gateway
// Mapeia rota raiz que retorna informações sobre o serviço
app.MapGet("/", () => new
{
    // Nome do serviço
    Service = "BCommerce API Gateway",
    // Versão do serviço
    Version = "1.0.0",
    // Status atual
    Status = "Running",
    // Timestamp UTC atual
    Timestamp = DateTime.UtcNow
});

// Configurar YARP reverse proxy - mapeia todas as rotas configuradas
app.MapReverseProxy();

// Inicia a aplicação e fica escutando requisições
app.Run();