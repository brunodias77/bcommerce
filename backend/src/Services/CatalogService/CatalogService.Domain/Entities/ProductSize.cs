using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Domain.Entities;

// Tamanhos dos produtos
[Table("product_sizes")]
public class ProductSize
{
    [Key]
    [Column("size_id")]
    public Guid SizeId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("size_code")]
    public string? SizeCode { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [Column("version")]
    public int Version { get; set; } = 1;

    // Navigation Properties
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
