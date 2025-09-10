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


// =====================================================================
// Configuracoes Entity Framework
// =====================================================================
// =====================================================
// IDENTITY SERVICE - DbContext and Configurations
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Data;

// =====================================================
// ENTITIES
// =====================================================

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid KeycloakUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public bool IsDeleted => DeletedAt.HasValue;
}

public class DomainEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;
}

// =====================================================
// CONFIGURATIONS
// =====================================================

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.KeycloakUserId)
            .HasColumnName("keycloak_user_id")
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(x => x.KeycloakUserId)
            .HasDatabaseName("idx_user_profiles_keycloak_user_id")
            .IsUnique();

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_user_profiles_created_at");

        builder.HasIndex(x => x.DeletedAt)
            .HasDatabaseName("idx_user_profiles_deleted_at")
            .HasFilter("deleted_at IS NULL");

        // Ignore computed properties
        builder.Ignore(x => x.FullName);
        builder.Ignore(x => x.IsDeleted);
    }
}

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("domain_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        // Indexes
        builder.HasIndex(x => x.EventId)
            .HasDatabaseName("idx_domain_events_event_id")
            .IsUnique();

        builder.HasIndex(x => x.AggregateId)
            .HasDatabaseName("idx_domain_events_aggregate_id");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("idx_domain_events_event_type");

        builder.HasIndex(x => x.OccurredAt)
            .HasDatabaseName("idx_domain_events_occurred_at");

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("idx_domain_events_processed_at")
            .HasFilter("processed_at IS NULL");

        // Ignore computed properties
        builder.Ignore(x => x.IsProcessed);
    }
}

// =====================================================
// DbContext
// =====================================================

// =====================================================
// IDENTITY SERVICE - DbContext and Configurations
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Data;

// =====================================================
// ENTITIES
// =====================================================

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid KeycloakUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public bool IsDeleted => DeletedAt.HasValue;
}

public class DomainEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;
}

// =====================================================
// CONFIGURATIONS
// =====================================================

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.KeycloakUserId)
            .HasColumnName("keycloak_user_id")
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Indexes
        builder.HasIndex(x => x.KeycloakUserId)
            .HasDatabaseName("idx_user_profiles_keycloak_user_id")
            .IsUnique();

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_user_profiles_created_at");

        builder.HasIndex(x => x.DeletedAt)
            .HasDatabaseName("idx_user_profiles_deleted_at")
            .HasFilter("deleted_at IS NULL");

        // Ignore computed properties
        builder.Ignore(x => x.FullName);
        builder.Ignore(x => x.IsDeleted);
    }
}

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("domain_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        // Indexes
        builder.HasIndex(x => x.EventId)
            .HasDatabaseName("idx_domain_events_event_id")
            .IsUnique();

        builder.HasIndex(x => x.AggregateId)
            .HasDatabaseName("idx_domain_events_aggregate_id");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("idx_domain_events_event_type");

        builder.HasIndex(x => x.OccurredAt)
            .HasDatabaseName("idx_domain_events_occurred_at");

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("idx_domain_events_processed_at")
            .HasFilter("processed_at IS NULL");

        // Ignore computed properties
        builder.Ignore(x => x.IsProcessed);
    }
}

// =====================================================
// DbContext
// =====================================================

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<DomainEvent> DomainEvents => Set<DomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
        modelBuilder.ApplyConfiguration(new DomainEventConfiguration());

        // Global query filters (soft delete)
        modelBuilder.Entity<UserProfile>()
            .HasQueryFilter(x => x.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is UserProfile &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (UserProfile)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}// =====================================================
// CATALOG SERVICE - DbContext and Configurations
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace CatalogService.Infrastructure.Data;

// =====================================================
// VALUE OBJECTS
// =====================================================

public class Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";

    public Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero => new(0);
    public static Money FromBrl(decimal amount) => new(amount, "BRL");
}

public class ProductDimensions
{
    public decimal Width { get; init; }
    public decimal Height { get; init; }
    public decimal Depth { get; init; }
    public decimal Weight { get; init; }

    public ProductDimensions(decimal width, decimal height, decimal depth, decimal weight)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Weight = weight;
    }
}

// =====================================================
// ENTITIES
// =====================================================

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];
    public List<Product> Products { get; set; } = [];

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public bool HasChildren => Children.Any();
    public bool IsRoot => ParentId == null;
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string Sku { get; set; } = string.Empty;
    public Money Price { get; set; } = Money.Zero;
    public Money? ComparePrice { get; set; }
    public Guid CategoryId { get; set; }
    public string? Brand { get; set; }
    public ProductDimensions? Dimensions { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public List<ProductImage> Images { get; set; } = [];
    public List<ProductAttribute> Attributes { get; set; } = [];

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public bool HasDiscount => ComparePrice?.Amount > Price.Amount;
    public ProductImage? MainImage => Images.OrderBy(x => x.SortOrder).FirstOrDefault();
}

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
}

public class ProductAttribute
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
}

public class DomainEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;
}

// =====================================================
// CONFIGURATIONS
// =====================================================

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.ParentId)
            .HasColumnName("parent_id");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Products)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.ParentId)
            .HasDatabaseName("idx_categories_parent_id");

        builder.HasIndex(x => x.Slug)
            .HasDatabaseName("idx_categories_slug")
            .IsUnique();

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("idx_categories_is_active");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.HasChildren);
        builder.Ignore(x => x.IsRoot);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.ShortDescription)
            .HasColumnName("short_description")
            .HasMaxLength(500);

        builder.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(x => x.Brand)
            .HasColumnName("brand")
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.IsFeatured)
            .HasColumnName("is_featured")
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Value Objects
        builder.OwnsOne(x => x.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("price_amount")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        builder.OwnsOne(x => x.ComparePrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("compare_price_amount")
                .HasColumnType("decimal(10,2)");

            money.Property(m => m.Currency)
                .HasColumnName("compare_price_currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        builder.OwnsOne(x => x.Dimensions, dimensions =>
        {
            dimensions.Property(d => d.Width)
                .HasColumnName("width")
                .HasColumnType("decimal(8,2)");

            dimensions.Property(d => d.Height)
                .HasColumnName("height")
                .HasColumnType("decimal(8,2)");

            dimensions.Property(d => d.Depth)
                .HasColumnName("depth")
                .HasColumnType("decimal(8,2)");

            dimensions.Property(d => d.Weight)
                .HasColumnName("weight")
                .HasColumnType("decimal(8,3)");
        });

        // Relationships
        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attributes)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CategoryId)
            .HasDatabaseName("idx_products_category_id");

        builder.HasIndex(x => x.Sku)
            .HasDatabaseName("idx_products_sku")
            .IsUnique();

        builder.HasIndex(x => x.Slug)
            .HasDatabaseName("idx_products_slug")
            .IsUnique();

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("idx_products_is_active");

        builder.HasIndex(x => x.IsFeatured)
            .HasDatabaseName("idx_products_is_featured");

        builder.HasIndex(x => x.Brand)
            .HasDatabaseName("idx_products_brand");

        builder.HasIndex("price_amount")
            .HasDatabaseName("idx_products_price");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.HasDiscount);
        builder.Ignore(x => x.MainImage);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.Url)
            .HasColumnName("url")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(200);

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId);

        // Indexes
        builder.HasIndex(x => x.ProductId)
            .HasDatabaseName("idx_product_images_product_id");

        builder.HasIndex(x => x.SortOrder)
            .HasDatabaseName("idx_product_images_sort_order");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
    }
}

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("product_attributes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnName("value")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasOne(x => x.Product)
            .WithMany(x => x.Attributes)
            .HasForeignKey(x => x.ProductId);

        // Indexes
        builder.HasIndex(x => x.ProductId)
            .HasDatabaseName("idx_product_attributes_product_id");

        builder.HasIndex(x => x.Name)
            .HasDatabaseName("idx_product_attributes_name");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
    }
}

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("domain_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        // Indexes
        builder.HasIndex(x => x.EventId)
            .HasDatabaseName("idx_domain_events_event_id")
            .IsUnique();

        builder.HasIndex(x => x.AggregateId)
            .HasDatabaseName("idx_domain_events_aggregate_id");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("idx_domain_events_event_type");

        // Ignore computed properties
        builder.Ignore(x => x.IsProcessed);
    }
}

