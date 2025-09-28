using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101001)]
public class Migration_20240101001_CreateCategoriesTable : Migration
{
    public override void Up()
    {
        // Criar extensão pgcrypto se não existir
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");
        
        // Criar função trigger_set_timestamp se não existir
        Execute.Sql(@"
            CREATE OR REPLACE FUNCTION trigger_set_timestamp()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.updated_at = CURRENT_TIMESTAMP;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
        ");
        
        // Criar tabela categories
        Create.Table("categories")
            .WithColumn("category_id").AsCustom("UUID").PrimaryKey()
            .WithColumn("name").AsString(100).NotNullable().Unique()
            .WithColumn("slug").AsString(150).NotNullable().Unique()
            .WithColumn("description").AsCustom("TEXT").Nullable()
            .WithColumn("parent_category_id").AsCustom("UUID").Nullable()
                .ForeignKey("fk_categories_parent_category_id", "categories", "category_id")
                .OnDelete(System.Data.Rule.SetNull)
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Definir valor padrão para category_id usando gen_random_uuid()
        Execute.Sql("ALTER TABLE categories ALTER COLUMN category_id SET DEFAULT gen_random_uuid();");
        
        // Criar índices
        Create.Index("idx_categories_parent_category_id")
            .OnTable("categories")
            .OnColumn("parent_category_id");
            
        Execute.Sql("CREATE INDEX idx_categories_is_active ON categories (is_active) WHERE deleted_at IS NULL;");
        
        // Criar trigger para updated_at
        Execute.Sql(@"
            CREATE TRIGGER set_timestamp_categories 
            BEFORE UPDATE ON categories 
            FOR EACH ROW 
            EXECUTE FUNCTION trigger_set_timestamp();
        ");
    }
    
    public override void Down()
    {
        // Remover trigger
        Execute.Sql("DROP TRIGGER IF EXISTS set_timestamp_categories ON categories;");
        
        // Remover índices
        Delete.Index("idx_categories_is_active").OnTable("categories");
        Delete.Index("idx_categories_parent_category_id").OnTable("categories");
        
        // Remover tabela
        Delete.Table("categories");
    }
}