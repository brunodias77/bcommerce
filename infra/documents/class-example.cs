// =====================================================
// SHARED DOMAIN MODELS - Common Base Classes
// =====================================================

using System.Collections.ObjectModel;

namespace Ecommerce.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    public abstract void Validate(IValidationHandler handler);

    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
    public void MarkAsDeleted() => DeletedAt = DateTime.UtcNow;
    public bool IsDeleted => DeletedAt.HasValue;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}

public abstract class AggregateRoot : Entity
{
    private readonly List<DomainEvent> _events = new();
    public IReadOnlyCollection<DomainEvent> Events => new ReadOnlyCollection<DomainEvent>(_events);

    protected AggregateRoot() : base() { }
    protected AggregateRoot(Guid id) : base(id) { }

    public void RaiseEvent(DomainEvent @event) => _events.Add(@event);
    public void AddDomainEvent(DomainEvent @event) => RaiseEvent(@event);
    public void ClearEvents() => _events.Clear();
    public bool HasEvents => _events.Count > 0;
}

public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract bool Equals(ValueObject? other);
    protected abstract int GetCustomHashCode();
    public abstract void Validate(IValidationHandler handler);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((ValueObject)obj);
    }

    public override int GetHashCode() => GetCustomHashCode();

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left is not null && left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}

// =====================================================
// IDENTITY SERVICE DOMAIN
// =====================================================

namespace Ecommerce.Identity.Domain.ValueObjects;

public class PersonName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {LastName}";

    public PersonName(string firstName, string lastName)
    {
        FirstName = firstName?.Trim() ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName?.Trim() ?? throw new ArgumentNullException(nameof(lastName));
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not PersonName name) return false;
        return FirstName.Equals(name.FirstName, StringComparison.OrdinalIgnoreCase) &&
               LastName.Equals(name.LastName, StringComparison.OrdinalIgnoreCase);
    }

    protected override int GetCustomHashCode()
    {
        return HashCode.Combine(
            FirstName.ToLowerInvariant(),
            LastName.ToLowerInvariant()
        );
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            handler.AddError("FirstName", "Nome é obrigatório");
        if (FirstName?.Length > 100)
            handler.AddError("FirstName", "Nome deve ter no máximo 100 caracteres");

        if (string.IsNullOrWhiteSpace(LastName))
            handler.AddError("LastName", "Sobrenome é obrigatório");
        if (LastName?.Length > 100)
            handler.AddError("LastName", "Sobrenome deve ter no máximo 100 caracteres");
    }
}

public class PhoneNumber : ValueObject
{
    public string Number { get; }

    public PhoneNumber(string number)
    {
        Number = number?.Trim() ?? throw new ArgumentNullException(nameof(number));
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not PhoneNumber phone) return false;
        return Number.Equals(phone.Number);
    }

    protected override int GetCustomHashCode() => Number.GetHashCode();

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Number))
            handler.AddError("PhoneNumber", "Telefone é obrigatório");
        if (Number?.Length > 20)
            handler.AddError("PhoneNumber", "Telefone deve ter no máximo 20 caracteres");
        // Adicionar validação de formato se necessário
    }
}

namespace Ecommerce.Identity.Domain.Entities;

public class UserProfile : AggregateRoot
{
    public Guid KeycloakUserId { get; private set; }
    public PersonName Name { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public DateTime? BirthDate { get; private set; }

    private UserProfile() : base() { } // EF Constructor

    public UserProfile(Guid keycloakUserId, PersonName name) : base()
    {
        KeycloakUserId = keycloakUserId;
        Name = name ?? throw new ArgumentNullException(nameof(name));

        RaiseEvent(new UserProfileCreatedEvent(Id, keycloakUserId, name.FullName));
    }

    public void UpdateProfile(PersonName name, PhoneNumber? phone = null, DateTime? birthDate = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Phone = phone;
        BirthDate = birthDate;
        MarkAsUpdated();

        RaiseEvent(new UserProfileUpdatedEvent(Id, KeycloakUserId, name.FullName));
    }

    public override void Validate(IValidationHandler handler)
    {
        Name?.Validate(handler);
        Phone?.Validate(handler);

        if (KeycloakUserId == Guid.Empty)
            handler.AddError("KeycloakUserId", "KeycloakUserId é obrigatório");

        if (BirthDate.HasValue && BirthDate.Value > DateTime.UtcNow.AddYears(-13))
            handler.AddError("BirthDate", "Usuário deve ter pelo menos 13 anos");
    }
}

namespace Ecommerce.Identity.Domain.Events;

public record UserProfileCreatedEvent(Guid ProfileId, Guid KeycloakUserId, string FullName) : DomainEvent;
public record UserProfileUpdatedEvent(Guid ProfileId, Guid KeycloakUserId, string FullName) : DomainEvent;

// =====================================================
// CATALOG SERVICE DOMAIN
// =====================================================

namespace Ecommerce.Catalog.Domain.ValueObjects;

public class Slug : ValueObject
{
    public string Value { get; }

    public Slug(string value)
    {
        Value = GenerateSlug(value ?? throw new ArgumentNullException(nameof(value)));
    }

    private static string GenerateSlug(string input)
    {
        return input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("ã", "a").Replace("õ", "o").Replace("ç", "c")
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Aggregate("", (s, c) => s + c);
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not Slug slug) return false;
        return Value.Equals(slug.Value, StringComparison.OrdinalIgnoreCase);
    }

    protected override int GetCustomHashCode() => Value.GetHashCode();

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Value))
            handler.AddError("Slug", "Slug é obrigatório");
        if (Value?.Length > 200)
            handler.AddError("Slug", "Slug deve ter no máximo 200 caracteres");
    }
}

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public static Money Zero(string currency = "BRL") => new(0, currency);
    public static Money Create(decimal amount, string currency = "BRL") => new(amount, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies");
        return new Money(Amount - other.Amount, Currency);
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not Money money) return false;
        return Amount.Equals(money.Amount) && Currency.Equals(money.Currency);
    }

    protected override int GetCustomHashCode() => HashCode.Combine(Amount, Currency);

    public override void Validate(IValidationHandler handler)
    {
        if (Amount < 0)
            handler.AddError("Amount", "Valor não pode ser negativo");
        if (string.IsNullOrWhiteSpace(Currency))
            handler.AddError("Currency", "Moeda é obrigatória");
    }
}

public class ProductDimensions : ValueObject
{
    public decimal Width { get; }
    public decimal Height { get; }
    public decimal Depth { get; }

