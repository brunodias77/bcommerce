using BuildingBlocks.Abstractions;
using CatalogService.Infrastructure.Abstractions;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FluentMigrator.Runner;
using System.Reflection;

namespace CatalogService.Api.Configurations;

public static class InfraDependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuração do Entity Framework Core com PostgreSQL
        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
            
            // Configurações adicionais para desenvolvimento
            if (configuration.GetValue<bool>("Logging:LogLevel:Microsoft.EntityFrameworkCore") == true)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "catalog-database",
                tags: new[] { "database", "postgresql" });

        // Unit of Work pattern
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICatalogUnitOfWork, UnitOfWork>();
        
        // FluentMigrator configuration
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(typeof(CatalogService.Infrastructure.Migrations.Migration_20240101001_CreateCategoriesTable).Assembly)
                .For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }
}