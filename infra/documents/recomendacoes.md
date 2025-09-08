# E-commerce Microservices - Revisão de Arquitetura e Recomendações

## 🎯 Visão Geral da Arquitetura

### Serviços Identificados

1. **Identity Service** - Gestão de perfis de usuário (complementa Keycloak)
2. **Catalog Service** - Catálogo de produtos e categorias
3. **Inventory Service** - Gestão de estoque
4. **Order Service** - Gestão de pedidos e endereços
5. **Payment Service** - Processamento de pagamentos
6. **Notification Service** - Envio de notificações
7. **API Gateway** - Ponto de entrada único

## ⚠️ Problemas Arquiteturais Identificados

### 1. **Acoplamento entre Order e Inventory Service**

```csharp
// Problema: Order Service fazendo chamadas síncronas para Inventory
public void ReserveStock(int quantity, Guid orderId)
{
    // Isso cria acoplamento temporal e de disponibilidade
}
```

**Solução Recomendada**: Usar padrão **Saga** para coordenar transações distribuídas.

### 2. **Falta de Eventual Consistency**

O modelo atual assume consistência forte entre serviços, o que pode causar problemas de performance e disponibilidade.

### 3. **Ausência de Circuit Breaker Pattern**

Não há proteção contra falhas em cascata entre serviços.

### 4. **Shared Database Anti-pattern**

Alguns serviços podem estar compartilhando dados, violando a autonomia dos microserviços.

## 🔧 Recomendações de Melhoria

### 1. **Implementar Padrão Saga para Pedidos**

```csharp
// Order Saga Coordinator
public class OrderSaga
{
    public async Task<OrderResult> ProcessOrder(CreateOrderCommand command)
    {
        var saga = new OrderProcessingSaga(command.OrderId);

        // Step 1: Validate products (Catalog Service)
        await saga.ExecuteStep("ValidateProducts", async () =>
        {
            return await _catalogService.ValidateProductsAsync(command.Items);
        });

        // Step 2: Reserve inventory (Inventory Service)
        await saga.ExecuteStep("ReserveInventory", async () =>
        {
            return await _inventoryService.ReserveStockAsync(command.Items);
        });

        // Step 3: Create order (Order Service)
        await saga.ExecuteStep("CreateOrder", async () =>
        {
            return await _orderService.CreateOrderAsync(command);
        });

        // Step 4: Process payment (Payment Service)
        await saga.ExecuteStep("ProcessPayment", async () =>
        {
            return await _paymentService.ProcessPaymentAsync(command.Payment);
        });

        return saga.Complete();
    }
}
```

### 2. **Event Sourcing para Auditoria Completa**

```csharp
// Event Store para rastreabilidade completa
public class OrderEventStore
{
    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events)
    {
        foreach (var @event in events)
        {
            await _eventStore.AppendEventAsync(aggregateId, @event);
        }
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        return await _eventStore.GetEventsAsync(aggregateId);
    }
}
```

### 3. **CQRS com Read Models Otimizados**

```csharp
// Read Model para consultas otimizadas
public class ProductReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } // Desnormalizado do Inventory Service
    public string CategoryName { get; set; } // Desnormalizado
    public List<string> ImageUrls { get; set; }

    // Índices otimizados para busca
    [SearchableField]
    public string SearchText { get; set; } // Name + Description + Brand
}
```

### 4. **API Gateway Avançado com Rate Limiting**

```csharp
public class ApiGatewayConfiguration
{
    public void ConfigureRoutes(RouteBuilder routes)
    {
        routes.MapRoute("catalog", "/api/catalog/{**catch-all}")
              .To("http://catalog-service:5001")
              .AddRateLimiting(100, TimeSpan.FromMinutes(1))
              .AddCircuitBreaker(5, TimeSpan.FromSeconds(30))
              .RequireAuthentication();

        routes.MapRoute("orders", "/api/orders/{**catch-all}")
              .To("http://order-service:5002")
              .AddRateLimiting(50, TimeSpan.FromMinutes(1))
              .RequireAuthentication()
              .RequireRole("Customer");
    }
}
```

## 🔍 Padrões Adicionais Recomendados

### 1. **Outbox Pattern para Eventos Confiáveis**

```csharp
public class OutboxEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Processed { get; set; }
}

// Transação atômica: salvar entidade + evento
using var transaction = await _dbContext.Database.BeginTransactionAsync();
_dbContext.Orders.Add(order);
_dbContext.OutboxEvents.Add(new OutboxEvent
{
    EventType = nameof(OrderCreatedEvent),
    EventData = JsonSerializer.Serialize(orderCreatedEvent)
});
await _dbContext.SaveChangesAsync();
await transaction.CommitAsync();
```

### 2. **Health Checks Distribuídos**

```csharp
public class ServiceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context)
    {
        var dbHealth = await CheckDatabaseAsync();
        var messageQueueHealth = await CheckMessageQueueAsync();
        var cacheHealth = await CheckCacheAsync();

        if (dbHealth && messageQueueHealth && cacheHealth)
            return HealthCheckResult.Healthy();

        return HealthCheckResult.Unhealthy("Service dependencies failed");
    }
}
```