    public ProductDimensions(decimal width, decimal height, decimal depth)
    {
        Width = width;
        Height = height;
        Depth = depth;
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not ProductDimensions dims) return false;
        return Width.Equals(dims.Width) && Height.Equals(dims.Height) && Depth.Equals(dims.Depth);
    }

    protected override int GetCustomHashCode() => HashCode.Combine(Width, Height, Depth);

    public override void Validate(IValidationHandler handler)
    {
        if (Width <= 0) handler.AddError("Width", "Largura deve ser positiva");
        if (Height <= 0) handler.AddError("Height", "Altura deve ser positiva");
        if (Depth <= 0) handler.AddError("Depth", "Profundidade deve ser positiva");
    }
}

namespace Ecommerce.Catalog.Domain.Entities;

public class Category : AggregateRoot
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() : base() { } // EF Constructor

    public Category(string name, string? description = null, Guid? parentId = null) : base()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Slug = new Slug(name);
        Description = description;
        ParentId = parentId;
        IsActive = true;

        RaiseEvent(new CategoryCreatedEvent(Id, name, Slug.Value));
    }

    public void UpdateInfo(string name, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Slug = new Slug(name);
        Description = description;
        MarkAsUpdated();

        RaiseEvent(new CategoryUpdatedEvent(Id, name, Slug.Value));
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Name))
            handler.AddError("Name", "Nome da categoria é obrigatório");
        if (Name?.Length > 100)
            handler.AddError("Name", "Nome deve ter no máximo 100 caracteres");

        Slug?.Validate(handler);
    }
}

public class Product : AggregateRoot
{
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public string? Description { get; private set; }
    public string? ShortDescription { get; private set; }
    public string Sku { get; private set; }
    public Money Price { get; private set; }
    public Money? ComparePrice { get; private set; }
    public Guid CategoryId { get; private set; }
    public string? Brand { get; private set; }
    public decimal? Weight { get; private set; }
    public ProductDimensions? Dimensions { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }

    // Navigation
    public Category Category { get; private set; }
    public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();
    public ICollection<ProductAttribute> Attributes { get; private set; } = new List<ProductAttribute>();

    private Product() : base() { } // EF Constructor

    public Product(string name, string sku, Money price, Guid categoryId, string? description = null) : base()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Sku = sku ?? throw new ArgumentNullException(nameof(sku));
        Price = price ?? throw new ArgumentNullException(nameof(price));
        CategoryId = categoryId;
        Description = description;
        Slug = new Slug(name);
        ShortDescription = description?.Length > 500 ? description[..500] : description;
        IsActive = true;
        IsFeatured = false;

        RaiseEvent(new ProductCreatedEvent(Id, name, sku, price.Amount));
    }

    public void UpdateInfo(string name, string? description = null, string? shortDescription = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        ShortDescription = shortDescription ?? (description?.Length > 500 ? description[..500] : description);
        Slug = new Slug(name);
        MarkAsUpdated();

        RaiseEvent(new ProductUpdatedEvent(Id, name, Sku));
    }

    public void UpdatePrice(Money price, Money? comparePrice = null)
    {
        Price = price ?? throw new ArgumentNullException(nameof(price));
        ComparePrice = comparePrice;
        MarkAsUpdated();

        RaiseEvent(new ProductPriceUpdatedEvent(Id, Sku, price.Amount, comparePrice?.Amount));
    }

    public void UpdateCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        MarkAsUpdated();
    }

    public void SetFeatured(bool featured)
    {
        IsFeatured = featured;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void AddImage(string url, string? altText = null, int sortOrder = 0)
    {
        var image = new ProductImage(Id, url, altText, sortOrder);
        Images.Add(image);
        MarkAsUpdated();
    }

    public void AddAttribute(string name, string value)
    {
        var attribute = new ProductAttribute(Id, name, value);
        Attributes.Add(attribute);
        MarkAsUpdated();
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Name))
            handler.AddError("Name", "Nome do produto é obrigatório");
        if (Name?.Length > 200)
            handler.AddError("Name", "Nome deve ter no máximo 200 caracteres");

        if (string.IsNullOrWhiteSpace(Sku))
            handler.AddError("Sku", "SKU é obrigatório");
        if (Sku?.Length > 50)
            handler.AddError("Sku", "SKU deve ter no máximo 50 caracteres");

        Price?.Validate(handler);
        ComparePrice?.Validate(handler);
        Slug?.Validate(handler);
        Dimensions?.Validate(handler);

        if (CategoryId == Guid.Empty)
            handler.AddError("CategoryId", "Categoria é obrigatória");
    }
}

public class ProductImage : Entity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; }
    public string? AltText { get; private set; }
    public int SortOrder { get; private set; }

    // Navigation
    public Product Product { get; private set; }

    private ProductImage() : base() { } // EF Constructor

    public ProductImage(Guid productId, string url, string? altText = null, int sortOrder = 0) : base()
    {
        ProductId = productId;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        AltText = altText;
        SortOrder = sortOrder;
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Url))
            handler.AddError("Url", "URL da imagem é obrigatória");
        if (Url?.Length > 500)
            handler.AddError("Url", "URL deve ter no máximo 500 caracteres");
        if (ProductId == Guid.Empty)
            handler.AddError("ProductId", "ProductId é obrigatório");
    }
}

public class ProductAttribute : Entity
{
    public Guid ProductId { get; private set; }
    public string Name { get; private set; }
    public string Value { get; private set; }

    // Navigation
    public Product Product { get; private set; }

    private ProductAttribute() : base() { } // EF Constructor

    public ProductAttribute(Guid productId, string name, string value) : base()
    {
        ProductId = productId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Name))
            handler.AddError("Name", "Nome do atributo é obrigatório");
        if (Name?.Length > 50)
            handler.AddError("Name", "Nome deve ter no máximo 50 caracteres");

        if (string.IsNullOrWhiteSpace(Value))
            handler.AddError("Value", "Valor do atributo é obrigatório");
        if (Value?.Length > 100)
            handler.AddError("Value", "Valor deve ter no máximo 100 caracteres");

        if (ProductId == Guid.Empty)
            handler.AddError("ProductId", "ProductId é obrigatório");
    }
}

namespace Ecommerce.Catalog.Domain.Events;

public record CategoryCreatedEvent(Guid CategoryId, string Name, string Slug) : DomainEvent;
public record CategoryUpdatedEvent(Guid CategoryId, string Name, string Slug) : DomainEvent;
public record ProductCreatedEvent(Guid ProductId, string Name, string Sku, decimal Price) : DomainEvent;
public record ProductUpdatedEvent(Guid ProductId, string Name, string Sku) : DomainEvent;
public record ProductPriceUpdatedEvent(Guid ProductId, string Sku, decimal Price, decimal? ComparePrice) : DomainEvent;

