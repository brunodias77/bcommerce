using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101002)]
public class Migration_20240101002_CreateBrandsTable : Migration
{
    public override void Up()
    {
        // Criar tabela brands
        Create.Table("brands")
            .WithColumn("brand_id").AsCustom("UUID").PrimaryKey()
            .WithColumn("name").AsString(100).NotNullable().Unique()
            .WithColumn("slug").AsString(150).NotNullable().Unique()
            .WithColumn("description").AsCustom("TEXT").Nullable()
            .WithColumn("logo_url").AsString(255).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsCustom("TIMESTAMPTZ").NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("deleted_at").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(1);
        
        // Definir valor padrão para brand_id usando gen_random_uuid()
        Execute.Sql("ALTER TABLE brands ALTER COLUMN brand_id SET DEFAULT gen_random_uuid();");
        
        // Criar índice condicional para is_active
        Execute.Sql("CREATE INDEX idx_brands_is_active ON brands (is_active) WHERE deleted_at IS NULL;");
        
        // Criar trigger para updated_at
        Execute.Sql(@"
            CREATE TRIGGER set_timestamp_brands 
            BEFORE UPDATE ON brands 
            FOR EACH ROW 
            EXECUTE FUNCTION trigger_set_timestamp();
        ");
    }
    
    public override void Down()
    {
        // Remover trigger
        Execute.Sql("DROP TRIGGER IF EXISTS set_timestamp_brands ON brands;");
        
        // Remover índice
        Delete.Index("idx_brands_is_active").OnTable("brands");
        
        // Remover tabela
        Delete.Table("brands");
    }
}