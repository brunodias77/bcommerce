using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data;
using BuildingBlocks.Abstractions;
using UserService.Application.Services;
using UserService.Application.Services.Interfaces;

namespace UserService.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddKeycloak(services, configuration);
        AddLogging(services, configuration);
        AddEmailSettings(services, configuration);
        AddServices(services);
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<UserManagementDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });
            
            // Configurações adicionais para desenvolvimento
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });
    }

    /// <summary>
    /// Configura serviços do Keycloak para integração com o provedor de identidade
    /// </summary>
    private static void AddKeycloak(IServiceCollection services, IConfiguration configuration)
    {
        // Registra as configurações do Keycloak no container de DI
        services.Configure<KeycloakSettings>(
            configuration.GetSection(KeycloakSettings.SectionName));

        // Configura HttpClient para comunicação com o servidor Keycloak
        // Usado para operações administrativas como criação de usuários
        services.AddHttpClient<IKeycloakService, KeycloakService>();

        // Registra o serviço do Keycloak com escopo por requisição
        services.AddScoped<IKeycloakService, KeycloakService>();
    }
    
    /// <summary>
    /// Configura provedores de logging para monitoramento e debugging
    /// </summary>
    private static void AddLogging(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            // Remove provedores padrão para configuração customizada
            builder.ClearProviders();
            
            // Adiciona logging no console para todas as aplicações
            builder.AddConsole();
            
            // Em desenvolvimento, adiciona logging de debug para mais detalhes
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                builder.AddDebug();
            }
        });
    }

    /// <summary>
    /// Configura as configurações de email para envio de notificações
    /// </summary>
    private static void AddEmailSettings(IServiceCollection services, IConfiguration configuration)
    {
        // Registra as configurações de email no container de DI
        services.Configure<EmailSettings>(
            configuration.GetSection(EmailSettings.SectionName));
    }

    private static void AddServices(IServiceCollection services)
    {
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Repositórios
        // services.AddScoped<IUserRepository, UserRepository>();
        
        // Serviços de infraestrutura
        services.AddScoped<IPasswordEncripter, PasswordEncripter>();
        services.AddScoped<IEmailService, EmailService>();
        // services.AddScoped<IJwtService, JwtService>();
        // services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        // Cache Redis (se necessário)
        // services.AddStackExchangeRedisCache(options =>
        // {
        //     options.Configuration = configuration.GetConnectionString("Redis");
        // });
    }
}