// =====================================================
// INVENTORY SERVICE DOMAIN
// =====================================================

namespace Ecommerce.Inventory.Domain.ValueObjects;

public class StockQuantity : ValueObject
{
    public int Available { get; }
    public int Reserved { get; }
    public int Total => Available + Reserved;

    public StockQuantity(int available, int reserved = 0)
    {
        if (available < 0) throw new ArgumentException("Available quantity cannot be negative", nameof(available));
        if (reserved < 0) throw new ArgumentException("Reserved quantity cannot be negative", nameof(reserved));

        Available = available;
        Reserved = reserved;
    }

    public StockQuantity Reserve(int quantity)
    {
        if (quantity > Available)
            throw new InvalidOperationException("Insufficient stock available");

        return new StockQuantity(Available - quantity, Reserved + quantity);
    }

    public StockQuantity Release(int quantity)
    {
        if (quantity > Reserved)
            throw new InvalidOperationException("Cannot release more than reserved");

        return new StockQuantity(Available + quantity, Reserved - quantity);
    }

    public StockQuantity Consume(int quantity)
    {
        if (quantity > Reserved)
            throw new InvalidOperationException("Cannot consume more than reserved");

        return new StockQuantity(Available, Reserved - quantity);
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not StockQuantity stock) return false;
        return Available.Equals(stock.Available) && Reserved.Equals(stock.Reserved);
    }

    protected override int GetCustomHashCode() => HashCode.Combine(Available, Reserved);

    public override void Validate(IValidationHandler handler)
    {
        if (Available < 0)
            handler.AddError("Available", "Quantidade disponível não pode ser negativa");
        if (Reserved < 0)
            handler.AddError("Reserved", "Quantidade reservada não pode ser negativa");
    }
}

namespace Ecommerce.Inventory.Domain.Entities;

public class Stock : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public StockQuantity Quantity { get; private set; }
    public int MinQuantity { get; private set; }
    public int? MaxQuantity { get; private set; }

    public ICollection<StockMovement> Movements { get; private set; } = new List<StockMovement>();

    private Stock() : base() { } // EF Constructor

    public Stock(Guid productId, int initialQuantity = 0, int minQuantity = 0, int? maxQuantity = null) : base()
    {
        ProductId = productId;
        Quantity = new StockQuantity(initialQuantity);
        MinQuantity = minQuantity;
        MaxQuantity = maxQuantity;

        if (initialQuantity > 0)
        {
            AddMovement(StockMovementType.In, initialQuantity, null, "Initial stock");
        }

        RaiseEvent(new StockCreatedEvent(ProductId, initialQuantity));
    }

    public void AdjustStock(int newQuantity, Guid? referenceId = null, string? notes = null)
    {
        var difference = newQuantity - Quantity.Available;
        var movementType = difference > 0 ? StockMovementType.In : StockMovementType.Out;

        Quantity = new StockQuantity(newQuantity, Quantity.Reserved);
        AddMovement(movementType, Math.Abs(difference), referenceId, notes);
        MarkAsUpdated();

        RaiseEvent(new StockAdjustedEvent(ProductId, newQuantity, difference));
    }

    public void ReserveStock(int quantity, Guid orderId)
    {
        if (quantity > Quantity.Available)
            throw new InsufficientStockException($"Cannot reserve {quantity} items. Only {Quantity.Available} available.");

        Quantity = Quantity.Reserve(quantity);
        AddMovement(StockMovementType.Reserved, quantity, orderId, $"Reserved for order {orderId}");
        MarkAsUpdated();

        RaiseEvent(new StockReservedEvent(ProductId, quantity, orderId));
    }

    public void ReleaseStock(int quantity, Guid orderId)
    {
        Quantity = Quantity.Release(quantity);
        AddMovement(StockMovementType.Released, quantity, orderId, $"Released from order {orderId}");
        MarkAsUpdated();

        RaiseEvent(new StockReleasedEvent(ProductId, quantity, orderId));
    }

    public void ConsumeStock(int quantity, Guid orderId)
    {
        Quantity = Quantity.Consume(quantity);
        AddMovement(StockMovementType.Out, quantity, orderId, $"Consumed for order {orderId}");
        MarkAsUpdated();

        RaiseEvent(new StockConsumedEvent(ProductId, quantity, orderId));
    }

    private void AddMovement(StockMovementType type, int quantity, Guid? referenceId, string? notes)
    {
        var movement = new StockMovement(ProductId, type, quantity, referenceId, "ORDER", notes);
        Movements.Add(movement);
    }

    public bool IsLowStock => Quantity.Available <= MinQuantity;
    public bool IsOutOfStock => Quantity.Available == 0;

    public override void Validate(IValidationHandler handler)
    {
        if (ProductId == Guid.Empty)
            handler.AddError("ProductId", "ProductId é obrigatório");

        Quantity?.Validate(handler);

        if (MinQuantity < 0)
            handler.AddError("MinQuantity", "Quantidade mínima não pode ser negativa");

        if (MaxQuantity.HasValue && MaxQuantity.Value < MinQuantity)
            handler.AddError("MaxQuantity", "Quantidade máxima deve ser maior que a mínima");
    }
}

public class StockMovement : Entity
{
    public Guid ProductId { get; private set; }
    public StockMovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    public Stock Stock { get; private set; }

    private StockMovement() : base() { } // EF Constructor

    public StockMovement(Guid productId, StockMovementType movementType, int quantity,
                        Guid? referenceId = null, string? referenceType = null, string? notes = null) : base()
    {
        ProductId = productId;
        MovementType = movementType;
        Quantity = quantity;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
        Notes = notes;
    }

    public override void Validate(IValidationHandler handler)
    {
        if (ProductId == Guid.Empty)
            handler.AddError("ProductId", "ProductId é obrigatório");
        if (Quantity <= 0)
            handler.AddError("Quantity", "Quantidade deve ser positiva");
    }
}

public enum StockMovementType
{
    In,
    Out,
    Reserved,
    Released
}

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string message) : base(message) { }
}

namespace Ecommerce.Inventory.Domain.Events;

