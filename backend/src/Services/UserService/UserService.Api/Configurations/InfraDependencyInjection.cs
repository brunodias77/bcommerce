using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data;
using BuildingBlocks.Abstractions;
using UserService.Infrastructure.Services;
using UserService.Infrastructure.Services.Interfaces;
using UserService.Application.Services;
using UserService.Application.Services.Interfaces;

namespace UserService.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDatabase(services, configuration);
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

    private static void AddServices(IServiceCollection services)
    {
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Repositórios
        // services.AddScoped<IUserRepository, UserRepository>();
        
        // Serviços de infraestrutura
        services.AddScoped<IKeycloakService, KeycloakService>();
        services.AddHttpClient<KeycloakService>();
        services.AddScoped<IPasswordEncripter, PasswordEncripter>();
        // services.AddScoped<IJwtService, JwtService>();
        // services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        // Cache Redis (se necessário)
        // services.AddStackExchangeRedisCache(options =>
        // {
        //     options.Configuration = configuration.GetConnectionString("Redis");
        // });
    }
}