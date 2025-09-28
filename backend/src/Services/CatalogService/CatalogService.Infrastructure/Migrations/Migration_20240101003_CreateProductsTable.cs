using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101003)]
public class Migration_20240101003_CreateProductsTable : Migration
{
    public override void Up()
    {
        // Criar tabela products
        Create.Table("products")
            .WithColumn("product_id").AsCustom("UUID").PrimaryKey()
            .WithColumn("base_sku").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(150).NotNullable()
            .WithColumn("slug").AsString(200).NotNullable().Unique()
            .WithColumn("description").AsCustom("TEXT").Nullable()
            .WithColumn("category_id").AsCustom("UUID").NotNullable()
                .ForeignKey("fk_products_category_id", "categories", "category_id")
                .OnDelete(System.Data.Rule.None) // RESTRICT
            .WithColumn("brand_id").AsCustom("UUID").Nullable()
                .ForeignKey("fk_products_brand_id", "brands", "brand_id")
                .OnDelete(System.Data.Rule.SetNull)
            .WithColumn("base_price").AsDecimal(10, 2).NotNullable()
            .WithColumn("sale_price").AsDecimal(10, 2).Nullable()
            .WithColumn("sale_price_start_date").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("sale_price_end_date").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("stock_quantity").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("weight_kg").AsDecimal(6, 3).Nullable()
            .WithColumn("height_cm").AsInt32().Nullable()
            .WithColumn("width_cm").AsInt32().Nullable()
            .WithColumn("depth_cm").AsInt32().Nullable()
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1)
            .WithColumn("search_vector").AsCustom("TSVECTOR").Nullable();
        
        // Definir valor padrão para product_id usando gen_random_uuid()
        Execute.Sql("ALTER TABLE products ALTER COLUMN product_id SET DEFAULT gen_random_uuid();");
        
        // Adicionar constraints de check
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_products_base_price CHECK (base_price >= 0);");
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_products_sale_price CHECK (sale_price IS NULL OR sale_price >= 0);");
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_products_stock_quantity CHECK (stock_quantity >= 0);");
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_products_weight_kg CHECK (weight_kg IS NULL OR weight_kg > 0);");
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_products_height_cm CHECK (height_cm IS NULL OR height_cm > 0);");
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_products_width_cm CHECK (width_cm IS NULL OR width_cm > 0);");
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_products_depth_cm CHECK (depth_cm IS NULL OR depth_cm > 0);");
        Execute.Sql("ALTER TABLE products ADD CONSTRAINT chk_sale_price CHECK (sale_price IS NULL OR sale_price < base_price);");
        Execute.Sql(@"
            ALTER TABLE products ADD CONSTRAINT chk_sale_dates 
            CHECK ((sale_price IS NULL) OR (sale_price IS NOT NULL AND sale_price_start_date IS NOT NULL AND sale_price_end_date IS NOT NULL));
        ");
        
        // Criar índices básicos
        Create.Index("idx_products_category_id")
            .OnTable("products")
            .OnColumn("category_id");
            
        Create.Index("idx_products_brand_id")
            .OnTable("products")
            .OnColumn("brand_id");
        
        // Criar índices condicionais
        Execute.Sql("CREATE INDEX idx_products_is_active ON products (is_active) WHERE deleted_at IS NULL;");
        
        // Criar índice GIN para search_vector
        Execute.Sql("CREATE INDEX idx_products_search_vector ON products USING GIN (search_vector);");
        
        // Criar trigger para updated_at
        Execute.Sql(@"
            CREATE TRIGGER set_timestamp_products 
            BEFORE UPDATE ON products 
            FOR EACH ROW 
            EXECUTE FUNCTION trigger_set_timestamp();
        ");
    }
    
    public override void Down()
    {
        // Remover trigger
        Execute.Sql("DROP TRIGGER IF EXISTS set_timestamp_products ON products;");
        
        // Remover índices
        Delete.Index("idx_products_search_vector").OnTable("products");
        Delete.Index("idx_products_is_active").OnTable("products");
        Delete.Index("idx_products_brand_id").OnTable("products");
        Delete.Index("idx_products_category_id").OnTable("products");
        
        // Remover tabela (constraints são removidas automaticamente)
        Delete.Table("products");
    }
}