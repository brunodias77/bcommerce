using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Domain.Entities;

// Variações dos produtos (cor e tamanho)
[Table("product_variants")]
public class ProductVariant
{
    [Key]
    [Column("product_variant_id")]
    public Guid ProductVariantId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("sku")]
    public string Sku { get; set; } = string.Empty;

    [Column("color_id")]
    public Guid? ColorId { get; set; }

    [Column("size_id")]
    public Guid? SizeId { get; set; }

    [Column("stock_quantity")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade em estoque deve ser maior ou igual a 0")]
    public int StockQuantity { get; set; } = 0;

    [Column("additional_price", TypeName = "decimal(10,2)")]
    public decimal AdditionalPrice { get; set; } = 0.00m;

    [MaxLength(255)]
    [Column("image_url")]
    public string? ImageUrl { get; set; }

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
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("ColorId")]
    public virtual ProductColor? Color { get; set; }

    [ForeignKey("SizeId")]
    public virtual ProductSize? Size { get; set; }
}