public record StockCreatedEvent(Guid ProductId, int InitialQuantity) : DomainEvent;
public record StockAdjustedEvent(Guid ProductId, int NewQuantity, int Difference) : DomainEvent;
public record StockReservedEvent(Guid ProductId, int Quantity, Guid OrderId) : DomainEvent;
public record StockReleasedEvent(Guid ProductId, int Quantity, Guid OrderId) : DomainEvent;
public record StockConsumedEvent(Guid ProductId, int Quantity, Guid OrderId) : DomainEvent;

// =====================================================
// ORDER SERVICE DOMAIN
// =====================================================

namespace Ecommerce.Orders.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(string street, string number, string neighborhood, string city,
                  string state, string zipCode, string country = "Brasil", string? complement = null)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        Number = number ?? throw new ArgumentNullException(nameof(number));
        Neighborhood = neighborhood ?? throw new ArgumentNullException(nameof(neighborhood));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        Country = country ?? throw new ArgumentNullException(nameof(country));
        Complement = complement;
    }

    public string FullAddress => $"{Street}, {Number}{(!string.IsNullOrEmpty(Complement) ? $", {Complement}" : "")}, {Neighborhood}, {City} - {State}, {ZipCode}, {Country}";

    public override bool Equals(ValueObject? other)
    {
        if (other is not Address address) return false;
        return Street.Equals(address.Street) &&
               Number.Equals(address.Number) &&
               Neighborhood.Equals(address.Neighborhood) &&
               City.Equals(address.City) &&
               State.Equals(address.State) &&
               ZipCode.Equals(address.ZipCode) &&
               Country.Equals(address.Country) &&
               (Complement?.Equals(address.Complement) ?? address.Complement is null);
    }

    protected override int GetCustomHashCode()
    {
        return HashCode.Combine(Street, Number, Neighborhood, City, State, ZipCode, Country, Complement);
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Street))
            handler.AddError("Street", "Rua é obrigatória");
        if (Street?.Length > 200)
            handler.AddError("Street", "Rua deve ter no máximo 200 caracteres");

        if (string.IsNullOrWhiteSpace(Number))
            handler.AddError("Number", "Número é obrigatório");
        if (Number?.Length > 20)
            handler.AddError("Number", "Número deve ter no máximo 20 caracteres");

        if (string.IsNullOrWhiteSpace(Neighborhood))
            handler.AddError("Neighborhood", "Bairro é obrigatório");
        if (Neighborhood?.Length > 100)
            handler.AddError("Neighborhood", "Bairro deve ter no máximo 100 caracteres");

        if (string.IsNullOrWhiteSpace(City))
            handler.AddError("City", "Cidade é obrigatória");
        if (City?.Length > 100)
            handler.AddError("City", "Cidade deve ter no máximo 100 caracteres");

        if (string.IsNullOrWhiteSpace(State))
            handler.AddError("State", "Estado é obrigatório");
        if (State?.Length > 50)
            handler.AddError("State", "Estado deve ter no máximo 50 caracteres");

        if (string.IsNullOrWhiteSpace(ZipCode))
            handler.AddError("ZipCode", "CEP é obrigatório");
        if (ZipCode?.Length > 10)
            handler.AddError("ZipCode", "CEP deve ter no máximo 10 caracteres");
    }
}

public class OrderNumber : ValueObject
{
    public string Value { get; }

    public OrderNumber(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static OrderNumber Generate()
    {
        var year = DateTime.UtcNow.Year;
        var random = new Random().Next(10000000, 99999999);
        return new OrderNumber($"{year}{random:D8}");
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not OrderNumber orderNumber) return false;
        return Value.Equals(orderNumber.Value);
    }

    protected override int GetCustomHashCode() => Value.GetHashCode();

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Value))
            handler.AddError("OrderNumber", "Número do pedido é obrigatório");
        if (Value?.Length > 20)
            handler.AddError("OrderNumber", "Número do pedido deve ter no máximo 20 caracteres");
    }
}

namespace Ecommerce.Orders.Domain.Entities;

public class CustomerAddress : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Address Address { get; private set; }
    public bool IsDefault { get; private set; }

    private CustomerAddress() : base() { } // EF Constructor

    public CustomerAddress(Guid userId, Address address, bool isDefault = false) : base()
    {
        UserId = userId;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        IsDefault = isDefault;

        RaiseEvent(new CustomerAddressCreatedEvent(Id, userId, address.FullAddress));
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        MarkAsUpdated();

        RaiseEvent(new CustomerAddressUpdatedEvent(Id, UserId, address.FullAddress));
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        MarkAsUpdated();
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
        MarkAsUpdated();
    }

    public override void Validate(IValidationHandler handler)
    {
        if (UserId == Guid.Empty)
            handler.AddError("UserId", "UserId é obrigatório");

        Address?.Validate(handler);
    }
}

public class Order : AggregateRoot
{
    public OrderNumber OrderNumber { get; private set; }
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money Subtotal { get; private set; }
    public Money ShippingCost { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TotalAmount { get; private set; }
    public Guid? ShippingAddressId { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? Notes { get; private set; }

    // Navigation
    public CustomerAddress? ShippingAddress { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; private set; } = new List<OrderStatusHistory>();

    private Order() : base() { } // EF Constructor

    public Order(Guid userId, List<OrderItem> items, Guid? shippingAddressId = null) : base()
    {
        UserId = userId;
        OrderNumber = OrderNumber.Generate();
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Pending;
        ShippingAddressId = shippingAddressId;

        AddItems(items);
        CalculateTotals();
        AddStatusHistory(OrderStatus.Pending, "Pedido criado");

        RaiseEvent(new OrderCreatedEvent(Id, OrderNumber.Value, UserId, TotalAmount.Amount));
    }

    private void AddItems(List<OrderItem> items)
    {
        if (!items?.Any() == true)
            throw new ArgumentException("Order must have at least one item", nameof(items));

        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(new OrderItem(Id, item.ProductId, item.ProductName, item.ProductSku,
                                   item.Quantity, item.UnitPrice));
        }
    }

    private void CalculateTotals()
    {
        Subtotal = Items.Aggregate(Money.Zero(), (acc, item) => acc.Add(item.TotalPrice));
        ShippingCost = Money.Zero(); // TODO: Implementar cálculo de frete
        TaxAmount = Money.Zero(); // TODO: Implementar cálculo de impostos
        DiscountAmount = Money.Zero(); // TODO: Implementar desconto

        TotalAmount = Subtotal.Add(ShippingCost).Add(TaxAmount).Subtract(DiscountAmount);
    }

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        Status = OrderStatus.Confirmed;
        AddStatusHistory(OrderStatus.Confirmed, "Pedido confirmado");
        MarkAsUpdated();

        RaiseEvent(new OrderConfirmedEvent(Id, OrderNumber.Value, UserId));
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be processed");

        Status = OrderStatus.Processing;
        AddStatusHistory(OrderStatus.Processing, "Pedido em processamento");
        MarkAsUpdated();

        RaiseEvent(new OrderProcessingStartedEvent(Id, OrderNumber.Value));
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Only processing orders can be shipped");

        Status = OrderStatus.Shipped;
        AddStatusHistory(OrderStatus.Shipped, "Pedido enviado");
        MarkAsUpdated();

        RaiseEvent(new OrderShippedEvent(Id, OrderNumber.Value, UserId));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered");

        Status = OrderStatus.Delivered;
        AddStatusHistory(OrderStatus.Delivered, "Pedido entregue");
        MarkAsUpdated();

        RaiseEvent(new OrderDeliveredEvent(Id, OrderNumber.Value, UserId));
    }

