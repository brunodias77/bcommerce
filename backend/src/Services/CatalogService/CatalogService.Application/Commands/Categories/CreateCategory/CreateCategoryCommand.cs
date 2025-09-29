using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Commands.Categories.CreateCategory;

public class CreateCategoryCommand : IRequest<Result<CreateCategoryCommandResponse>>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}