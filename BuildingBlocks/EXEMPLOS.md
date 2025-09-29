# BuildingBlock - Exemplos de Uso

Este documento apresenta exemplos práticos de como usar o BuildingBlock implementado, demonstrando Commands, Queries, Events e validações em um domínio de e-commerce.

## Estrutura do BuildingBlock

O BuildingBlock fornece:
- **Abstrações**: `Entity`, `AggregateRoot`, `ValueObject` com suporte a Domain Events e validações
- **Mediator Pattern**: `IRequest`, `IRequestHandler`, `INotification`, `INotificationHandler`
- **Validações**: `Error`, `ValidationException`, `ValidationHandler`

## 1. Value Objects com Validação

### Email Value Object
```csharp
using BuildingBlock.Abstractions;
using BuildingBlock.Validations;
using System.Text.RegularExpressions;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        Value = value?.Trim() ?? string.Empty;
        ValidateAndThrow();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override ValidationHandler Validate()
    {
        var handler = new ValidationHandler();

        if (string.IsNullOrWhiteSpace(Value))
            handler.Add("EMAIL_REQUIRED", "Email é obrigatório");
        else if (!IsValidEmail(Value))
            handler.Add("EMAIL_INVALID", "Email deve ter um formato válido");

        return handler;
    }

    private static bool IsValidEmail(string email)
    {
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}
```

### Money Value Object
```csharp
using BuildingBlock.Abstractions;
using BuildingBlock.Validations;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency?.ToUpper() ?? "BRL";
        ValidateAndThrow();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override ValidationHandler Validate()
    {
        var handler = new ValidationHandler();

        if (Amount < 0)
            handler.Add("MONEY_NEGATIVE", "Valor não pode ser negativo");

        if (string.IsNullOrWhiteSpace(Currency))
            handler.Add("CURRENCY_REQUIRED", "Moeda é obrigatória");

        return handler;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Não é possível somar valores de moedas diferentes");
        
        return new Money(Amount + other.Amount, Currency);
    }
}
```

## 2. Entities com Domain Events

### Cliente Entity
```csharp
using BuildingBlock.Abstractions;
using BuildingBlock.Validations;

public class Cliente : Entity
{
    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public bool Ativo { get; private set; }

    public Cliente(string nome, Email email)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        DataCadastro = DateTime.UtcNow;
        Ativo = true;

        ValidateAndThrow();
        AddDomainEvent(new ClienteCriadoEvent(Id, Nome, Email.Value));
    }

    public void AtualizarNome(string novoNome)
    {
        var nomeAnterior = Nome;
        Nome = novoNome;
        
        ValidateAndThrow();
        AddDomainEvent(new ClienteNomeAtualizadoEvent(Id, nomeAnterior, novoNome));
    }

    public void Desativar()
    {
        if (!Ativo) return;
        
        Ativo = false;
        AddDomainEvent(new ClienteDesativadoEvent(Id, Nome));
    }

    protected override ValidationHandler ValidateEntity()
    {
        var handler = new ValidationHandler();

        if (string.IsNullOrWhiteSpace(Nome))
            handler.Add("NOME_REQUIRED", "Nome do cliente é obrigatório");
        else if (Nome.Length < 2)
            handler.Add("NOME_TOO_SHORT", "Nome deve ter pelo menos 2 caracteres");
        else if (Nome.Length > 100)
            handler.Add("NOME_TOO_LONG", "Nome não pode ter mais de 100 caracteres");

        if (Email == null)
            handler.Add("EMAIL_REQUIRED", "Email do cliente é obrigatório");

        return handler;
    }
}
```