// =====================================================
// DbContext
// =====================================================

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<DomainEvent> DomainEvents => Set<DomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
        modelBuilder.ApplyConfiguration(new ProductAttributeConfiguration());
        modelBuilder.ApplyConfiguration(new DomainEventConfiguration());

        // Global query filters (soft delete)
        modelBuilder.Entity<Category>()
            .HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<Product>()
            .HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<ProductImage>()
            .HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<ProductAttribute>()
            .HasQueryFilter(x => x.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.Entity.GetType().GetProperty("UpdatedAt") != null)
            {
                entityEntry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }

            if (entityEntry.State == EntityState.Added &&
                entityEntry.Entity.GetType().GetProperty("CreatedAt") != null)
            {
                entityEntry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}// =====================================================
// INVENTORY SERVICE - DbContext and Configurations
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryService.Infrastructure.Data;

// =====================================================
// ENUMS
// =====================================================

public enum MovementType
{
    In,
    Out,
    Reserve,
    Release,
    Adjustment
}

public enum ReservationStatus
{
    Active,
    Confirmed,
    Expired,
    Cancelled
}

// =====================================================
// ENTITIES
// =====================================================

public class InventoryItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; } = 0;
    public int ReservedQuantity { get; set; } = 0;
    public int MinimumStockLevel { get; set; } = 0;
    public int? MaximumStockLevel { get; set; }
    public int ReorderPoint { get; set; } = 0;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public List<StockMovement> Movements { get; set; } = [];
    public List<StockReservation> Reservations { get; set; } = [];

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public int TotalQuantity => AvailableQuantity + ReservedQuantity;
    public bool IsLowStock => AvailableQuantity <= MinimumStockLevel;
    public bool NeedsReorder => AvailableQuantity <= ReorderPoint;
    public bool IsOutOfStock => AvailableQuantity <= 0;
    public List<StockReservation> ActiveReservations =>
        Reservations.Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt > DateTime.UtcNow).ToList();
}

public class StockMovement
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public InventoryItem InventoryItem { get; set; } = null!;

    // Computed properties
    public bool IsInbound => MovementType == MovementType.In || MovementType == MovementType.Release;
    public bool IsOutbound => MovementType == MovementType.Out || MovementType == MovementType.Reserve;
}

public class StockReservation
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public Guid OrderId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public InventoryItem InventoryItem { get; set; } = null!;

    // Computed properties
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => Status == ReservationStatus.Active && !IsExpired;
    public TimeSpan TimeUntilExpiry => ExpiresAt - DateTime.UtcNow;
}

public class DomainEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;
}

// =====================================================
// CONFIGURATIONS
// =====================================================

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AvailableQuantity)
            .HasColumnName("available_quantity")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.ReservedQuantity)
            .HasColumnName("reserved_quantity")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.MinimumStockLevel)
            .HasColumnName("minimum_stock_level")
            .HasDefaultValue(0);

        builder.Property(x => x.MaximumStockLevel)
            .HasColumnName("maximum_stock_level");

        builder.Property(x => x.ReorderPoint)
            .HasColumnName("reorder_point")
            .HasDefaultValue(0);

        builder.Property(x => x.Location)
            .HasColumnName("location")
            .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasMany(x => x.Movements)
            .WithOne(x => x.InventoryItem)
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reservations)
            .WithOne(x => x.InventoryItem)
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ProductId)
            .HasDatabaseName("idx_inventory_items_product_id")
            .IsUnique();

        builder.HasIndex(x => x.Sku)
            .HasDatabaseName("idx_inventory_items_sku")
            .IsUnique();

        builder.HasIndex(x => x.AvailableQuantity)
            .HasDatabaseName("idx_inventory_items_available_quantity");

        builder.HasIndex(x => x.Location)
            .HasDatabaseName("idx_inventory_items_location");

        // Check constraints
        builder.HasCheckConstraint("chk_available_quantity_positive", "available_quantity >= 0");
        builder.HasCheckConstraint("chk_reserved_quantity_positive", "reserved_quantity >= 0");
        builder.HasCheckConstraint("chk_minimum_stock_positive", "minimum_stock_level >= 0");
        builder.HasCheckConstraint("chk_reorder_point_positive", "reorder_point >= 0");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.TotalQuantity);
        builder.Ignore(x => x.IsLowStock);
        builder.Ignore(x => x.NeedsReorder);
        builder.Ignore(x => x.IsOutOfStock);
        builder.Ignore(x => x.ActiveReservations);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.InventoryItemId)
            .HasColumnName("inventory_item_id")
            .IsRequired();

        builder.Property(x => x.MovementType)
            .HasColumnName("movement_type")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(x => x.ReferenceType)
            .HasColumnName("reference_type")
            .HasMaxLength(50);

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by");

        // Relationships
        builder.HasOne(x => x.InventoryItem)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.InventoryItemId);

        // Indexes
        builder.HasIndex(x => x.InventoryItemId)
            .HasDatabaseName("idx_stock_movements_inventory_item_id");

        builder.HasIndex(x => x.MovementType)
            .HasDatabaseName("idx_stock_movements_movement_type");

        builder.HasIndex(new[] { "reference_id", "reference_type" })
            .HasDatabaseName("idx_stock_movements_reference");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_stock_movements_created_at");

        // Check constraints
        builder.HasCheckConstraint("chk_quantity_not_zero", "quantity != 0");

        // Ignore computed properties
        builder.Ignore(x => x.IsInbound);
        builder.Ignore(x => x.IsOutbound);
    }
}

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.InventoryItemId)
            .HasColumnName("inventory_item_id")
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(ReservationStatus.Active)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasOne(x => x.InventoryItem)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.InventoryItemId);

        // Indexes
        builder.HasIndex(x => x.InventoryItemId)
            .HasDatabaseName("idx_stock_reservations_inventory_item_id");

        builder.HasIndex(x => x.OrderId)
            .HasDatabaseName("idx_stock_reservations_order_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_stock_reservations_status");

        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("idx_stock_reservations_expires_at");

        // Check constraints
        builder.HasCheckConstraint("chk_reservation_quantity_positive", "quantity > 0");

        // Ignore computed properties
        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.TimeUntilExpiry);
    }
}

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("domain_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        // Indexes
        builder.HasIndex(x => x.EventId)
            .HasDatabaseName("idx_domain_events_event_id")
            .IsUnique();

        builder.HasIndex(x => x.AggregateId)
            .HasDatabaseName("idx_domain_events_aggregate_id");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("idx_domain_events_event_type");

        // Ignore computed properties
        builder.Ignore(x => x.IsProcessed);
    }
}

