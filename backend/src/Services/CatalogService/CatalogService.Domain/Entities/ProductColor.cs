using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Domain.Entities;

// Cores dos produtos
[Table("product_colors")]
public class ProductColor
{
    [Key]
    [Column("color_id")]
    public Guid ColorId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(7, MinimumLength = 7, ErrorMessage = "O código hex deve ter exatamente 7 caracteres")]
    [RegularExpression(@"^#[0-9a-fA-F]{6}$", ErrorMessage = "O código hex deve estar no formato #RRGGBB")]
    [Column("hex_code")]
    public string? HexCode { get; set; }

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