### Produto Entity
```csharp
using BuildingBlock.Abstractions;
using BuildingBlock.Validations;

public class Produto : Entity
{
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public Money Preco { get; private set; }
    public int EstoqueAtual { get; private set; }
    public bool Ativo { get; private set; }

    public Produto(string nome, string descricao, Money preco, int estoqueInicial)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        EstoqueAtual = estoqueInicial;
        Ativo = true;

        ValidateAndThrow();
        AddDomainEvent(new ProdutoCriadoEvent(Id, Nome, Preco.Amount));
    }

    public void AtualizarPreco(Money novoPreco)
    {
        var precoAnterior = Preco;
        Preco = novoPreco;
        
        ValidateAndThrow();
        AddDomainEvent(new ProdutoPrecoAtualizadoEvent(Id, precoAnterior.Amount, novoPreco.Amount));
    }

    public void ReduzirEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        if (EstoqueAtual < quantidade)
            throw new InvalidOperationException("Estoque insuficiente");

        EstoqueAtual -= quantidade;
        AddDomainEvent(new ProdutoEstoqueReduzidoEvent(Id, quantidade, EstoqueAtual));
    }

    protected override ValidationHandler ValidateEntity()
    {
        var handler = new ValidationHandler();

        if (string.IsNullOrWhiteSpace(Nome))
            handler.Add("NOME_REQUIRED", "Nome do produto é obrigatório");
        else if (Nome.Length > 200)
            handler.Add("NOME_TOO_LONG", "Nome não pode ter mais de 200 caracteres");

        if (Preco == null)
            handler.Add("PRECO_REQUIRED", "Preço do produto é obrigatório");

        if (EstoqueAtual < 0)
            handler.Add("ESTOQUE_NEGATIVE", "Estoque não pode ser negativo");

        return handler;
    }
}
```

## 3. Aggregate Root

### Pedido Aggregate
```csharp
using BuildingBlock.Abstractions;
using BuildingBlock.Validations;

public class Pedido : AggregateRoot
{
    private readonly List<ItemPedido> _itens = new();

    public Guid ClienteId { get; private set; }
    public DateTime DataPedido { get; private set; }
    public StatusPedido Status { get; private set; }
    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();
    public Money ValorTotal => CalcularValorTotal();

    public Pedido(Guid clienteId)
    {
        Id = Guid.NewGuid();
        ClienteId = clienteId;
        DataPedido = DateTime.UtcNow;
        Status = StatusPedido.Pendente;

        ValidateAndThrow();
        AddDomainEvent(new PedidoCriadoEvent(Id, ClienteId));
    }

    public void AdicionarItem(Guid produtoId, string nomeProduto, Money precoUnitario, int quantidade)
    {
        var item = new ItemPedido(produtoId, nomeProduto, precoUnitario, quantidade);
        _itens.Add(item);

        ValidateAndThrow();
        AddDomainEvent(new ItemAdicionadoAoPedidoEvent(Id, produtoId, quantidade));
    }

    public void Confirmar()
    {
        if (Status != StatusPedido.Pendente)
            throw new InvalidOperationException("Apenas pedidos pendentes podem ser confirmados");

        Status = StatusPedido.Confirmado;
        AddDomainEvent(new PedidoConfirmadoEvent(Id, ClienteId, ValorTotal.Amount));
    }

    protected override ValidationHandler ValidateAggregate()
    {
        var handler = new ValidationHandler();

        if (ClienteId == Guid.Empty)
            handler.Add("CLIENTE_REQUIRED", "Cliente é obrigatório");

        if (!_itens.Any())
            handler.Add("ITENS_REQUIRED", "Pedido deve ter pelo menos um item");

        return handler;
    }

    private Money CalcularValorTotal()
    {
        if (!_itens.Any()) return new Money(0);
        
        return _itens
            .Select(i => i.ValorTotal)
            .Aggregate((total, valor) => total.Add(valor));
    }
}

public class ItemPedido
{
    public Guid ProdutoId { get; }
    public string NomeProduto { get; }
    public Money PrecoUnitario { get; }
    public int Quantidade { get; }
    public Money ValorTotal => new Money(PrecoUnitario.Amount * Quantidade, PrecoUnitario.Currency);

    public ItemPedido(Guid produtoId, string nomeProduto, Money precoUnitario, int quantidade)
    {
        ProdutoId = produtoId;
        NomeProduto = nomeProduto;
        PrecoUnitario = precoUnitario;
        Quantidade = quantidade;
    }
}

public enum StatusPedido
{
    Pendente,
    Confirmado,
    Cancelado,
    Entregue
}
```

## 4. Domain Events

### Events do Cliente
```csharp
using BuildingBlock.Mediator;

public record ClienteCriadoEvent(Guid ClienteId, string Nome, string Email) : INotification;

public record ClienteNomeAtualizadoEvent(Guid ClienteId, string NomeAnterior, string NovoNome) : INotification;

public record ClienteDesativadoEvent(Guid ClienteId, string Nome) : INotification;
```

### Events do Produto
```csharp
using BuildingBlock.Mediator;

public record ProdutoCriadoEvent(Guid ProdutoId, string Nome, decimal Preco) : INotification;

public record ProdutoPrecoAtualizadoEvent(Guid ProdutoId, decimal PrecoAnterior, decimal NovoPreco) : INotification;

public record ProdutoEstoqueReduzidoEvent(Guid ProdutoId, int QuantidadeReduzida, int EstoqueAtual) : INotification;
```

### Events do Pedido
```csharp
using BuildingBlock.Mediator;

public record PedidoCriadoEvent(Guid PedidoId, Guid ClienteId) : INotification;

public record ItemAdicionadoAoPedidoEvent(Guid PedidoId, Guid ProdutoId, int Quantidade) : INotification;

public record PedidoConfirmadoEvent(Guid PedidoId, Guid ClienteId, decimal ValorTotal) : INotification;
```

## 5. Commands com Validação

### Criar Cliente Command
```csharp
using BuildingBlock.Mediator;
using BuildingBlock.Validations;

public record CriarClienteCommand(string Nome, string Email) : IRequest<Guid>
{
    public ValidationHandler Validate()
    {
        var handler = new ValidationHandler();

        if (string.IsNullOrWhiteSpace(Nome))
            handler.Add("NOME_REQUIRED", "Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(Email))
            handler.Add("EMAIL_REQUIRED", "Email é obrigatório");

        return handler;
    }
}

public class CriarClienteCommandHandler : IRequestHandler<CriarClienteCommand, Guid>
{
    private readonly IClienteRepository _clienteRepository;

    public CriarClienteCommandHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Guid> HandleAsync(CriarClienteCommand request, CancellationToken cancellationToken = default)
    {
        // Validar command
        request.Validate().ThrowIfHasErrors();

        // Verificar se email já existe
        var emailExistente = await _clienteRepository.ExisteEmailAsync(request.Email);
        if (emailExistente)
            throw new ValidationException("EMAIL_ALREADY_EXISTS", "Email já está em uso");

        // Criar cliente
        var email = new Email(request.Email);
        var cliente = new Cliente(request.Nome, email);

        // Salvar
        await _clienteRepository.AdicionarAsync(cliente);
        
        return cliente.Id;
    }
}
```

### Criar Pedido Command
```csharp
using BuildingBlock.Mediator;
using BuildingBlock.Validations;

public record CriarPedidoCommand(Guid ClienteId, List<ItemPedidoDto> Itens) : IRequest<Guid>
{
    public ValidationHandler Validate()
    {
        var handler = new ValidationHandler();

        if (ClienteId == Guid.Empty)
            handler.Add("CLIENTE_REQUIRED", "Cliente é obrigatório");

        if (Itens == null || !Itens.Any())
            handler.Add("ITENS_REQUIRED", "Pedido deve ter pelo menos um item");

        return handler;
    }
}

public record ItemPedidoDto(Guid ProdutoId, int Quantidade);

public class CriarPedidoCommandHandler : IRequestHandler<CriarPedidoCommand, Guid>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMediator _mediator;

    public CriarPedidoCommandHandler(
        IPedidoRepository pedidoRepository,
        IProdutoRepository produtoRepository,
        IClienteRepository clienteRepository,
        IMediator mediator)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _clienteRepository = clienteRepository;
        _mediator = mediator;
    }

    public async Task<Guid> HandleAsync(CriarPedidoCommand request, CancellationToken cancellationToken = default)
    {
        // Validar command
        request.Validate().ThrowIfHasErrors();

        // Verificar se cliente existe
        var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId);
        if (cliente == null)
            throw new ValidationException("CLIENTE_NOT_FOUND", "Cliente não encontrado");

        // Criar pedido
        var pedido = new Pedido(request.ClienteId);

        // Adicionar itens
        foreach (var itemDto in request.Itens)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(itemDto.ProdutoId);
            if (produto == null)
                throw new ValidationException("PRODUTO_NOT_FOUND", $"Produto {itemDto.ProdutoId} não encontrado");

            pedido.AdicionarItem(produto.Id, produto.Nome, produto.Preco, itemDto.Quantidade);
        }

        // Salvar
        await _pedidoRepository.AdicionarAsync(pedido);
        
        // ⭐ PUBLICAR DOMAIN EVENTS - Os eventos são disparados automaticamente
        await pedido.PublishDomainEventsAsync(_mediator, cancellationToken);
        
        return pedido.Id;
    }
}
```

### Confirmar Pedido Command - Exemplo Completo de Disparo de Eventos
```csharp
using BuildingBlock.Mediator;
using BuildingBlock.Validations;

public record ConfirmarPedidoCommand(Guid PedidoId) : IRequest<Unit>
{
    public ValidationHandler Validate()
    {
        var handler = new ValidationHandler();

        if (PedidoId == Guid.Empty)
            handler.Add("PEDIDO_ID_REQUIRED", "ID do pedido é obrigatório");

        return handler;
    }
}

public class ConfirmarPedidoCommandHandler : IRequestHandler<ConfirmarPedidoCommand, Unit>
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMediator _mediator;

    public ConfirmarPedidoCommandHandler(
        IPedidoRepository pedidoRepository,
        IProdutoRepository produtoRepository,
        IMediator mediator)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _mediator = mediator;
    }

    public async Task<Unit> HandleAsync(ConfirmarPedidoCommand request, CancellationToken cancellationToken = default)
    {
        // 1. Validar command
        request.Validate().ThrowIfHasErrors();

        // 2. Buscar pedido
        var pedido = await _pedidoRepository.ObterPorIdAsync(request.PedidoId);
        if (pedido == null)
            throw new ValidationException("PEDIDO_NOT_FOUND", "Pedido não encontrado");

        // 3. Reduzir estoque dos produtos (isso gera eventos de domínio)
        foreach (var item in pedido.Itens)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(item.ProdutoId);
            if (produto != null)
            {
                produto.ReduzirEstoque(item.Quantidade); // Gera ProdutoEstoqueReduzidoEvent
                await _produtoRepository.AtualizarAsync(produto);
                
                // Publicar eventos do produto
                await produto.PublishDomainEventsAsync(_mediator, cancellationToken);
            }
        }

        // 4. Confirmar pedido (isso gera evento de domínio)
        pedido.Confirmar(); // Gera PedidoConfirmadoEvent

        // 5. Salvar pedido
        await _pedidoRepository.AtualizarAsync(pedido);

        // 6. ⭐ PUBLICAR DOMAIN EVENTS DO PEDIDO
        // Aqui os eventos são disparados automaticamente:
        // - PedidoConfirmadoEvent será publicado
        // - Todos os handlers registrados para este evento serão executados
        await pedido.PublishDomainEventsAsync(_mediator, cancellationToken);

        return Unit.Value;
    }
}
```

## 6. Queries com Validação

### Obter Cliente Query
```csharp
using BuildingBlock.Mediator;
using BuildingBlock.Validations;

public record ObterClienteQuery(Guid ClienteId) : IRequest<ClienteDto?>
{
    public ValidationHandler Validate()
    {
        var handler = new ValidationHandler();

        if (ClienteId == Guid.Empty)
            handler.Add("CLIENTE_ID_REQUIRED", "ID do cliente é obrigatório");

        return handler;
    }
}

public record ClienteDto(Guid Id, string Nome, string Email, DateTime DataCadastro, bool Ativo);

public class ObterClienteQueryHandler : IRequestHandler<ObterClienteQuery, ClienteDto?>
{
    private readonly IClienteRepository _clienteRepository;

    public ObterClienteQueryHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<ClienteDto?> HandleAsync(ObterClienteQuery request, CancellationToken cancellationToken = default)
    {
        // Validar query
        request.Validate().ThrowIfHasErrors();

        var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId);
        
        return cliente == null ? null : new ClienteDto(
            cliente.Id,
            cliente.Nome,
            cliente.Email.Value,
            cliente.DataCadastro,
            cliente.Ativo
        );
    }
}
```