// =====================================================
// DbContext
// =====================================================

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<DomainEvent> DomainEvents => Set<DomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
        modelBuilder.ApplyConfiguration(new StockMovementConfiguration());
        modelBuilder.ApplyConfiguration(new StockReservationConfiguration());
        modelBuilder.ApplyConfiguration(new DomainEventConfiguration());

        // Global query filters (soft delete)
        modelBuilder.Entity<InventoryItem>()
            .HasQueryFilter(x => x.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.Entity is InventoryItem inventoryItem)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    inventoryItem.CreatedAt = DateTime.UtcNow;
                }
                if (entityEntry.State == EntityState.Modified)
                {
                    inventoryItem.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (entityEntry.Entity is StockReservation reservation &&
                entityEntry.State == EntityState.Modified)
            {
                reservation.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}// =====================================================
// ORDER SERVICE - DbContext and Configurations
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderService.Infrastructure.Data;

// =====================================================
// ENUMS
// =====================================================

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

// =====================================================
// VALUE OBJECTS
// =====================================================

public class Address
{
    public string Street { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string? Complement { get; init; }
    public string Neighborhood { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = "Brasil";

    public Address(string street, string number, string neighborhood, string city,
                  string state, string postalCode, string? complement = null, string country = "Brasil")
    {
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public string GetFullAddress()
    {
        var address = $"{Street}, {Number}";
        if (!string.IsNullOrWhiteSpace(Complement))
            address += $", {Complement}";
        address += $", {Neighborhood}, {City}/{State}, {PostalCode}";
        return address;
    }
}

public class Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";

    public Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero => new(0);
    public static Money FromBrl(decimal amount) => new(amount, "BRL");

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier)
    {
        return new Money(money.Amount * multiplier, money.Currency);
    }
}

// =====================================================
// ENTITIES
// =====================================================

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Money TotalAmount { get; set; } = Money.Zero;
    public Address? ShippingAddress { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public List<OrderItem> Items { get; set; } = [];
    public List<OrderStatusHistory> StatusHistory { get; set; } = [];

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
    public bool IsCompleted => Status == OrderStatus.Delivered;
    public bool IsCancelled => Status == OrderStatus.Cancelled;
    public Money SubTotal => Items.Aggregate(Money.Zero, (acc, item) => acc + item.TotalPrice);
    public int TotalItems => Items.Sum(x => x.Quantity);
    public OrderItem? GetItem(Guid productId) => Items.FirstOrDefault(x => x.ProductId == productId);
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public Money UnitPrice { get; set; } = Money.Zero;
    public Money TotalPrice { get; set; } = Money.Zero;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public Money CalculatedTotalPrice => UnitPrice * Quantity;

    // Methods
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        Quantity = newQuantity;
        TotalPrice = UnitPrice * newQuantity;
    }

    public void UpdatePrice(Money newUnitPrice)
    {
        UnitPrice = newUnitPrice;
        TotalPrice = newUnitPrice * Quantity;
    }
}

public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;

    // Computed properties
    public bool IsInitialStatus => !FromStatus.HasValue;
}

public class DomainEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;
}

// =====================================================
// CONFIGURATIONS
// =====================================================

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(OrderStatus.Pending)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Value Objects
        builder.OwnsOne(x => x.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("total_amount")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        builder.OwnsOne(x => x.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("shipping_street")
                .HasMaxLength(200);

            address.Property(a => a.Number)
                .HasColumnName("shipping_number")
                .HasMaxLength(20);

            address.Property(a => a.Complement)
                .HasColumnName("shipping_complement")
                .HasMaxLength(100);

            address.Property(a => a.Neighborhood)
                .HasColumnName("shipping_neighborhood")
                .HasMaxLength(100);

            address.Property(a => a.City)
                .HasColumnName("shipping_city")
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("shipping_state")
                .HasMaxLength(50);

            address.Property(a => a.PostalCode)
                .HasColumnName("shipping_postal_code")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("shipping_country")
                .HasMaxLength(50)
                .HasDefaultValue("Brasil");
        });

        builder.OwnsOne(x => x.BillingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("billing_street")
                .HasMaxLength(200);

            address.Property(a => a.Number)
                .HasColumnName("billing_number")
                .HasMaxLength(20);

            address.Property(a => a.Complement)
                .HasColumnName("billing_complement")
                .HasMaxLength(100);

            address.Property(a => a.Neighborhood)
                .HasColumnName("billing_neighborhood")
                .HasMaxLength(100);

            address.Property(a => a.City)
                .HasColumnName("billing_city")
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("billing_state")
                .HasMaxLength(50);

            address.Property(a => a.PostalCode)
                .HasColumnName("billing_postal_code")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("billing_country")
                .HasMaxLength(50)
                .HasDefaultValue("Brasil");
        });

        // Relationships
        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.CustomerId)
            .HasDatabaseName("idx_orders_customer_id");

        builder.HasIndex(x => x.OrderNumber)
            .HasDatabaseName("idx_orders_order_number")
            .IsUnique();

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_orders_status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_orders_created_at");

        builder.HasIndex("total_amount")
            .HasDatabaseName("idx_orders_total_amount");

        // Check constraints
        builder.HasCheckConstraint("chk_total_amount_positive", "total_amount >= 0");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.CanBeCancelled);
        builder.Ignore(x => x.IsCompleted);
        builder.Ignore(x => x.IsCancelled);
        builder.Ignore(x => x.SubTotal);
        builder.Ignore(x => x.TotalItems);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Value Objects
        builder.OwnsOne(x => x.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("unit_price")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        builder.OwnsOne(x => x.TotalPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("total_price")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        // Relationships
        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId);

        // Indexes
        builder.HasIndex(x => x.OrderId)
            .HasDatabaseName("idx_order_items_order_id");

        builder.HasIndex(x => x.ProductId)
            .HasDatabaseName("idx_order_items_product_id");

        builder.HasIndex(x => x.Sku)
            .HasDatabaseName("idx_order_items_sku");

        // Check constraints
        builder.HasCheckConstraint("chk_quantity_positive", "quantity > 0");
        builder.HasCheckConstraint("chk_unit_price_positive", "unit_price >= 0");
        builder.HasCheckConstraint("chk_total_price_positive", "total_price >= 0");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.CalculatedTotalPrice);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.FromStatus)
            .HasColumnName("from_status")
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.ToStatus)
            .HasColumnName("to_status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(200);

        builder.Property(x => x.ChangedBy)
            .HasColumnName("changed_by");

        builder.Property(x => x.ChangedAt)
            .HasColumnName("changed_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Order)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.OrderId);

        // Indexes
        builder.HasIndex(x => x.OrderId)
            .HasDatabaseName("idx_order_status_history_order_id");

        builder.HasIndex(x => x.ChangedAt)
            .HasDatabaseName("idx_order_status_history_changed_at");

        // Ignore computed properties
        builder.Ignore(x => x.IsInitialStatus);
    }
}

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("domain_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        // Indexes
        builder.HasIndex(x => x.EventId)
            .HasDatabaseName("idx_domain_events_event_id")
            .IsUnique();

        builder.HasIndex(x => x.AggregateId)
            .HasDatabaseName("idx_domain_events_aggregate_id");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("idx_domain_events_event_type");

        // Ignore computed properties
        builder.Ignore(x => x.IsProcessed);
    }
}

