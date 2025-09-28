var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços do YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Adicionar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Adicionar Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("UserService", () => 
    {
        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync("http://localhost:5187/health").Result;
            return response.IsSuccessStatusCode ? 
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy() : 
                Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
        }
        catch
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
        }
    });

var app = builder.Build();

// Usar CORS
app.UseCors("AllowFrontend");

// Mapear Health Checks
app.MapHealthChecks("/health");

// Usar YARP Reverse Proxy
app.MapReverseProxy();

app.MapGet("/", () => "API Gateway is running!");

app.Run();