    public void Cancel(string? reason = null)
    {
        if (Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel delivered or already cancelled orders");

        Status = OrderStatus.Cancelled;
        AddStatusHistory(OrderStatus.Cancelled, reason ?? "Pedido cancelado");
        MarkAsUpdated();

        RaiseEvent(new OrderCancelledEvent(Id, OrderNumber.Value, UserId, reason));
    }

    public void UpdatePaymentStatus(PaymentStatus paymentStatus, string? paymentMethod = null)
    {
        PaymentStatus = paymentStatus;
        if (!string.IsNullOrWhiteSpace(paymentMethod))
            PaymentMethod = paymentMethod;
        MarkAsUpdated();

        RaiseEvent(new OrderPaymentUpdatedEvent(Id, OrderNumber.Value, paymentStatus, paymentMethod));
    }

    private void AddStatusHistory(OrderStatus status, string? notes = null)
    {
        var history = new OrderStatusHistory(Id, status, notes);
        StatusHistory.Add(history);
    }

    public bool CanBeCancelled => Status is OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Processing;
    public bool IsCompleted => Status == OrderStatus.Delivered;
    public bool IsCancelled => Status == OrderStatus.Cancelled;

    public override void Validate(IValidationHandler handler)
    {
        if (UserId == Guid.Empty)
            handler.AddError("UserId", "UserId é obrigatório");

        OrderNumber?.Validate(handler);
        Subtotal?.Validate(handler);
        TotalAmount?.Validate(handler);

        if (!Items?.Any() == true)
            handler.AddError("Items", "Pedido deve ter pelo menos um item");

        foreach (var item in Items ?? Enumerable.Empty<OrderItem>())
            item.Validate(handler);
    }
}

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductSku { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice { get; private set; }

    // Navigation
    public Order Order { get; private set; }

    private OrderItem() : base() { } // EF Constructor

    public OrderItem(Guid orderId, Guid productId, string productName, string productSku,
                    int quantity, Money unitPrice) : base()
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        ProductSku = productSku ?? throw new ArgumentNullException(nameof(productSku));
        Quantity = quantity;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        TotalPrice = new Money(unitPrice.Amount * quantity, unitPrice.Currency);
    }

    public override void Validate(IValidationHandler handler)
    {
        if (OrderId == Guid.Empty)
            handler.AddError("OrderId", "OrderId é obrigatório");
        if (ProductId == Guid.Empty)
            handler.AddError("ProductId", "ProductId é obrigatório");

        if (string.IsNullOrWhiteSpace(ProductName))
            handler.AddError("ProductName", "Nome do produto é obrigatório");
        if (string.IsNullOrWhiteSpace(ProductSku))
            handler.AddError("ProductSku", "SKU do produto é obrigatório");

        if (Quantity <= 0)
            handler.AddError("Quantity", "Quantidade deve ser positiva");

        UnitPrice?.Validate(handler);
        TotalPrice?.Validate(handler);
    }
}

public class OrderStatusHistory : Entity
{
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ChangedBy { get; private set; }

    // Navigation
    public Order Order { get; private set; }

    private OrderStatusHistory() : base() { } // EF Constructor

    public OrderStatusHistory(Guid orderId, OrderStatus status, string? notes = null, Guid? changedBy = null) : base()
    {
        OrderId = orderId;
        Status = status;
        Notes = notes;
        ChangedBy = changedBy;
    }

    public override void Validate(IValidationHandler handler)
    {
        if (OrderId == Guid.Empty)
            handler.AddError("OrderId", "OrderId é obrigatório");
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}

namespace Ecommerce.Orders.Domain.Events;

public record CustomerAddressCreatedEvent(Guid AddressId, Guid UserId, string FullAddress) : DomainEvent;
public record CustomerAddressUpdatedEvent(Guid AddressId, Guid UserId, string FullAddress) : DomainEvent;
public record OrderCreatedEvent(Guid OrderId, string OrderNumber, Guid UserId, decimal TotalAmount) : DomainEvent;
public record OrderConfirmedEvent(Guid OrderId, string OrderNumber, Guid UserId) : DomainEvent;
public record OrderProcessingStartedEvent(Guid OrderId, string OrderNumber) : DomainEvent;
public record OrderShippedEvent(Guid OrderId, string OrderNumber, Guid UserId) : DomainEvent;
public record OrderDeliveredEvent(Guid OrderId, string OrderNumber, Guid UserId) : DomainEvent;
public record OrderCancelledEvent(Guid OrderId, string OrderNumber, Guid UserId, string? Reason) : DomainEvent;
public record OrderPaymentUpdatedEvent(Guid OrderId, string OrderNumber, PaymentStatus PaymentStatus, string? PaymentMethod) : DomainEvent;

// =====================================================
// PAYMENT SERVICE DOMAIN
// =====================================================

namespace Ecommerce.Payments.Domain.ValueObjects;

public class PaymentMethod : ValueObject
{
    public string Type { get; }
    public Dictionary<string, object>? Details { get; }

    public PaymentMethod(string type, Dictionary<string, object>? details = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Details = details ?? new Dictionary<string, object>();
    }

    public static PaymentMethod CreditCard(string brand, string lastFour) =>
        new("CREDIT_CARD", new Dictionary<string, object>
        {
            ["brand"] = brand,
            ["lastFour"] = lastFour
        });

    public static PaymentMethod DebitCard(string brand, string lastFour) =>
        new("DEBIT_CARD", new Dictionary<string, object>
        {
            ["brand"] = brand,
            ["lastFour"] = lastFour
        });