// =====================================================
// DbContext
// =====================================================

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<DomainEvent> DomainEvents => Set<DomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new OrderStatusHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new DomainEventConfiguration());

        // Global query filters (soft delete)
        modelBuilder.Entity<Order>()
            .HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<OrderItem>()
            .HasQueryFilter(x => x.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.Entity is Order order)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    order.CreatedAt = DateTime.UtcNow;

                    // Generate order number if not set
                    if (string.IsNullOrEmpty(order.OrderNumber))
                    {
                        order.OrderNumber = await GenerateOrderNumberAsync();
                    }
                }
                if (entityEntry.State == EntityState.Modified)
                {
                    order.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (entityEntry.Entity is OrderItem orderItem)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    orderItem.CreatedAt = DateTime.UtcNow;
                }
                if (entityEntry.State == EntityState.Modified)
                {
                    orderItem.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var yearPrefix = year.ToString();

        // Get the last order number for the current year
        var lastOrder = await Orders
            .Where(o => o.OrderNumber.StartsWith(yearPrefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastOrder != null && lastOrder.OrderNumber.Length >= 12)
        {
            var numberPart = lastOrder.OrderNumber.Substring(4);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{yearPrefix}{nextNumber:D8}";
    }
}// =====================================================
// PAYMENT SERVICE - DbContext and Configurations
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaymentService.Infrastructure.Data;

// =====================================================
// ENUMS
// =====================================================

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    Pix,
    Boleto,
    PayPal
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Refunded
}

public enum RefundStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

// =====================================================
// VALUE OBJECTS
// =====================================================

public class Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";

    public Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero => new(0);
    public static Money FromBrl(decimal amount) => new(amount, "BRL");

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        return new Money(left.Amount - right.Amount, left.Currency);
    }
}

public class CardInfo
{
    public string LastFour { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;

    public CardInfo(string lastFour, string brand, string token)
    {
        LastFour = lastFour;
        Brand = brand;
        Token = token;
    }

    public string MaskedNumber => $"**** **** **** {LastFour}";
}

public class PixInfo
{
    public string Key { get; init; } = string.Empty;
    public string QrCode { get; init; } = string.Empty;

    public PixInfo(string key, string qrCode)
    {
        Key = key;
        QrCode = qrCode;
    }
}

public class BoletoInfo
{
    public string Barcode { get; init; } = string.Empty;
    public DateOnly DueDate { get; init; }

    public BoletoInfo(string barcode, DateOnly dueDate)
    {
        Barcode = barcode;
        DueDate = dueDate;
    }

    public bool IsExpired => DateOnly.FromDateTime(DateTime.Today) > DueDate;
}

// =====================================================
// ENTITIES
// =====================================================

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Money Amount { get; set; } = Money.Zero;
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Gateway Info
    public string? GatewayProvider { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayPaymentId { get; set; }

    // Payment Method Specific Info
    public CardInfo? CardInfo { get; set; }
    public PixInfo? PixInfo { get; set; }
    public BoletoInfo? BoletoInfo { get; set; }

    public string? FailureReason { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public List<PaymentAttempt> Attempts { get; set; } = [];
    public List<Refund> Refunds { get; set; } = [];

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public bool IsCompleted => Status == PaymentStatus.Completed;
    public bool IsPending => Status == PaymentStatus.Pending;
    public bool HasFailed => Status == PaymentStatus.Failed;
    public bool CanBeRetried => HasFailed && Attempts.Count < 3;
    public bool CanBeRefunded => IsCompleted;
    public Money TotalRefunded => Refunds
        .Where(r => r.Status == RefundStatus.Completed)
        .Aggregate(Money.Zero, (acc, refund) => acc + refund.Amount);
    public Money RefundableAmount => Amount - TotalRefunded;
    public bool IsFullyRefunded => TotalRefunded.Amount >= Amount.Amount;
}

public class PaymentAttempt
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public int AttemptNumber { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayResponse { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime AttemptedAt { get; set; }

    // Navigation properties
    public Payment Payment { get; set; } = null!;

    // Computed properties
    public bool IsSuccessful => Status == PaymentStatus.Completed;
    public bool IsFailed => Status == PaymentStatus.Failed;
}

public class Refund
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Money Amount { get; set; } = Money.Zero;
    public string? Reason { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public string? GatewayRefundId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Payment Payment { get; set; } = null!;

    // Computed properties
    public bool IsCompleted => Status == RefundStatus.Completed;
    public bool IsPending => Status == RefundStatus.Pending;
    public bool HasFailed => Status == RefundStatus.Failed;
    public bool IsPartialRefund => Amount.Amount < Payment.Amount.Amount;
}

public class DomainEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;
}

// =====================================================
// CONFIGURATIONS
// =====================================================

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasColumnName("payment_method")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(PaymentStatus.Pending)
            .IsRequired();

        builder.Property(x => x.GatewayProvider)
            .HasColumnName("gateway_provider")
            .HasMaxLength(50);

        builder.Property(x => x.GatewayTransactionId)
            .HasColumnName("gateway_transaction_id")
            .HasMaxLength(100);

        builder.Property(x => x.GatewayPaymentId)
            .HasColumnName("gateway_payment_id")
            .HasMaxLength(100);

        builder.Property(x => x.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Value Objects
        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        builder.OwnsOne(x => x.CardInfo, card =>
        {
            card.Property(c => c.LastFour)
                .HasColumnName("card_last_four")
                .HasMaxLength(4);

            card.Property(c => c.Brand)
                .HasColumnName("card_brand")
                .HasMaxLength(20);

            card.Property(c => c.Token)
                .HasColumnName("card_token")
                .HasMaxLength(200);
        });

        builder.OwnsOne(x => x.PixInfo, pix =>
        {
            pix.Property(p => p.Key)
                .HasColumnName("pix_key")
                .HasMaxLength(200);

            pix.Property(p => p.QrCode)
                .HasColumnName("pix_qr_code");
        });

        builder.OwnsOne(x => x.BoletoInfo, boleto =>
        {
            boleto.Property(b => b.Barcode)
                .HasColumnName("boleto_barcode")
                .HasMaxLength(100);

            boleto.Property(b => b.DueDate)
                .HasColumnName("boleto_due_date");
        });

        // Relationships
        builder.HasMany(x => x.Attempts)
            .WithOne(x => x.Payment)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Refunds)
            .WithOne(x => x.Payment)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OrderId)
            .HasDatabaseName("idx_payments_order_id");

