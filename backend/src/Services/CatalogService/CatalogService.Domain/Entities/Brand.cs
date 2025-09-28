using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Domain.Entities;

// Marca de produtos
[Table("brands")]
public class Brand
{
    [Key]
    [Column("brand_id")]
    public Guid BrandId { get; set; } = Guid.NewGuid();

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

    [MaxLength(255)]
    [Column("logo_url")]
    public string? LogoUrl { get; set; }

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
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}