    public static PaymentMethod Pix() => new("PIX");
    public static PaymentMethod Boleto() => new("BOLETO");

    public override bool Equals(ValueObject? other)
    {
        if (other is not PaymentMethod method) return false;
        return Type.Equals(method.Type);
    }

    protected override int GetCustomHashCode() => Type.GetHashCode();

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Type))
            handler.AddError("Type", "Tipo de pagamento é obrigatório");

        var validTypes = new[] { "CREDIT_CARD", "DEBIT_CARD", "PIX", "BOLETO" };
        if (!validTypes.Contains(Type))
            handler.AddError("Type", "Tipo de pagamento inválido");
    }
}

namespace Ecommerce.Payments.Domain.Entities;

public class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string Provider { get; private set; }
    public string? ProviderPaymentId { get; private set; }
    public Dictionary<string, object>? ProviderResponse { get; private set; }
    public string? FailureReason { get; private set; }

    public ICollection<PaymentWebhook> Webhooks { get; private set; } = new List<PaymentWebhook>();

    private Payment() : base() { } // EF Constructor

    public Payment(Guid orderId, Money amount, PaymentMethod paymentMethod, string provider) : base()
    {
        OrderId = orderId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        PaymentMethod = paymentMethod ?? throw new ArgumentNullException(nameof(paymentMethod));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Status = PaymentStatus.Pending;

        RaiseEvent(new PaymentCreatedEvent(Id, orderId, amount.Amount, paymentMethod.Type, provider));
    }

    public void StartProcessing(string providerPaymentId)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can start processing");

        Status = PaymentStatus.Processing;
        ProviderPaymentId = providerPaymentId;
        MarkAsUpdated();

        RaiseEvent(new PaymentProcessingStartedEvent(Id, OrderId, providerPaymentId));
    }

    public void MarkAsSuccessful(Dictionary<string, object>? providerResponse = null)
    {
        if (Status is PaymentStatus.Success or PaymentStatus.Refunded)
            throw new InvalidOperationException("Payment is already completed");

        Status = PaymentStatus.Success;
        ProviderResponse = providerResponse;
        FailureReason = null;
        MarkAsUpdated();

        RaiseEvent(new PaymentSucceededEvent(Id, OrderId, Amount.Amount));
    }

    public void MarkAsFailed(string reason, Dictionary<string, object>? providerResponse = null)
    {
        if (Status is PaymentStatus.Success or PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot fail a completed payment");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProviderResponse = providerResponse;
        MarkAsUpdated();

        RaiseEvent(new PaymentFailedEvent(Id, OrderId, reason));
    }

    public void Cancel()
    {
        if (Status is PaymentStatus.Success or PaymentStatus.Refunded)
            throw new InvalidOperationException("Cannot cancel a completed payment");

        Status = PaymentStatus.Cancelled;
        MarkAsUpdated();

        RaiseEvent(new PaymentCancelledEvent(Id, OrderId));
    }

    public void Refund(Money refundAmount, string? reason = null)
    {
        if (Status != PaymentStatus.Success)
            throw new InvalidOperationException("Only successful payments can be refunded");
        if (refundAmount.Amount > Amount.Amount)
            throw new InvalidOperationException("Refund amount cannot exceed payment amount");

        Status = PaymentStatus.Refunded;
        MarkAsUpdated();

        RaiseEvent(new PaymentRefundedEvent(Id, OrderId, refundAmount.Amount, reason));
    }

    public void AddWebhook(string eventType, Dictionary<string, object> eventData)
    {
        var webhook = new PaymentWebhook(Id, Provider, eventType, eventData);
        Webhooks.Add(webhook);
        MarkAsUpdated();
    }

    public bool CanBeRefunded => Status == PaymentStatus.Success;
    public bool IsCompleted => Status is PaymentStatus.Success or PaymentStatus.Failed or PaymentStatus.Cancelled or PaymentStatus.Refunded;

    public override void Validate(IValidationHandler handler)
    {
        if (OrderId == Guid.Empty)
            handler.AddError("OrderId", "OrderId é obrigatório");

        Amount?.Validate(handler);
        PaymentMethod?.Validate(handler);

        if (string.IsNullOrWhiteSpace(Provider))
            handler.AddError("Provider", "Provider é obrigatório");

        var validProviders = new[] { "STRIPE", "MERCADOPAGO", "PAGSEGURO" };
        if (!validProviders.Contains(Provider))
            handler.AddError("Provider", "Provider inválido");
    }
}

public class PaymentWebhook : Entity
{
    public Guid? PaymentId { get; private set; }
    public string Provider { get; private set; }
    public string EventType { get; private set; }
    public Dictionary<string, object> EventData { get; private set; }
    public bool Processed { get; private set; }

    // Navigation
    public Payment? Payment { get; private set; }

    private PaymentWebhook() : base() { } // EF Constructor

    public PaymentWebhook(Guid? paymentId, string provider, string eventType, Dictionary<string, object> eventData) : base()
    {
        PaymentId = paymentId;
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));
        Processed = false;
    }

    public void MarkAsProcessed()
    {
        Processed = true;
        MarkAsUpdated();
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Provider))
            handler.AddError("Provider", "Provider é obrigatório");
        if (string.IsNullOrWhiteSpace(EventType))
            handler.AddError("EventType", "EventType é obrigatório");
        if (EventData == null)
            handler.AddError("EventData", "EventData é obrigatório");
    }
}

namespace Ecommerce.Payments.Domain.Events;

public record PaymentCreatedEvent(Guid PaymentId, Guid OrderId, decimal Amount, string PaymentMethod, string Provider) : DomainEvent;
public record PaymentProcessingStartedEvent(Guid PaymentId, Guid OrderId, string ProviderPaymentId) : DomainEvent;
public record PaymentSucceededEvent(Guid PaymentId, Guid OrderId, decimal Amount) : DomainEvent;
public record PaymentFailedEvent(Guid PaymentId, Guid OrderId, string Reason) : DomainEvent;
public record PaymentCancelledEvent(Guid PaymentId, Guid OrderId) : DomainEvent;
public record PaymentRefundedEvent(Guid PaymentId, Guid OrderId, decimal RefundAmount, string? Reason) : DomainEvent;

// =====================================================
// NOTIFICATION SERVICE DOMAIN
// =====================================================

namespace Ecommerce.Notifications.Domain.ValueObjects;

public class NotificationRecipient : ValueObject
{
    public string Value { get; }
    public NotificationType Type { get; }