        builder.HasIndex(x => x.CustomerId)
            .HasDatabaseName("idx_payments_customer_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_payments_status");

        builder.HasIndex(x => x.PaymentMethod)
            .HasDatabaseName("idx_payments_payment_method");

        builder.HasIndex(x => x.GatewayTransactionId)
            .HasDatabaseName("idx_payments_gateway_transaction_id");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_payments_created_at");

        // Check constraints
        builder.HasCheckConstraint("chk_amount_positive", "amount > 0");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.IsCompleted);
        builder.Ignore(x => x.IsPending);
        builder.Ignore(x => x.HasFailed);
        builder.Ignore(x => x.CanBeRetried);
        builder.Ignore(x => x.CanBeRefunded);
        builder.Ignore(x => x.TotalRefunded);
        builder.Ignore(x => x.RefundableAmount);
        builder.Ignore(x => x.IsFullyRefunded);
    }
}

public class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("payment_attempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.AttemptNumber)
            .HasColumnName("attempt_number")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.GatewayResponse)
            .HasColumnName("gateway_response")
            .HasColumnType("jsonb");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(500);

        builder.Property(x => x.AttemptedAt)
            .HasColumnName("attempted_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Payment)
            .WithMany(x => x.Attempts)
            .HasForeignKey(x => x.PaymentId);

        // Indexes
        builder.HasIndex(x => x.PaymentId)
            .HasDatabaseName("idx_payment_attempts_payment_id");

        builder.HasIndex(x => x.AttemptedAt)
            .HasDatabaseName("idx_payment_attempts_attempted_at");

        // Ignore computed properties
        builder.Ignore(x => x.IsSuccessful);
        builder.Ignore(x => x.IsFailed);
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(RefundStatus.Pending)
            .IsRequired();

        builder.Property(x => x.GatewayRefundId)
            .HasColumnName("gateway_refund_id")
            .HasMaxLength(100);

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        // Value Objects
        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .HasDefaultValue("BRL");
        });

        // Relationships
        builder.HasOne(x => x.Payment)
            .WithMany(x => x.Refunds)
            .HasForeignKey(x => x.PaymentId);

        // Indexes
        builder.HasIndex(x => x.PaymentId)
            .HasDatabaseName("idx_refunds_payment_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_refunds_status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_refunds_created_at");

        // Check constraints
        builder.HasCheckConstraint("chk_refund_amount_positive", "amount > 0");

        // Ignore computed properties
        builder.Ignore(x => x.IsCompleted);
        builder.Ignore(x => x.IsPending);
        builder.Ignore(x => x.HasFailed);
        builder.Ignore(x => x.IsPartialRefund);
    }
}

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("domain_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        // Indexes
        builder.HasIndex(x => x.EventId)
            .HasDatabaseName("idx_domain_events_event_id")
            .IsUnique();

        builder.HasIndex(x => x.AggregateId)
            .HasDatabaseName("idx_domain_events_aggregate_id");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("idx_domain_events_event_type");

        // Ignore computed properties
        builder.Ignore(x => x.IsProcessed);
    }
}

// =====================================================
// DbContext
// =====================================================

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<DomainEvent> DomainEvents => Set<DomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentAttemptConfiguration());
        modelBuilder.ApplyConfiguration(new RefundConfiguration());
        modelBuilder.ApplyConfiguration(new DomainEventConfiguration());

        // Global query filters (soft delete)
        modelBuilder.Entity<Payment>()
            .HasQueryFilter(x => x.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.Entity is Payment payment)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    payment.CreatedAt = DateTime.UtcNow;
                }
                if (entityEntry.State == EntityState.Modified)
                {
                    payment.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (entityEntry.Entity is Refund refund &&
                entityEntry.State == EntityState.Modified)
            {
                refund.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}// =====================================================
// NOTIFICATION SERVICE - DbContext and Configurations
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace NotificationService.Infrastructure.Data;

// =====================================================
// ENUMS
// =====================================================

public enum NotificationType
{
    Email,
    Sms,
    Push,
    InApp
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Failed,
    Cancelled
}

public enum NotificationChannel
{
    OrderConfirmation,
    PaymentSuccess,
    PaymentFailed,
    ShippingUpdate,
    OrderDelivered,
    UserWelcome,
    PasswordReset,
    AccountVerification,
    PromotionalOffer,
    SystemAlert
}

// =====================================================
// VALUE OBJECTS
// =====================================================

public class NotificationMetadata
{
    public Dictionary<string, object> Data { get; init; } = new();

    public NotificationMetadata()
    {
    }

    public NotificationMetadata(Dictionary<string, object> data)
    {
        Data = data ?? new Dictionary<string, object>();
    }

    public T? GetValue<T>(string key)
    {
        if (Data.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    public void SetValue(string key, object value)
    {
        Data[key] = value;
    }

    public bool HasValue(string key) => Data.ContainsKey(key);
}

public class TemplateVariables
{
    public Dictionary<string, string> Variables { get; init; } = new();

    public TemplateVariables()
    {
    }

    public TemplateVariables(Dictionary<string, string> variables)
    {
        Variables = variables ?? new Dictionary<string, string>();
    }

    public string? GetVariable(string name)
    {
        return Variables.TryGetValue(name, out var value) ? value : null;
    }

    public void SetVariable(string name, string value)
    {
        Variables[name] = value;
    }

    public bool HasVariable(string name) => Variables.ContainsKey(name);

    public List<string> GetVariableNames() => Variables.Keys.ToList();
}

// =====================================================
// ENTITIES
// =====================================================

public class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public string? Subject { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    // Delivery Info
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }

    // Provider Info
    public string? Provider { get; set; }
    public string? ProviderMessageId { get; set; }

    // Metadata
    public NotificationMetadata Metadata { get; set; } = new();
    public int Priority { get; set; } = 5; // 1 (highest) to 10 (lowest)

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public bool IsDelivered => Status == NotificationStatus.Delivered;
    public bool IsPending => Status == NotificationStatus.Pending;
    public bool HasFailed => Status == NotificationStatus.Failed;
    public bool IsHighPriority => Priority <= 3;
    public bool IsLowPriority => Priority >= 7;
    public TimeSpan? DeliveryTime => SentAt.HasValue && DeliveredAt.HasValue
        ? DeliveredAt.Value - SentAt.Value
        : null;
}

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? SubjectTemplate { get; set; }
    public string ContentTemplate { get; set; } = string.Empty;
    public TemplateVariables Variables { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Computed properties
    public bool IsDeleted => DeletedAt.HasValue;
    public bool IsEmailTemplate => Type == NotificationType.Email;
    public bool IsSmsTemplate => Type == NotificationType.Sms;
    public List<string> RequiredVariables => Variables.GetVariableNames();

    // Methods
    public string RenderSubject(Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(SubjectTemplate))
            return string.Empty;

        var rendered = SubjectTemplate;
        foreach (var kvp in values)
        {
            rendered = rendered.Replace($"#{{{kvp.Key}}}", kvp.Value);
        }
        return rendered;
    }

    public string RenderContent(Dictionary<string, string> values)
    {
        var rendered = ContentTemplate;
        foreach (var kvp in values)
        {
            rendered = rendered.Replace($"#{{{kvp.Key}}}", kvp.Value);
        }
        return rendered;
    }

    public bool ValidateVariables(Dictionary<string, string> values)
    {
        return RequiredVariables.All(variable => values.ContainsKey(variable));
    }
}

public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationType Type { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed properties
    public bool IsDisabled => !Enabled;
}

public class DomainEvent
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;
}