### 3. **Distributed Lock para Operações Críticas**

```csharp
public class InventoryService
{
    public async Task ReserveStockAsync(Guid productId, int quantity)
    {
        var lockKey = $"stock-lock:{productId}";

        using var distributedLock = await _redisLock.AcquireLockAsync(
            lockKey,
            TimeSpan.FromSeconds(30)
        );

        if (!distributedLock.IsAcquired)
            throw new ConcurrentModificationException("Stock is being modified by another process");

        // Operação crítica de estoque
        var stock = await _stockRepository.GetByProductIdAsync(productId);
        stock.ReserveStock(quantity, orderId);
        await _stockRepository.UpdateAsync(stock);
    }
}
```

## 📊 Métricas e Observabilidade

### 1. **OpenTelemetry para Tracing Distribuído**

```csharp
public class TracingConfiguration
{
    public void ConfigureTracing(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddRedisInstrumentation()
                .AddJaegerExporter();
        });
    }
}
```

### 2. **Business Metrics**

```csharp
public class BusinessMetrics
{
    private readonly IMetrics _metrics;

    public void RecordOrderCreated(decimal orderValue)
    {
        _metrics.CreateCounter<long>("orders_created_total").Add(1);
        _metrics.CreateHistogram<double>("order_value").Record(orderValue);
    }

    public void RecordStockReservation(Guid productId, int quantity)
    {
        _metrics.CreateCounter<long>("stock_reservations_total")
                .Add(1, new("product_id", productId.ToString()));
    }
}
```

## 🚀 Estratégia de Deployment

### 1. **Blue-Green Deployment**

```yaml
# kubernetes/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: catalog-service-blue
spec:
  replicas: 3
  selector:
    matchLabels:
      app: catalog-service
      version: blue
  template:
    metadata:
      labels:
        app: catalog-service
        version: blue
    spec:
      containers:
        - name: catalog-service
          image: catalog-service:v1.2.0
          ports:
            - containerPort: 5000
```

### 2. **Database Migration Strategy**

```csharp
public class DatabaseMigrationService
{
    public async Task MigrateAsync()
    {
        // Backward compatible migrations only
        await _dbContext.Database.MigrateAsync();

        // Seed data if needed
        await SeedDataAsync();
    }

    private async Task SeedDataAsync()
    {
        if (!await _dbContext.Categories.AnyAsync())
        {
            // Seed initial categories
        }
    }
}
```

## 🔒 Segurança Avançada

### 1. **JWT com Refresh Tokens**

```csharp
public class TokenService
{
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(refreshToken);
        var userId = principal.FindFirst("sub")?.Value;

        var user = await _userService.GetByIdAsync(userId);
        if (user?.RefreshToken != refreshToken)
            throw new SecurityException("Invalid refresh token");

        return await GenerateTokenAsync(user);
    }
}
```

### 2. **API Rate Limiting por Usuário**

```csharp
public class UserRateLimitMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var userId = context.User.FindFirst("sub")?.Value;
        var endpoint = context.Request.Path;
        var key = $"ratelimit:{userId}:{endpoint}";

        var currentCount = await _redis.GetAsync<int>(key);
        if (currentCount >= GetLimitForEndpoint(endpoint))
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        await _redis.IncrementAsync(key, TimeSpan.FromMinutes(1));
        await next(context);
    }
}
```

## 📝 Testing Strategy

### 1. **Contract Testing**

```csharp
[Test]
public async Task CatalogService_Should_Return_Valid_Product_Schema()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/products/123");
    var json = await response.Content.ReadAsStringAsync();

    // Assert
    var schema = JSchema.Parse(ProductSchema);
    var product = JObject.Parse(json);

    product.IsValid(schema).Should().BeTrue();
}
```

### 2. **Chaos Engineering**

```csharp
public class ChaosTestingService
{
    public async Task SimulateServiceFailure(string serviceName, TimeSpan duration)
    {
        // Simulate network partition
        await _networkService.BlockTrafficAsync(serviceName, duration);

        // Verify system resilience
        var healthCheck = await _healthCheckService.CheckAsync();
        healthCheck.Status.Should().Be(HealthStatus.Degraded);
    }
}
```

## 🎯 Próximos Passos Recomendados

1. **Prioridade Alta**:

   - Implementar Saga pattern para pedidos
   - Adicionar Circuit Breaker no API Gateway
   - Implementar Outbox pattern

2. **Prioridade Média**:

   - Event Sourcing para auditoria
   - CQRS com read models
   - Distributed tracing

3. **Prioridade Baixa**:
   - Chaos engineering
   - Advanced caching strategies
   - ML-based recommendations

## 💡 Considerações de Performance

### Cache Strategy

- **L1**: In-memory cache (5 min TTL)
- **L2**: Redis distributed cache (1 hour TTL)
- **L3**: CDN for static content (24 hours TTL)

### Database Optimization

- Read replicas para consultas
- Partitioning para tabelas grandes (orders, stock_movements)
- Índices otimizados para queries frequentes

### Async Processing

- Background jobs para notificações
- Batch processing para relatórios
- Queue-based communication entre serviços
