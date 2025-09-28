using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101004)]
public class Migration_20240101004_CreateProductImagesTable : Migration
{
    public override void Up()
    {
        // Criar tabela product_images
        Create.Table("product_images")
            .WithColumn("image_id").AsCustom("UUID").PrimaryKey()
            .WithColumn("product_id").AsCustom("UUID").NotNullable()
                .ForeignKey("fk_product_images_product_id", "products", "product_id")
                .OnDelete(System.Data.Rule.Cascade)
            .WithColumn("image_url").AsString(500).NotNullable()
            .WithColumn("alt_text").AsString(255).Nullable()
            .WithColumn("is_primary").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Definir valor padrão para image_id usando gen_random_uuid()
        Execute.Sql("ALTER TABLE product_images ALTER COLUMN image_id SET DEFAULT gen_random_uuid();");
        
        // Criar índice para product_id
        Create.Index("idx_product_images_product_id")
            .OnTable("product_images")
            .OnColumn("product_id");
        
        // Criar índice único para garantir apenas uma imagem primária por produto
        Execute.Sql(@"
            CREATE UNIQUE INDEX idx_product_images_primary_unique 
            ON product_images (product_id) 
            WHERE is_primary = true AND deleted_at IS NULL;
        ");
        
        // Criar índice para sort_order
        Create.Index("idx_product_images_sort_order")
            .OnTable("product_images")
            .OnColumn("sort_order");
        
        // Adicionar constraint de check para sort_order
        Execute.Sql("ALTER TABLE product_images ADD CONSTRAINT chk_product_images_sort_order CHECK (sort_order >= 0);");
        
        // Criar trigger para updated_at
        Execute.Sql(@"
            CREATE TRIGGER set_timestamp_product_images 
            BEFORE UPDATE ON product_images 
            FOR EACH ROW 
            EXECUTE FUNCTION trigger_set_timestamp();
        ");
    }
    
    public override void Down()
    {
        // Remover trigger
        Execute.Sql("DROP TRIGGER IF EXISTS set_timestamp_product_images ON product_images;");
        
        // Remover índices
        Delete.Index("idx_product_images_sort_order").OnTable("product_images");
        Delete.Index("idx_product_images_primary_unique").OnTable("product_images");
        Delete.Index("idx_product_images_product_id").OnTable("product_images");
        
        // Remover tabela
        Delete.Table("product_images");
    }
}