// =====================================================
// CONFIGURATIONS
// =====================================================

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.RecipientId)
            .HasColumnName("recipient_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasColumnName("subject")
            .HasMaxLength(200);

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(x => x.RecipientEmail)
            .HasColumnName("recipient_email")
            .HasMaxLength(200);

        builder.Property(x => x.RecipientPhone)
            .HasColumnName("recipient_phone")
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(NotificationStatus.Pending)
            .IsRequired();

        builder.Property(x => x.SentAt)
            .HasColumnName("sent_at");

        builder.Property(x => x.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.Property(x => x.FailedAt)
            .HasColumnName("failed_at");

        builder.Property(x => x.FailureReason)
            .HasColumnName("failure_reason")
            .HasMaxLength(500);

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasMaxLength(50);

        builder.Property(x => x.ProviderMessageId)
            .HasColumnName("provider_message_id")
            .HasMaxLength(200);

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasDefaultValue(5)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Value Objects
        builder.OwnsOne(x => x.Metadata, metadata =>
        {
            metadata.Property(m => m.Data)
                .HasColumnName("metadata")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new()
                );
        });

        // Indexes
        builder.HasIndex(x => x.RecipientId)
            .HasDatabaseName("idx_notifications_recipient_id");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("idx_notifications_type");

        builder.HasIndex(x => x.Channel)
            .HasDatabaseName("idx_notifications_channel");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_notifications_status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("idx_notifications_created_at");

        builder.HasIndex(x => x.Priority)
            .HasDatabaseName("idx_notifications_priority");

        builder.HasIndex(x => x.ProviderMessageId)
            .HasDatabaseName("idx_notifications_provider_message_id");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.IsDelivered);
        builder.Ignore(x => x.IsPending);
        builder.Ignore(x => x.HasFailed);
        builder.Ignore(x => x.IsHighPriority);
        builder.Ignore(x => x.IsLowPriority);
        builder.Ignore(x => x.DeliveryTime);
    }
}

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.SubjectTemplate)
            .HasColumnName("subject_template")
            .HasMaxLength(200);

        builder.Property(x => x.ContentTemplate)
            .HasColumnName("content_template")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Value Objects
        builder.OwnsOne(x => x.Variables, variables =>
        {
            variables.Property(v => v.Variables)
                .HasColumnName("variables")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new()
                );
        });

        // Indexes
        builder.HasIndex(x => x.Name)
            .HasDatabaseName("idx_notification_templates_name")
            .IsUnique();

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("idx_notification_templates_type");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("idx_notification_templates_is_active");

        // Ignore computed properties
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.IsEmailTemplate);
        builder.Ignore(x => x.IsSmsTemplate);
        builder.Ignore(x => x.RequiredVariables);
    }
}

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Enabled)
            .HasColumnName("enabled")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        // Composite unique constraint
        builder.HasIndex(new[] { "user_id", "channel", "type" })
            .HasDatabaseName("idx_notification_preferences_unique")
            .IsUnique();

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("idx_notification_preferences_user_id");

        builder.HasIndex(new[] { "channel", "type" })
            .HasDatabaseName("idx_notification_preferences_channel_type");

        // Ignore computed properties
        builder.Ignore(x => x.IsDisabled);
    }
}

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("domain_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()")
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .IsRequired();

        builder.Property(x => x.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        // Indexes
        builder.HasIndex(x => x.EventId)
            .HasDatabaseName("idx_domain_events_event_id")
            .IsUnique();

        builder.HasIndex(x => x.AggregateId)
            .HasDatabaseName("idx_domain_events_aggregate_id");

        builder.HasIndex(x => x.EventType)
            .HasDatabaseName("idx_domain_events_event_type");

        // Ignore computed properties
        builder.Ignore(x => x.IsProcessed);
    }
}

// =====================================================
// DbContext
// =====================================================

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<DomainEvent> DomainEvents => Set<DomainEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
        modelBuilder.ApplyConfiguration(new DomainEventConfiguration());

        // Global query filters (soft delete)
        modelBuilder.Entity<Notification>()
            .HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<NotificationTemplate>()
            .HasQueryFilter(x => x.DeletedAt == null);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps automatically
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.Entity is Notification notification)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    notification.CreatedAt = DateTime.UtcNow;
                }
                if (entityEntry.State == EntityState.Modified)
                {
                    notification.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (entityEntry.Entity is NotificationTemplate template)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    template.CreatedAt = DateTime.UtcNow;
                }
                if (entityEntry.State == EntityState.Modified)
                {
                    template.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (entityEntry.Entity is NotificationPreference preference &&
                entityEntry.State == EntityState.Modified)
            {
                preference.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}// =====================================================
// SERVICES CONFIGURATION AND DEPENDENCY INJECTION
// =====================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Services.Configuration;

// =====================================================
// IDENTITY SERVICE CONFIGURATION
// =====================================================

public static class IdentityServiceConfiguration
{
    public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("IdentityConnection"))
                   .EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                   .EnableDetailedErrors());

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("IdentityConnection")!, name: "identity-db");

        return services;
    }
}

// =====================================================
// CATALOG SERVICE CONFIGURATION
// =====================================================

public static class CatalogServiceConfiguration
{
    public static IServiceCollection AddCatalogService(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CatalogConnection"))
                   .EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                   .EnableDetailedErrors());

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("CatalogConnection")!, name: "catalog-db")
            .AddRedis(configuration.GetConnectionString("Redis")!, name: "redis-cache");

        return services;
    }
}

// =====================================================
// INVENTORY SERVICE CONFIGURATION
// =====================================================

