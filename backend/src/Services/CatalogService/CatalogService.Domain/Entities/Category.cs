using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Domain.Entities;

// Categoria de produtos
[Table("categories")]
public class Category
{
    [Key]
    [Column("category_id")]
    public Guid CategoryId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("parent_category_id")]
    public Guid? ParentCategoryId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [Column("version")]
    public int Version { get; set; } = 1;

    // Navigation Properties
    [ForeignKey("ParentCategoryId")]
    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}