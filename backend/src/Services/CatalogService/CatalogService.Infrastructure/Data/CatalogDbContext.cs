using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }
    
    // DbSets das entidades
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductColor> ProductColors { get; set; }
    public DbSet<ProductSize> ProductSizes { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

            // Configurações de Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.ParentCategoryId);
                entity.HasIndex(e => e.IsActive)
                    .HasFilter("deleted_at IS NULL");

                // Auto-relacionamento de categorias
                entity.HasOne(d => d.ParentCategory)
                    .WithMany(p => p.SubCategories)
                    .HasForeignKey(d => d.ParentCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Filtro global para soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configurações de Brand
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.IsActive)
                    .HasFilter("deleted_at IS NULL");

                // Filtro global para soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configurações de Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.BaseSku).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.BrandId);
                entity.HasIndex(e => e.IsActive)
                    .HasFilter("deleted_at IS NULL");

                // Relacionamento com Category (obrigatório)
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Brand (opcional)
                entity.HasOne(d => d.Brand)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.BrandId)
                    .OnDelete(DeleteBehavior.SetNull);

                // TODO: Adicionar configuração SearchVector em migration futura
                // entity.Property(e => e.SearchVector)
                //     .HasColumnType("tsvector");

                // Constraints personalizadas
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_BasePrice", "base_price >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_SalePrice", "sale_price IS NULL OR sale_price >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_SalePrice_LessThan_BasePrice", "sale_price IS NULL OR sale_price < base_price"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_StockQuantity", "stock_quantity >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_Weight", "weight_kg IS NULL OR weight_kg > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_Height", "height_cm IS NULL OR height_cm > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_Width", "width_cm IS NULL OR width_cm > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_Depth", "depth_cm IS NULL OR depth_cm > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Products_SaleDates", "(sale_price IS NULL) OR (sale_price IS NOT NULL AND sale_price_start_date IS NOT NULL AND sale_price_end_date IS NOT NULL)"));

                // Filtro global para soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configurações de ProductImage
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasIndex(e => e.ProductId);

                // Índice único para garantir apenas uma imagem de capa por produto
                entity.HasIndex(e => e.ProductId)
                    .IsUnique()
                    .HasFilter("is_cover = TRUE AND deleted_at IS NULL");

                // Relacionamento com Product
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Filtro global para soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configurações de ProductColor
            modelBuilder.Entity<ProductColor>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.HexCode).IsUnique();

                // Filtro global para soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configurações de ProductSize
            modelBuilder.Entity<ProductSize>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.SizeCode).IsUnique();

                // Filtro global para soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configurações de ProductVariant
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.ColorId);
                entity.HasIndex(e => e.SizeId);

                // Índice único composto para combinação produto + cor + tamanho
                entity.HasIndex(e => new { e.ProductId, e.ColorId, e.SizeId })
                    .IsUnique()
                    .HasDatabaseName("IX_ProductVariants_ProductId_ColorId_SizeId");

                // Relacionamento com Product
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relacionamento com Color
                entity.HasOne(d => d.Color)
                    .WithMany(p => p.ProductVariants)
                    .HasForeignKey(d => d.ColorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Size
                entity.HasOne(d => d.Size)
                    .WithMany(p => p.ProductVariants)
                    .HasForeignKey(d => d.SizeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Constraints personalizadas
                entity.ToTable(t => t.HasCheckConstraint("CK_ProductVariants_StockQuantity", "stock_quantity >= 0"));

                // Filtro global para soft delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configurações globais para PostgreSQL
            ConfigurePostgreSQLSpecific(modelBuilder);
    }
    
    private void ConfigurePostgreSQLSpecific(ModelBuilder modelBuilder)
    {
        // Configurações específicas do PostgreSQL

        // Configurar função de timestamp para updated_at
        modelBuilder.HasPostgresExtension("uuid-ossp");

        // TODO: Adicionar search vector para produtos (PostgreSQL Full Text Search) em migration futura
        // modelBuilder.Entity<Product>(entity =>
        // {
        //     entity.HasIndex(e => e.SearchVector)
        //         .HasMethod("GIN")
        //         .HasDatabaseName("IX_Products_SearchVector");
        // });
    }
    
    
    
    
    
    

}