    public NotificationRecipient(string value, NotificationType type)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Type = type;
    }

    public static NotificationRecipient Email(string email) => new(email, NotificationType.Email);
    public static NotificationRecipient Sms(string phone) => new(phone, NotificationType.Sms);
    public static NotificationRecipient Push(string deviceToken) => new(deviceToken, NotificationType.Push);

    public override bool Equals(ValueObject? other)
    {
        if (other is not NotificationRecipient recipient) return false;
        return Value.Equals(recipient.Value) && Type.Equals(recipient.Type);
    }

    protected override int GetCustomHashCode() => HashCode.Combine(Value, Type);

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Value))
            handler.AddError("Value", "Valor do destinatário é obrigatório");

        switch (Type)
        {
            case NotificationType.Email when !Value.Contains('@'):
                handler.AddError("Value", "Email inválido");
                break;
            case NotificationType.Sms when Value.Length < 10:
                handler.AddError("Value", "Telefone inválido");
                break;
        }
    }
}

namespace Ecommerce.Notifications.Domain.Entities;

public class NotificationTemplate : AggregateRoot
{
    public string Name { get; private set; }
    public NotificationType Type { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; }
    public Dictionary<string, object>? Variables { get; private set; }
    public bool IsActive { get; private set; }

    private NotificationTemplate() : base() { } // EF Constructor

    public NotificationTemplate(string name, NotificationType type, string body, string? subject = null) : base()
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Subject = subject;
        IsActive = true;
        Variables = new Dictionary<string, object>();

        RaiseEvent(new NotificationTemplateCreatedEvent(Id, name, type.ToString()));
    }

    public void UpdateTemplate(string body, string? subject = null, Dictionary<string, object>? variables = null)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Subject = subject;
        Variables = variables ?? new Dictionary<string, object>();
        MarkAsUpdated();

        RaiseEvent(new NotificationTemplateUpdatedEvent(Id, Name));
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public string RenderBody(Dictionary<string, object> data)
    {
        var result = Body;
        foreach (var kvp in data)
        {
            result = result.Replace($"#{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
        }
        return result;
    }

    public string? RenderSubject(Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(Subject)) return null;

        var result = Subject;
        foreach (var kvp in data)
        {
            result = result.Replace($"#{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
        }
        return result;
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(Name))
            handler.AddError("Name", "Nome do template é obrigatório");
        if (Name?.Length > 100)
            handler.AddError("Name", "Nome deve ter no máximo 100 caracteres");

        if (string.IsNullOrWhiteSpace(Body))
            handler.AddError("Body", "Corpo do template é obrigatório");

        if (Type == NotificationType.Email && string.IsNullOrWhiteSpace(Subject))
            handler.AddError("Subject", "Assunto é obrigatório para emails");
    }
}

public class Notification : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid? TemplateId { get; private set; }
    public NotificationRecipient Recipient { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? Provider { get; private set; }
    public Dictionary<string, object>? ProviderResponse { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    // Navigation
    public NotificationTemplate? Template { get; private set; }

    private Notification() : base() { } // EF Constructor

    public Notification(Guid userId, NotificationRecipient recipient, string body, string? subject = null,
                       Guid? templateId = null, DateTime? scheduledAt = null) : base()
    {
        UserId = userId;
        Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Subject = subject;
        TemplateId = templateId;
        ScheduledAt = scheduledAt;
        Status = NotificationStatus.Pending;

        RaiseEvent(new NotificationCreatedEvent(Id, userId, recipient.Type.ToString(), recipient.Value));
    }

    public void Schedule(DateTime scheduledAt)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Only pending notifications can be scheduled");

        ScheduledAt = scheduledAt;
        MarkAsUpdated();
    }

    public void MarkAsSent(string provider, Dictionary<string, object>? providerResponse = null)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Only pending notifications can be marked as sent");

        Status = NotificationStatus.Sent;
        Provider = provider;
        ProviderResponse = providerResponse;
        SentAt = DateTime.UtcNow;
        MarkAsUpdated();

        RaiseEvent(new NotificationSentEvent(Id, UserId, provider));
    }

    public void MarkAsDelivered(Dictionary<string, object>? providerResponse = null)
    {
        if (Status != NotificationStatus.Sent)
            throw new InvalidOperationException("Only sent notifications can be marked as delivered");

        Status = NotificationStatus.Delivered;
        ProviderResponse = providerResponse;
        MarkAsUpdated();

        RaiseEvent(new NotificationDeliveredEvent(Id, UserId));
    }

    public void MarkAsFailed(string reason, Dictionary<string, object>? providerResponse = null)
    {
        Status = NotificationStatus.Failed;
        ProviderResponse = providerResponse ?? new Dictionary<string, object> { ["error"] = reason };
        MarkAsUpdated();

        RaiseEvent(new NotificationFailedEvent(Id, UserId, reason));
    }

    public bool CanBeSent => Status == NotificationStatus.Pending &&
                            (!ScheduledAt.HasValue || ScheduledAt.Value <= DateTime.UtcNow);

    public override void Validate(IValidationHandler handler)
    {
        if (UserId == Guid.Empty)
            handler.AddError("UserId", "UserId é obrigatório");

        Recipient?.Validate(handler);

        if (string.IsNullOrWhiteSpace(Body))
            handler.AddError("Body", "Corpo da notificação é obrigatório");

        if (Recipient?.Type == NotificationType.Email && string.IsNullOrWhiteSpace(Subject))
            handler.AddError("Subject", "Assunto é obrigatório para emails");
    }
}

public enum NotificationType
{
    Email,
    Sms,
    Push
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Failed
}

namespace Ecommerce.Notifications.Domain.Events;

public record NotificationTemplateCreatedEvent(Guid TemplateId, string Name, string Type) : DomainEvent;
public record NotificationTemplateUpdatedEvent(Guid TemplateId, string Name) : DomainEvent;
public record NotificationCreatedEvent(Guid NotificationId, Guid UserId, string Type, string Recipient) : DomainEvent;
public record NotificationSentEvent(Guid NotificationId, Guid UserId, string Provider) : DomainEvent;
public record NotificationDeliveredEvent(Guid NotificationId, Guid UserId) : DomainEvent;
public record NotificationFailedEvent(Guid NotificationId, Guid UserId, string Reason) : DomainEvent;

// =====================================================
// SHARED INTERFACES & VALIDATIONS
// =====================================================

namespace Ecommerce.Domain.Common;

public interface IValidationHandler
{
    void AddError(string property, string message);
    bool HasErrors { get; }
    IEnumerable<ValidationError> Errors { get; }
}

