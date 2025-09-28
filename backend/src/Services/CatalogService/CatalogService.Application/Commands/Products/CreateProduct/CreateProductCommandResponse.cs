namespace CatalogService.Application.Commands.Products.CreateProduct;

public record CreateProductCommandResponse
{
    public Guid ProductId { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? BaseSku { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}