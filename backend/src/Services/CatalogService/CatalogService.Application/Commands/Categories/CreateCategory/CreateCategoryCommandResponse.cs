namespace CatalogService.Application.Commands.Categories.CreateCategory;

public record CreateCategoryCommandResponse
{
    public Guid CategoryId { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public int Version { get; init; }
}