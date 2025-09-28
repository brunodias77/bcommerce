using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Domain.Entities;

// Imagens dos produtos
[Table("product_images")]
public class ProductImage
{
    [Key]
    [Column("product_image_id")]
    public Guid ProductImageId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("image_url")]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("alt_text")]
    public string? AltText { get; set; }

    [Column("is_cover")]
    public bool IsCover { get; set; } = false;

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
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}