public class ValidationError
{
    public string Property { get; }
    public string Message { get; }

    public ValidationError(string property, string message)
    {
        Property = property;
        Message = message;
    }
}

public class ValidationHandler : IValidationHandler
{
    private readonly List<ValidationError> _errors = new();

    public bool HasErrors => _errors.Any();
    public IEnumerable<ValidationError> Errors => _errors.AsReadOnly();

    public void AddError(string property, string message)
    {
        _errors.Add(new ValidationError(property, message));
    }

    public void ThrowIfHasErrors()
    {
        if (HasErrors)
        {
            var messages = _errors.Select(e => $"{e.Property}: {e.Message}");
            throw new DomainValidationException(string.Join("; ", messages));
        }
    }
}

public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
}

// =====================================================
// REPOSITORY INTERFACES
// =====================================================

namespace Ecommerce.Identity.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByKeycloakUserIdAsync(Guid keycloakUserId, CancellationToken cancellationToken = default);
    Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

namespace Ecommerce.Catalog.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetByParentIdAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Product> Products, int TotalCount)> SearchAsync(string? searchTerm, Guid? categoryId,
        decimal? minPrice, decimal? maxPrice, int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

namespace Ecommerce.Inventory.Domain.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<Stock>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Stock stock, CancellationToken cancellationToken = default);
    Task UpdateAsync(Stock stock, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid productId, CancellationToken cancellationToken = default);
}

namespace Ecommerce.Orders.Domain.Repositories;

public interface ICustomerAddressRepository
{
    Task<CustomerAddress?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerAddress?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(CustomerAddress address, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerAddress address, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(Guid? userId, OrderStatus? status,
        int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

namespace Ecommerce.Payments.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByProviderPaymentIdAsync(string providerPaymentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}

namespace Ecommerce.Notifications.Domain.Repositories;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationTemplate>> GetActiveByTypeAsync(NotificationType type, CancellationToken cancellationToken = default);
    Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Notification>> GetScheduledAsync(DateTime maxScheduledAt, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);
}

// =====================================================
// DOMAIN SERVICES
// =====================================================

namespace Ecommerce.Catalog.Domain.Services;

public interface IPricingService
{
    Money CalculateDiscountedPrice(Money originalPrice, decimal discountPercentage);
    bool IsOnSale(Money price, Money? comparePrice);
    Money CalculateTax(Money price, decimal taxRate);
}

public class PricingService : IPricingService
{
    public Money CalculateDiscountedPrice(Money originalPrice, decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

        var discountAmount = originalPrice.Amount * (discountPercentage / 100);
        return new Money(originalPrice.Amount - discountAmount, originalPrice.Currency);
    }

    public bool IsOnSale(Money price, Money? comparePrice)
    {
        return comparePrice != null && comparePrice.Amount > price.Amount;
    }

    public Money CalculateTax(Money price, decimal taxRate)
    {
        var taxAmount = price.Amount * (taxRate / 100);
        return new Money(taxAmount, price.Currency);
    }
}

namespace Ecommerce.Orders.Domain.Services;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}

public interface IShippingCalculatorService
{
    Task<Money> CalculateShippingCostAsync(Address destination, IEnumerable<OrderItem> items,
        CancellationToken cancellationToken = default);
}

namespace Ecommerce.Inventory.Domain.Services;

public interface IStockValidationService
{
    Task<bool> IsStockAvailableAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> ValidateStockForMultipleProductsAsync(Dictionary<Guid, int> productQuantities,
        CancellationToken cancellationToken = default);
}

// =====================================================
// EXAMPLE USAGE & FACTORY METHODS
// =====================================================

namespace Ecommerce.Domain.Factories;

public static class DomainFactory
{
    // Identity
    public static UserProfile CreateUserProfile(Guid keycloakUserId, string firstName, string lastName,
        string? phone = null, DateTime? birthDate = null)
    {
        var name = new PersonName(firstName, lastName);
        var profile = new UserProfile(keycloakUserId, name);

        if (!string.IsNullOrEmpty(phone) || birthDate.HasValue)
        {
            var phoneNumber = !string.IsNullOrEmpty(phone) ? new PhoneNumber(phone) : null;
            profile.UpdateProfile(name, phoneNumber, birthDate);
        }

        return profile;
    }

    // Catalog
    public static Product CreateProduct(string name, string sku, decimal price, Guid categoryId,
        string? description = null, string? brand = null)
    {
        var money = new Money(price);
        return new Product(name, sku, money, categoryId, description);
    }

    public static Category CreateCategory(string name, string? description = null, Guid? parentId = null)
    {
        return new Category(name, description, parentId);
    }

    // Orders
    public static Order CreateOrder(Guid userId, List<(Guid productId, string name, string sku, int quantity, decimal price)> items,
        Guid? shippingAddressId = null)
    {
        var orderItems = items.Select(item =>
            new OrderItem(Guid.Empty, item.productId, item.name, item.sku, item.quantity, new Money(item.price))
        ).ToList();

        return new Order(userId, orderItems, shippingAddressId);
    }

    public static CustomerAddress CreateAddress(Guid userId, string street, string number, string neighborhood,
        string city, string state, string zipCode, string? complement = null, bool isDefault = false)
    {
        var address = new Address(street, number, neighborhood, city, state, zipCode, "Brasil", complement);
        return new CustomerAddress(userId, address, isDefault);
    }

    // Payments
    public static Payment CreatePayment(Guid orderId, decimal amount, string paymentMethodType, string provider)
    {
        var money = new Money(amount);
        var paymentMethod = paymentMethodType.ToUpper() switch
        {
            "CREDIT_CARD" => PaymentMethod.CreditCard("VISA", "1234"),
            "DEBIT_CARD" => PaymentMethod.DebitCard("MASTERCARD", "5678"),
            "PIX" => PaymentMethod.Pix(),
            "BOLETO" => PaymentMethod.Boleto(),
            _ => throw new ArgumentException("Invalid payment method type", nameof(paymentMethodType))
        };

        return new Payment(orderId, money, paymentMethod, provider);
    }

    // Notifications
    public static Notification CreateEmailNotification(Guid userId, string email, string subject, string body,
        Guid? templateId = null, DateTime? scheduledAt = null)
    {
        var recipient = NotificationRecipient.Email(email);
        return new Notification(userId, recipient, body, subject, templateId, scheduledAt);
    }

    public static NotificationTemplate CreateEmailTemplate(string name, string subject, string body)
    {
        return new NotificationTemplate(name, NotificationType.Email, body, subject);
    }
}