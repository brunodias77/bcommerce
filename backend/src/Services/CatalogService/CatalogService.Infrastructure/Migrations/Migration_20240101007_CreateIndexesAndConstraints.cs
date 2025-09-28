using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101007)]
public class Migration_20240101007_CreateIndexesAndConstraints : Migration
{
    public override void Up()
    {
        // Índices adicionais para performance em products
        Execute.Sql("CREATE INDEX idx_products_base_price ON products (base_price) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_products_sale_price ON products (sale_price) WHERE sale_price IS NOT NULL AND deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_products_stock_quantity ON products (stock_quantity) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_products_created_at ON products (created_at) WHERE deleted_at IS NULL;");
        
        // Índice composto para consultas de produtos ativos por categoria
        Execute.Sql("CREATE INDEX idx_products_category_active ON products (category_id, is_active) WHERE deleted_at IS NULL;");
        
        // Índice composto para consultas de produtos ativos por marca
        Execute.Sql("CREATE INDEX idx_products_brand_active ON products (brand_id, is_active) WHERE deleted_at IS NULL AND brand_id IS NOT NULL;");
        
        // Índice para consultas de produtos em promoção
        Execute.Sql(@"
            CREATE INDEX idx_products_on_sale 
            ON products (sale_price, sale_price_start_date, sale_price_end_date) 
            WHERE sale_price IS NOT NULL AND deleted_at IS NULL;
        ");
        
        // Índices adicionais para product_variants
        Execute.Sql("CREATE INDEX idx_product_variants_price ON product_variants (price) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_product_variants_stock ON product_variants (stock_quantity) WHERE deleted_at IS NULL;");
        
        // Índice composto para consultas de variantes ativas por produto
        Execute.Sql("CREATE INDEX idx_product_variants_product_active ON product_variants (product_id, is_active) WHERE deleted_at IS NULL;");
        
        // Índices para consultas por cor e tamanho específicos
        Execute.Sql("CREATE INDEX idx_product_variants_color_active ON product_variants (color_id, is_active) WHERE color_id IS NOT NULL AND deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_product_variants_size_active ON product_variants (size_id, is_active) WHERE size_id IS NOT NULL AND deleted_at IS NULL;");
        
        // Índices adicionais para product_images
        Execute.Sql("CREATE INDEX idx_product_images_primary ON product_images (product_id, is_primary) WHERE deleted_at IS NULL;");
        
        // Índices para soft delete em todas as tabelas (para consultas que excluem registros deletados)
        Execute.Sql("CREATE INDEX idx_categories_not_deleted ON categories (category_id) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_brands_not_deleted ON brands (brand_id) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_products_not_deleted ON products (product_id) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_product_images_not_deleted ON product_images (image_id) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_product_colors_not_deleted ON product_colors (color_id) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_product_sizes_not_deleted ON product_sizes (size_id) WHERE deleted_at IS NULL;");
        Execute.Sql("CREATE INDEX idx_product_variants_not_deleted ON product_variants (variant_id) WHERE deleted_at IS NULL;");
    }
    
    public override void Down()
    {
        // Remover índices de soft delete
        Execute.Sql("DROP INDEX IF EXISTS idx_product_variants_not_deleted;");
        Execute.Sql("DROP INDEX IF EXISTS idx_product_sizes_not_deleted;");
        Execute.Sql("DROP INDEX IF EXISTS idx_product_colors_not_deleted;");
        Execute.Sql("DROP INDEX IF EXISTS idx_product_images_not_deleted;");
        Execute.Sql("DROP INDEX IF EXISTS idx_products_not_deleted;");
        Execute.Sql("DROP INDEX IF EXISTS idx_brands_not_deleted;");
        Execute.Sql("DROP INDEX IF EXISTS idx_categories_not_deleted;");
        
        // Remover índices de product_images
        Execute.Sql("DROP INDEX IF EXISTS idx_product_images_primary;");
        
        // Remover índices de product_variants
        Execute.Sql("DROP INDEX IF EXISTS idx_product_variants_size_active;");
        Execute.Sql("DROP INDEX IF EXISTS idx_product_variants_color_active;");
        Execute.Sql("DROP INDEX IF EXISTS idx_product_variants_product_active;");
        Execute.Sql("DROP INDEX IF EXISTS idx_product_variants_stock;");
        Execute.Sql("DROP INDEX IF EXISTS idx_product_variants_price;");
        
        // Remover índices de products
        Execute.Sql("DROP INDEX IF EXISTS idx_products_on_sale;");
        Execute.Sql("DROP INDEX IF EXISTS idx_products_brand_active;");
        Execute.Sql("DROP INDEX IF EXISTS idx_products_category_active;");
        Execute.Sql("DROP INDEX IF EXISTS idx_products_created_at;");
        Execute.Sql("DROP INDEX IF EXISTS idx_products_stock_quantity;");
        Execute.Sql("DROP INDEX IF EXISTS idx_products_sale_price;");
        Execute.Sql("DROP INDEX IF EXISTS idx_products_base_price;");
    }
}