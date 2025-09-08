var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços de health check
builder.Services.AddHealthChecks();

var app = builder.Build();

// Rota principal
app.MapGet("/", () => "Hello World!");

// Rota de health check
app.MapHealthChecks("/health");

// Rota de teste para API Gateway
app.MapGet("/api/identity", () => new
{
    Service = "Identity Service",
    Status = "Running",
    Timestamp = DateTime.UtcNow,
    Message = "Rota de teste acessada com sucesso através do API Gateway!"
});

// Rota adicional com parâmetro
app.MapGet("/api/identity/test/{id}", (string id) => new
{
    Service = "Identity Service",
    TestId = id,
    Status = "Success",
    Timestamp = DateTime.UtcNow,
    Message = $"Teste executado para ID: {id}"
});

app.Run();