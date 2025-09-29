namespace CatalogService.Application.Commands.Products.UpdateProductCommand;

public record UpdateProductCommandResponse
{
    public Guid ProductId { get; init; }
    public string? BaseSku { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public int Version { get; init; }
}