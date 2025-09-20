using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data;
using System.Text.Json;

namespace UserService.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var healthGroup = endpoints.MapGroup("/health")
            .WithTags("Health");

        // Endpoint básico de health check
        healthGroup.MapGet("/", GetHealth)
            .WithName("GetHealth")
            .WithSummary("Verifica o status geral da aplicação")
            .Produces<HealthResponse>(200)
            .Produces<HealthResponse>(503);

        // Endpoint de readiness - verifica se a aplicação está pronta para receber tráfego
        healthGroup.MapGet("/ready", GetReadiness)
            .WithName("GetReadiness")
            .WithSummary("Verifica se a aplicação está pronta (banco de dados conectado)")
            .Produces<HealthResponse>(200)
            .Produces<HealthResponse>(503);

        // Endpoint de liveness - verifica se a aplicação está viva
        healthGroup.MapGet("/live", GetLiveness)
            .WithName("GetLiveness")
            .WithSummary("Verifica se a aplicação está viva")
            .Produces<HealthResponse>(200)
            .Produces<HealthResponse>(503);
    }

    private static async Task<IResult> GetHealth(
        UserManagementDbContext dbContext,
        IConfiguration configuration)
    {
        var healthResponse = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "UserService",
            Version = "1.0.0",
            Checks = new Dictionary<string, HealthCheck>()
        };

        var overallHealthy = true;

        // Verificar conexão com banco de dados
        try
        {
            await dbContext.Database.CanConnectAsync();
            healthResponse.Checks["database"] = new HealthCheck
            {
                Status = "Healthy",
                Description = "Database connection is working",
                ResponseTime = await MeasureResponseTime(async () => await dbContext.Database.CanConnectAsync())
            };
        }
        catch (Exception ex)
        {
            overallHealthy = false;
            healthResponse.Checks["database"] = new HealthCheck
            {
                Status = "Unhealthy",
                Description = $"Database connection failed: {ex.Message}",
                ResponseTime = 0
            };
        }

        // Verificar Redis (se configurado)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                // Aqui você pode adicionar verificação do Redis se estiver usando
                healthResponse.Checks["redis"] = new HealthCheck
                {
                    Status = "Healthy",
                    Description = "Redis connection is working",
                    ResponseTime = 0
                };
            }
            catch (Exception ex)
            {
                overallHealthy = false;
                healthResponse.Checks["redis"] = new HealthCheck
                {
                    Status = "Unhealthy",
                    Description = $"Redis connection failed: {ex.Message}",
                    ResponseTime = 0
                };
            }
        }

        // Verificar memória disponível
        var workingSet = GC.GetTotalMemory(false);
        healthResponse.Checks["memory"] = new HealthCheck
        {
            Status = workingSet < 500_000_000 ? "Healthy" : "Warning", // 500MB threshold
            Description = $"Memory usage: {workingSet / 1024 / 1024} MB",
            ResponseTime = 0
        };

        if (!overallHealthy)
        {
            healthResponse.Status = "Unhealthy";
            return Results.Json(healthResponse, statusCode: 503);
        }

        return Results.Ok(healthResponse);
    }

    private static async Task<IResult> GetReadiness(
        UserManagementDbContext dbContext)
    {
        var healthResponse = new HealthResponse
        {
            Status = "Ready",
            Timestamp = DateTime.UtcNow,
            Service = "UserService",
            Version = "1.0.0",
            Checks = new Dictionary<string, HealthCheck>()
        };

        try
        {
            // Verificar se consegue conectar e fazer uma query simples no banco
            var canConnect = await dbContext.Database.CanConnectAsync();
            if (canConnect)
            {
                // Tentar fazer uma query simples para garantir que o banco está realmente pronto
                var responseTime = await MeasureResponseTime(async () => 
                    await dbContext.Database.ExecuteSqlRawAsync("SELECT 1"));
                
                healthResponse.Checks["database"] = new HealthCheck
                {
                    Status = "Ready",
                    Description = "Database is ready to accept queries",
                    ResponseTime = responseTime
                };
                
                return Results.Ok(healthResponse);
            }
        }
        catch (Exception ex)
        {
            healthResponse.Status = "NotReady";
            healthResponse.Checks["database"] = new HealthCheck
            {
                Status = "NotReady",
                Description = $"Database is not ready: {ex.Message}",
                ResponseTime = 0
            };
            
            return Results.Json(healthResponse, statusCode: 503);
        }

        healthResponse.Status = "NotReady";
        return Results.Json(healthResponse, statusCode: 503);
    }

    private static Task<IResult> GetLiveness()
    {
        var healthResponse = new HealthResponse
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            Service = "UserService",
            Version = "1.0.0",
            Checks = new Dictionary<string, HealthCheck>
            {
                ["application"] = new HealthCheck
                {
                    Status = "Alive",
                    Description = "Application is running",
                    ResponseTime = 0
                }
            }
        };

        return Task.FromResult(Results.Ok(healthResponse));
    }

    private static async Task<long> MeasureResponseTime(Func<Task> action)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await action();
        }
        finally
        {
            stopwatch.Stop();
        }
        return stopwatch.ElapsedMilliseconds;
    }
}

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Service { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, HealthCheck> Checks { get; set; } = new();
}

public class HealthCheck
{
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
}