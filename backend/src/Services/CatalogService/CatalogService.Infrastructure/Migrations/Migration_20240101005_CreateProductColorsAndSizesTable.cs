using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101005)]
public class Migration_20240101005_CreateProductColorsAndSizesTable : Migration
{
    public override void Up()
    {
        // Criar tabela product_colors
        Create.Table("product_colors")
            .WithColumn("color_id").AsCustom("UUID").PrimaryKey()
            .WithColumn("name").AsString(50).NotNullable().Unique()
            .WithColumn("hex_code").AsString(7).NotNullable().Unique()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Definir valor padrão para color_id usando gen_random_uuid()
        Execute.Sql("ALTER TABLE product_colors ALTER COLUMN color_id SET DEFAULT gen_random_uuid();");
        
        // Adicionar constraint de check para hex_code
        Execute.Sql("ALTER TABLE product_colors ADD CONSTRAINT chk_product_colors_hex_code CHECK (hex_code ~ '^#[0-9A-Fa-f]{6}$');");
        
        // Criar índice condicional para is_active
        Execute.Sql("CREATE INDEX idx_product_colors_is_active ON product_colors (is_active) WHERE deleted_at IS NULL;");
        
        // Criar trigger para updated_at
        Execute.Sql(@"
            CREATE TRIGGER set_timestamp_product_colors 
            BEFORE UPDATE ON product_colors 
            FOR EACH ROW 
            EXECUTE FUNCTION trigger_set_timestamp();
        ");
        
        // Criar tabela product_sizes
        Create.Table("product_sizes")
            .WithColumn("size_id").AsCustom("UUID").PrimaryKey()
            .WithColumn("name").AsString(20).NotNullable().Unique()
            .WithColumn("size_code").AsString(10).NotNullable().Unique()
            .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Definir valor padrão para size_id usando gen_random_uuid()
        Execute.Sql("ALTER TABLE product_sizes ALTER COLUMN size_id SET DEFAULT gen_random_uuid();");
        
        // Adicionar constraint de check para sort_order
        Execute.Sql("ALTER TABLE product_sizes ADD CONSTRAINT chk_product_sizes_sort_order CHECK (sort_order >= 0);");
        
        // Criar índice para sort_order
        Create.Index("idx_product_sizes_sort_order")
            .OnTable("product_sizes")
            .OnColumn("sort_order");
        
        // Criar índice condicional para is_active
        Execute.Sql("CREATE INDEX idx_product_sizes_is_active ON product_sizes (is_active) WHERE deleted_at IS NULL;");
        
        // Criar trigger para updated_at
        Execute.Sql(@"
            CREATE TRIGGER set_timestamp_product_sizes 
            BEFORE UPDATE ON product_sizes 
            FOR EACH ROW 
            EXECUTE FUNCTION trigger_set_timestamp();
        ");
    }
    
    public override void Down()
    {
        // Remover triggers
        Execute.Sql("DROP TRIGGER IF EXISTS set_timestamp_product_sizes ON product_sizes;");
        Execute.Sql("DROP TRIGGER IF EXISTS set_timestamp_product_colors ON product_colors;");
        
        // Remover índices
        Delete.Index("idx_product_sizes_is_active").OnTable("product_sizes");
        Delete.Index("idx_product_sizes_sort_order").OnTable("product_sizes");
        Delete.Index("idx_product_colors_is_active").OnTable("product_colors");
        
        // Remover tabelas
        Delete.Table("product_sizes");
        Delete.Table("product_colors");
    }
}