public static class InventoryServiceConfiguration
{
    public static IServiceCollection AddInventoryService(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("InventoryConnection"))
                   .EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                   .EnableDetailedErrors());

        // Redis for Caching and Distributed Locks
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("InventoryConnection")!, name: "inventory-db")
            .AddRedis(configuration.GetConnectionString("Redis")!, name: "redis-cache");

        return services;
    }
}

// =====================================================
// ORDER SERVICE CONFIGURATION
// =====================================================

public static class OrderServiceConfiguration
{
    public static IServiceCollection AddOrderService(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("OrderConnection"))
                   .EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                   .EnableDetailedErrors());

        // Redis for Cart
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("OrderConnection")!, name: "order-db")
            .AddRedis(configuration.GetConnectionString("Redis")!, name: "redis-cache");

        return services;
    }
}

// =====================================================
// PAYMENT SERVICE CONFIGURATION
// =====================================================

public static class PaymentServiceConfiguration
{
    public static IServiceCollection AddPaymentService(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PaymentConnection"))
                   .EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                   .EnableDetailedErrors());

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("PaymentConnection")!, name: "payment-db");

        return services;
    }
}

// =====================================================
// NOTIFICATION SERVICE CONFIGURATION
// =====================================================

public static class NotificationServiceConfiguration
{
    public static IServiceCollection AddNotificationService(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationConnection"))
                   .EnableSensitiveDataLogging(configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
                   .EnableDetailedErrors());

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("NotificationConnection")!, name: "notification-db");

        return services;
    }
}

// =====================================================
// APPSETTINGS.JSON EXAMPLES
// =====================================================

/*
// appsettings.Development.json example:
{
  "ConnectionStrings": {
    "IdentityConnection": "Host=localhost;Database=identity_service;Username=postgres;Password=dev_password",
    "CatalogConnection": "Host=localhost;Database=catalog_service;Username=postgres;Password=dev_password",
    "InventoryConnection": "Host=localhost;Database=inventory_service;Username=postgres;Password=dev_password",
    "OrderConnection": "Host=localhost;Database=order_service;Username=postgres;Password=dev_password",
    "PaymentConnection": "Host=localhost;Database=payment_service;Username=postgres;Password=dev_password",
    "NotificationConnection": "Host=localhost;Database=notification_service;Username=postgres;Password=dev_password",
    "Redis": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    },
    "EnableSensitiveDataLogging": true
  },
  "Jwt": {
    "Authority": "http://localhost:8080/realms/bcommerce",
    "Audience": "bcommerce-api",
    "RequireHttpsMetadata": false
  }
}

// appsettings.Production.json example:
{
  "ConnectionStrings": {
    "IdentityConnection": "Host=prod-db-server;Database=identity_service;Username=app_user;Password=${IDENTITY_DB_PASSWORD}",
    "CatalogConnection": "Host=prod-db-server;Database=catalog_service;Username=app_user;Password=${CATALOG_DB_PASSWORD}",
    "InventoryConnection": "Host=prod-db-server;Database=inventory_service;Username=app_user;Password=${INVENTORY_DB_PASSWORD}",
    "OrderConnection": "Host=prod-db-server;Database=order_service;Username=app_user;Password=${ORDER_DB_PASSWORD}",
    "PaymentConnection": "Host=prod-db-server;Database=payment_service;Username=app_user;Password=${PAYMENT_DB_PASSWORD}",
    "NotificationConnection": "Host=prod-db-server;Database=notification_service;Username=app_user;Password=${NOTIFICATION_DB_PASSWORD}",
    "Redis": "prod-redis-cluster:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "EnableSensitiveDataLogging": false
  },
  "Jwt": {
    "Authority": "https://auth.yourdomain.com/realms/bcommerce",
    "Audience": "bcommerce-api",
    "RequireHttpsMetadata": true
  }
}
*/

// =====================================================
// PROGRAM.CS EXAMPLES FOR EACH SERVICE
// =====================================================

// Identity Service Program.cs
/*
using IdentityService.Infrastructure.Data;
using ECommerce.Services.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddIdentityService(builder.Configuration);

// Add authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Jwt:RequireHttpsMetadata")
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");

// Minimal API endpoints
app.MapGet("/api/profiles/{keycloakUserId}", async (Guid keycloakUserId, IdentityDbContext context) =>
{
    var profile = await context.UserProfiles
        .FirstOrDefaultAsync(p => p.KeycloakUserId == keycloakUserId);
    
    return profile is not null ? Results.Ok(profile) : Results.NotFound();
})
.RequireAuthorization();

// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
*/

// Catalog Service Program.cs
/*
using CatalogService.Infrastructure.Data;
using ECommerce.Services.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddCatalogService(builder.Configuration);

// Add authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Jwt:RequireHttpsMetadata")
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");

// Minimal API endpoints
app.MapGet("/api/products", async (CatalogDbContext context, int page = 1, int pageSize = 20) =>
{
    var products = await context.Products
        .Include(p => p.Category)
        .Include(p => p.Images.OrderBy(i => i.SortOrder))
        .Where(p => p.IsActive)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return Results.Ok(products);
});

app.MapGet("/api/products/{id}", async (Guid id, CatalogDbContext context) =>
{
    var product = await context.Products
        .Include(p => p.Category)
        .Include(p => p.Images.OrderBy(i => i.SortOrder))
        .Include(p => p.Attributes)
        .FirstOrDefaultAsync(p => p.Id == id);
    
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
*/

// =====================================================
// DOCKER COMPOSE CONFIGURATION
// =====================================================

