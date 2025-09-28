using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101006)]
public class Migration_20240101006_CreateProductVariantsTable : Migration
{
    public override void Up()
    {
        // Criar tabela product_variants
        Create.Table("product_variants")
            .WithColumn("variant_id").AsCustom("UUID").PrimaryKey()
            .WithColumn("product_id").AsCustom("UUID").NotNullable()
                .ForeignKey("fk_product_variants_product_id", "products", "product_id")
                .OnDelete(System.Data.Rule.Cascade)
            .WithColumn("color_id").AsCustom("UUID").Nullable()
                .ForeignKey("fk_product_variants_color_id", "product_colors", "color_id")
                .OnDelete(System.Data.Rule.SetNull)
            .WithColumn("size_id").AsCustom("UUID").Nullable()
                .ForeignKey("fk_product_variants_size_id", "product_sizes", "size_id")
                .OnDelete(System.Data.Rule.SetNull)
            .WithColumn("sku").AsString(50).NotNullable().Unique()
            .WithColumn("price").AsDecimal(10, 2).NotNullable()
            .WithColumn("stock_quantity").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Definir valor padrão para variant_id usando gen_random_uuid()
        Execute.Sql("ALTER TABLE product_variants ALTER COLUMN variant_id SET DEFAULT gen_random_uuid();");
        
        // Adicionar constraints de check
        Execute.Sql("ALTER TABLE product_variants ADD CONSTRAINT chk_product_variants_price CHECK (price >= 0);");
        Execute.Sql("ALTER TABLE product_variants ADD CONSTRAINT chk_product_variants_stock_quantity CHECK (stock_quantity >= 0);");
        
        // Constraint para garantir que pelo menos color_id ou size_id seja fornecido
        Execute.Sql("ALTER TABLE product_variants ADD CONSTRAINT chk_product_variants_color_or_size CHECK (color_id IS NOT NULL OR size_id IS NOT NULL);");
        
        // Criar índice único para combinação product_id + color_id + size_id
        Execute.Sql(@"
            CREATE UNIQUE INDEX idx_product_variants_unique_combination 
            ON product_variants (product_id, COALESCE(color_id, '00000000-0000-0000-0000-000000000000'::uuid), COALESCE(size_id, '00000000-0000-0000-0000-000000000000'::uuid)) 
            WHERE deleted_at IS NULL;
        ");
        
        // Criar índices para foreign keys
        Create.Index("idx_product_variants_product_id")
            .OnTable("product_variants")
            .OnColumn("product_id");
            
        Create.Index("idx_product_variants_color_id")
            .OnTable("product_variants")
            .OnColumn("color_id");
            
        Create.Index("idx_product_variants_size_id")
            .OnTable("product_variants")
            .OnColumn("size_id");
        
        // Criar índice condicional para is_active
        Execute.Sql("CREATE INDEX idx_product_variants_is_active ON product_variants (is_active) WHERE deleted_at IS NULL;");
        
        // Criar trigger para updated_at
        Execute.Sql(@"
            CREATE TRIGGER set_timestamp_product_variants 
            BEFORE UPDATE ON product_variants 
            FOR EACH ROW 
            EXECUTE FUNCTION trigger_set_timestamp();
        ");
    }
    
    public override void Down()
    {
        // Remover trigger
        Execute.Sql("DROP TRIGGER IF EXISTS set_timestamp_product_variants ON product_variants;");
        
        // Remover índices
        Delete.Index("idx_product_variants_is_active").OnTable("product_variants");
        Delete.Index("idx_product_variants_size_id").OnTable("product_variants");
        Delete.Index("idx_product_variants_color_id").OnTable("product_variants");
        Delete.Index("idx_product_variants_product_id").OnTable("product_variants");
        Delete.Index("idx_product_variants_unique_combination").OnTable("product_variants");
        
        // Remover tabela
        Delete.Table("product_variants");
    }
}