### Listar Produtos Query
```csharp
using BuildingBlock.Mediator;
using BuildingBlock.Validations;

public record ListarProdutosQuery(int Pagina = 1, int TamanhoPagina = 10, string? Filtro = null) : IRequest<ProdutosPaginadosDto>
{
    public ValidationHandler Validate()
    {
        var handler = new ValidationHandler();

        if (Pagina <= 0)
            handler.Add("PAGINA_INVALID", "Página deve ser maior que zero");

        if (TamanhoPagina <= 0 || TamanhoPagina > 100)
            handler.Add("TAMANHO_PAGINA_INVALID", "Tamanho da página deve estar entre 1 e 100");

        return handler;
    }
}

public record ProdutosPaginadosDto(List<ProdutoDto> Produtos, int Total, int Pagina, int TamanhoPagina);
public record ProdutoDto(Guid Id, string Nome, string Descricao, decimal Preco, int EstoqueAtual, bool Ativo);

public class ListarProdutosQueryHandler : IRequestHandler<ListarProdutosQuery, ProdutosPaginadosDto>
{
    private readonly IProdutoRepository _produtoRepository;

    public ListarProdutosQueryHandler(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<ProdutosPaginadosDto> HandleAsync(ListarProdutosQuery request, CancellationToken cancellationToken = default)
    {
        // Validar query
        request.Validate().ThrowIfHasErrors();

        var (produtos, total) = await _produtoRepository.ListarPaginadoAsync(
            request.Pagina, 
            request.TamanhoPagina, 
            request.Filtro
        );

        var produtosDto = produtos.Select(p => new ProdutoDto(
            p.Id,
            p.Nome,
            p.Descricao,
            p.Preco.Amount,
            p.EstoqueAtual,
            p.Ativo
        )).ToList();

        return new ProdutosPaginadosDto(produtosDto, total, request.Pagina, request.TamanhoPagina);
    }
}
```

## 7. Event Handlers

### Handler para Cliente Criado
```csharp
using BuildingBlock.Mediator;

public class ClienteCriadoEventHandler : INotificationHandler<ClienteCriadoEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<ClienteCriadoEventHandler> _logger;

    public ClienteCriadoEventHandler(IEmailService emailService, ILogger<ClienteCriadoEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(ClienteCriadoEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cliente criado: {ClienteId} - {Nome}", notification.ClienteId, notification.Nome);

        // Enviar email de boas-vindas
        await _emailService.EnviarBoasVindasAsync(notification.Email, notification.Nome);
    }
}
```

### Handler para Pedido Confirmado
```csharp
using BuildingBlock.Mediator;

public class PedidoConfirmadoEventHandler : INotificationHandler<PedidoConfirmadoEvent>
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<PedidoConfirmadoEventHandler> _logger;

    public PedidoConfirmadoEventHandler(
        IProdutoRepository produtoRepository,
        IEmailService emailService,
        ILogger<PedidoConfirmadoEventHandler> logger)
    {
        _produtoRepository = produtoRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(PedidoConfirmadoEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pedido confirmado: {PedidoId} - Valor: {Valor}", 
            notification.PedidoId, notification.ValorTotal);

        // Aqui você poderia reduzir estoque, enviar email, etc.
        await _emailService.EnviarConfirmacaoPedidoAsync(notification.PedidoId, notification.ClienteId);
    }
}
```

## 8. Fluxo Completo: Command → Domain Events → Event Handlers

### Exemplo Prático: Confirmação de Pedido

Este exemplo demonstra o fluxo completo de como um Command dispara Domain Events que são processados por Event Handlers:

```csharp
// 1. COMMAND é enviado via API
[HttpPost("{id}/confirmar")]
public async Task<ActionResult> ConfirmarPedido(Guid id)
{
    var command = new ConfirmarPedidoCommand(id);
    await _mediator.SendAsync(command);
    return Ok(new { message = "Pedido confirmado com sucesso" });
}

// 2. COMMAND HANDLER executa a lógica de negócio
public async Task<Unit> HandleAsync(ConfirmarPedidoCommand request, CancellationToken cancellationToken)
{
    var pedido = await _pedidoRepository.ObterPorIdAsync(request.PedidoId);
    
    // Lógica de negócio que gera eventos
    pedido.Confirmar(); // ← Adiciona PedidoConfirmadoEvent aos DomainEvents
    
    await _pedidoRepository.AtualizarAsync(pedido);
    
    // 3. PUBLICAR EVENTOS - Dispara todos os eventos pendentes
    await pedido.PublishDomainEventsAsync(_mediator, cancellationToken);
    
    return Unit.Value;
}

// 4. EVENT HANDLERS são executados automaticamente
public class PedidoConfirmadoEventHandler : INotificationHandler<PedidoConfirmadoEvent>
{
    public async Task HandleAsync(PedidoConfirmadoEvent notification, CancellationToken cancellationToken)
    {
        // Enviar email de confirmação
        await _emailService.EnviarConfirmacaoPedidoAsync(notification.PedidoId);
        
        // Atualizar relatórios
        await _relatorioService.AtualizarVendasAsync(notification.ValorTotal);
        
        // Integrar com sistema externo
        await _integracaoService.NotificarPedidoConfirmadoAsync(notification);
    }
}
```

