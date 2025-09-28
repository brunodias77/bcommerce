using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Domain.Entities;

// Produto principal
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        [Column("base_sku")]
        public string BaseSku { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [Column("slug")]
        public string Slug { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("category_id")]
        public Guid CategoryId { get; set; }

        [Column("brand_id")]
        public Guid? BrandId { get; set; }

        [Required]
        [Column("base_price", TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O preço base deve ser maior ou igual a 0")]
        public decimal BasePrice { get; set; }

        [Column("sale_price", TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "O preço promocional deve ser maior ou igual a 0")]
        public decimal? SalePrice { get; set; }

        [Column("sale_price_start_date")]
        public DateTimeOffset? SalePriceStartDate { get; set; }

        [Column("sale_price_end_date")]
        public DateTimeOffset? SalePriceEndDate { get; set; }

        [Column("stock_quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade em estoque deve ser maior ou igual a 0")]
        public int StockQuantity { get; set; } = 0;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("weight_kg", TypeName = "decimal(6,3)")]
        [Range(0.001, double.MaxValue, ErrorMessage = "O peso deve ser maior que 0")]
        public decimal? WeightKg { get; set; }

        [Column("height_cm")]
        [Range(1, int.MaxValue, ErrorMessage = "A altura deve ser maior que 0")]
        public int? HeightCm { get; set; }

        [Column("width_cm")]
        [Range(1, int.MaxValue, ErrorMessage = "A largura deve ser maior que 0")]
        public int? WidthCm { get; set; }

        [Column("depth_cm")]
        [Range(1, int.MaxValue, ErrorMessage = "A profundidade deve ser maior que 0")]
        public int? DepthCm { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        [Column("version")]
        public int Version { get; set; } = 1;

        [Column("search_vector", TypeName = "tsvector")]
        public string? SearchVector { get; set; }

        // Navigation Properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }