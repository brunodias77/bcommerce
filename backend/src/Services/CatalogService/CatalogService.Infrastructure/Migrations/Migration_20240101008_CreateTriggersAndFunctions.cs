using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20240101008)]
public class Migration_20240101008_CreateTriggersAndFunctions : Migration
{
    public override void Up()
    {
        // Criar função para atualizar search_vector
        Execute.Sql(@"
            CREATE OR REPLACE FUNCTION trigger_update_products_search_vector()
            RETURNS TRIGGER AS $$
            BEGIN
                -- Atualizar o search_vector com base no nome e descrição do produto
                NEW.search_vector := 
                    setweight(to_tsvector('portuguese', COALESCE(NEW.name, '')), 'A') ||
                    setweight(to_tsvector('portuguese', COALESCE(NEW.description, '')), 'B');
                
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
        ");
        
        // Criar trigger para atualizar search_vector automaticamente
        Execute.Sql(@"
            CREATE TRIGGER trigger_products_search_vector_update
            BEFORE INSERT OR UPDATE OF name, description ON products
            FOR EACH ROW
            EXECUTE FUNCTION trigger_update_products_search_vector();
        ");
        
        // Atualizar search_vector para produtos existentes (se houver)
        Execute.Sql(@"
            UPDATE products 
            SET search_vector = 
                setweight(to_tsvector('portuguese', COALESCE(name, '')), 'A') ||
                setweight(to_tsvector('portuguese', COALESCE(description, '')), 'B')
            WHERE search_vector IS NULL;
        ");
    }
    
    public override void Down()
    {
        // Remover trigger
        Execute.Sql("DROP TRIGGER IF EXISTS trigger_products_search_vector_update ON products;");
        
        // Remover função
        Execute.Sql("DROP FUNCTION IF EXISTS trigger_update_products_search_vector();");
        
        // Limpar search_vector (opcional)
        Execute.Sql("UPDATE products SET search_vector = NULL;");
    }
}