### Sequência de Execução

```
1. API Controller recebe requisição
   ↓
2. Mediator.SendAsync(ConfirmarPedidoCommand)
   ↓
3. ConfirmarPedidoCommandHandler.HandleAsync()
   ├── Executa lógica de negócio
   ├── pedido.Confirmar() → Adiciona PedidoConfirmadoEvent
   ├── Salva no repositório
   └── pedido.PublishDomainEventsAsync() → Dispara eventos
       ↓
4. Mediator.PublishAsync(PedidoConfirmadoEvent)
   ↓
5. Todos os INotificationHandler<PedidoConfirmadoEvent> são executados:
   ├── PedidoConfirmadoEventHandler
   ├── EstoqueEventHandler
   ├── EmailEventHandler
   └── RelatorioEventHandler
```

### Vantagens desta Arquitetura

✅ **Desacoplamento**: Command Handlers não precisam conhecer todos os efeitos colaterais  
✅ **Extensibilidade**: Novos Event Handlers podem ser adicionados sem modificar código existente  
✅ **Testabilidade**: Cada handler pode ser testado isoladamente  
✅ **Consistência**: Eventos são publicados apenas após sucesso da operação principal  
✅ **Auditoria**: Todos os eventos ficam registrados para rastreabilidade  

## 9. Exemplo de Uso Completo

### Controller de API
```csharp
using BuildingBlock.Mediator;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CriarCliente([FromBody] CriarClienteCommand command)
    {
        try
        {
            var clienteId = await _mediator.SendAsync(command);
            return CreatedAtAction(nameof(ObterCliente), new { id = clienteId }, clienteId);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDto>> ObterCliente(Guid id)
    {
        try
        {
            var query = new ObterClienteQuery(id);
            var cliente = await _mediator.SendAsync(query);
            
            return cliente == null ? NotFound() : Ok(cliente);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IMediator _mediator;

    public PedidosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CriarPedido([FromBody] CriarPedidoCommand command)
    {
        try
        {
            var pedidoId = await _mediator.SendAsync(command);
            return CreatedAtAction(nameof(ObterPedido), new { id = pedidoId }, pedidoId);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    [HttpPost("{id}/confirmar")]
    public async Task<ActionResult> ConfirmarPedido(Guid id)
    {
        try
        {
            var command = new ConfirmarPedidoCommand(id);
            await _mediator.SendAsync(command);
            return Ok(new { message = "Pedido confirmado com sucesso" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PedidoDto>> ObterPedido(Guid id)
    {
        // Implementação similar...
        return Ok();
    }
}
```

### Configuração de Dependências
```csharp
using BuildingBlock.Mediator;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Registrar Mediator
        services.AddMediator(typeof(CriarClienteCommandHandler).Assembly);

        // Registrar repositórios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();

        // Registrar serviços
        services.AddScoped<IEmailService, EmailService>();

        // Outros serviços...
    }
}
```

## Resumo dos Benefícios

1. **Separação de Responsabilidades**: Commands, Queries e Events têm propósitos bem definidos
2. **Validação Consistente**: Padrão uniforme de validação em todas as camadas
3. **Domain Events**: Comunicação desacoplada entre agregados
4. **Mediator Pattern**: Reduz dependências diretas entre componentes
5. **Tratamento de Erros**: Exceções estruturadas com múltiplos erros
6. **Testabilidade**: Cada handler pode ser testado isoladamente
7. **Escalabilidade**: Fácil adição de novos handlers para events existentes

Este BuildingBlock fornece uma base sólida para aplicações que seguem os princípios de Domain-Driven Design (DDD) e Clean Architecture.