/*
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:15-alpine
    container_name: ecommerce_postgres
    environment:
      POSTGRES_PASSWORD: dev_password
      POSTGRES_DB: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./sql/init-databases.sql:/docker-entrypoint-initdb.d/init-databases.sql
    networks:
      - ecommerce-network

  # Redis Cache
  redis:
    image: redis:7-alpine
    container_name: ecommerce_redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - ecommerce-network

  # RabbitMQ for messaging
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: ecommerce_rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - ecommerce-network

  # Keycloak Authentication
  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    container_name: ecommerce_keycloak
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin123
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: postgres
      KC_DB_PASSWORD: dev_password
    ports:
      - "8080:8080"
    depends_on:
      - postgres
    command: start-dev
    networks:
      - ecommerce-network

  # Identity Service
  identity-service:
    build:
      context: ./src/services/identity-service
      dockerfile: Dockerfile
    container_name: identity_service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__IdentityConnection=Host=postgres;Database=identity_service;Username=postgres;Password=dev_password
      - ConnectionStrings__Redis=redis:6379
      - Jwt__Authority=http://keycloak:8080/realms/bcommerce
    ports:
      - "5001:8080"
    depends_on:
      - postgres
      - redis
      - keycloak
    networks:
      - ecommerce-network

  # Catalog Service
  catalog-service:
    build:
      context: ./src/services/catalog-service
      dockerfile: Dockerfile
    container_name: catalog_service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__CatalogConnection=Host=postgres;Database=catalog_service;Username=postgres;Password=dev_password
      - ConnectionStrings__Redis=redis:6379
      - Jwt__Authority=http://keycloak:8080/realms/bcommerce
    ports:
      - "5002:8080"
    depends_on:
      - postgres
      - redis
      - keycloak
    networks:
      - ecommerce-network

  # Inventory Service
  inventory-service:
    build:
      context: ./src/services/inventory-service
      dockerfile: Dockerfile
    container_name: inventory_service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__InventoryConnection=Host=postgres;Database=inventory_service;Username=postgres;Password=dev_password
      - ConnectionStrings__Redis=redis:6379
      - Jwt__Authority=http://keycloak:8080/realms/bcommerce
    ports:
      - "5003:8080"
    depends_on:
      - postgres
      - redis
      - keycloak
    networks:
      - ecommerce-network

  # Order Service
  order-service:
    build:
      context: ./src/services/order-service
      dockerfile: Dockerfile
    container_name: order_service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__OrderConnection=Host=postgres;Database=order_service;Username=postgres;Password=dev_password
      - ConnectionStrings__Redis=redis:6379
      - Jwt__Authority=http://keycloak:8080/realms/bcommerce
    ports:
      - "5004:8080"
    depends_on:
      - postgres
      - redis
      - keycloak
    networks:
      - ecommerce-network

  # Payment Service
  payment-service:
    build:
      context: ./src/services/payment-service
      dockerfile: Dockerfile
    container_name: payment_service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__PaymentConnection=Host=postgres;Database=payment_service;Username=postgres;Password=dev_password
      - Jwt__Authority=http://keycloak:8080/realms/bcommerce
    ports:
      - "5005:8080"
    depends_on:
      - postgres
      - keycloak
    networks:
      - ecommerce-network

  # Notification Service
  notification-service:
    build:
      context: ./src/services/notification-service
      dockerfile: Dockerfile
    container_name: notification_service
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__NotificationConnection=Host=postgres;Database=notification_service;Username=postgres;Password=dev_password
      - Jwt__Authority=http://keycloak:8080/realms/bcommerce
    ports:
      - "5006:8080"
    depends_on:
      - postgres
      - keycloak
    networks:
      - ecommerce-network

  # API Gateway
  api-gateway:
    build:
      context: ./src/services/api-gateway
      dockerfile: Dockerfile
    container_name: api_gateway
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Jwt__Authority=http://keycloak:8080/realms/bcommerce
    ports:
      - "5000:8080"
    depends_on:
      - identity-service
      - catalog-service
      - inventory-service
      - order-service
      - payment-service
      - notification-service
    networks:
      - ecommerce-network

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:

networks:
  ecommerce-network:
    driver: bridge
*/

// =====================================================
// DOCKERFILE TEMPLATE FOR SERVICES
// =====================================================

/*
# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ServiceName.csproj", "."]
RUN dotnet restore "ServiceName.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src"
RUN dotnet build "ServiceName.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ServiceName.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ServiceName.dll"]
*/

// =====================================================
// MIGRATION COMMANDS
// =====================================================

/*
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Identity Service Migrations
cd src/services/identity-service
dotnet ef migrations add InitialCreate
dotnet ef database update

# Catalog Service Migrations
cd src/services/catalog-service
dotnet ef migrations add InitialCreate
dotnet ef database update

# Inventory Service Migrations
cd src/services/inventory-service
dotnet ef migrations add InitialCreate
dotnet ef database update

# Order Service Migrations
cd src/services/order-service
dotnet ef migrations add InitialCreate
dotnet ef database update

# Payment Service Migrations
cd src/services/payment-service
dotnet ef migrations add InitialCreate
dotnet ef database update

# Notification Service Migrations
cd src/services/notification-service
dotnet ef migrations add InitialCreate
dotnet ef database update
*/

// =====================================================
// PROJECT STRUCTURE
// =====================================================

/*
ecommerce-microservices/
├── src/
│   ├── services/
│   │   ├── identity-service/
│   │   │   ├── Program.cs
│   │   │   ├── IdentityService.csproj
│   │   │   ├── Dockerfile
│   │   │   ├── Infrastructure/
│   │   │   │   └── Data/
│   │   │   │       ├── IdentityDbContext.cs
│   │   │   │       ├── Entities/
│   │   │   │       └── Configurations/
│   │   │   └── appsettings.json
│   │   ├── catalog-service/
│   │   │   ├── Program.cs
│   │   │   ├── CatalogService.csproj
│   │   │   ├── Dockerfile
│   │   │   ├── Infrastructure/
│   │   │   │   └── Data/
│   │   │   │       ├── CatalogDbContext.cs
│   │   │   │       ├── Entities/
│   │   │   │       └── Configurations/
│   │   │   └── appsettings.json
│   │   ├── inventory-service/
│   │   ├── order-service/
│   │   ├── payment-service/
│   │   ├── notification-service/
│   │   └── api-gateway/
│   ├── shared/
│   │   ├── ECommerce.Shared/
│   │   │   ├── Events/
│   │   │   ├── Models/
│   │   │   └── Extensions/
│   │   └── ECommerce.Infrastructure/
│   │       ├── Auth/
│   │       ├── Messaging/
│   │       └── Database/
│   └── frontend/
│       └── angular-app/
├── infrastructure/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   ├── sql/
│   │   └── init-databases.sql
│   └── k8s/
├── tests/
│   ├── unit/
│   ├── integration/
│   └── e2e/
├── docs/
│   ├── architecture/
│   ├── api/
│   └── deployment/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── cd.yml
├── README.md
├── .gitignore
└── ecommerce-microservices.sln
*/

// =====================================================
// CSPROJ TEMPLATE FOR SERVICES
// =====================================================

/*
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="7.0.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="Polly" Version="8.2.0" />
    <PackageReference Include="AspNetCore.HealthChecks.Npgsql" Version="7.2.0" />
    <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="7.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../shared/ECommerce.Shared/ECommerce.Shared.csproj" />
    <ProjectReference Include="../../shared/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj" />
  </ItemGroup>

</Project>
*/

// =====================================================
// DEVELOPMENT COMMANDS
// =====================================================

/*
# Start all services with Docker Compose
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f [service-name]

# Rebuild specific service
docker-compose up -d --build [service-name]

# Run only infrastructure (DB, Redis, Keycloak)
docker-compose up -d postgres redis keycloak

# Access PostgreSQL
docker exec -it ecommerce_postgres psql -U postgres

# Access Redis CLI
docker exec -it ecommerce_redis redis-cli

# Check service health
curl http://localhost:5001/health  # Identity Service
curl http://localhost:5002/health  # Catalog Service
curl http://localhost:5003/health  # Inventory Service
curl http://localhost:5004/health  # Order Service
curl http://localhost:5005/health  # Payment Service
curl http://localhost:5006/health  # Notification Service

# API Gateway routes
curl http://localhost:5000/api/identity/profiles/[user-id]
curl http://localhost:5000/api/catalog/products
curl http://localhost:5000/api/orders
*/