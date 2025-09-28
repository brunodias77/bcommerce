-- =====================================================================
-- MICROSSERVIÇO 2: CATALOG SERVICE
-- Responsabilidades: Produtos, categorias, marcas, inventário
-- =====================================================================

-- Tabela de categorias
CREATE TABLE categories (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(150) NOT NULL UNIQUE,
    description TEXT,
    parent_category_id UUID REFERENCES categories(category_id) ON DELETE SET NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX idx_categories_parent_category_id ON categories (parent_category_id);
CREATE INDEX idx_categories_is_active ON categories (is_active) WHERE deleted_at IS NULL;
CREATE TRIGGER set_timestamp_categories BEFORE UPDATE ON categories FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

-- Tabela de marcas
CREATE TABLE brands (
    brand_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(150) NOT NULL UNIQUE,
    description TEXT,
    logo_url VARCHAR(255),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX idx_brands_is_active ON brands (is_active) WHERE deleted_at IS NULL;
CREATE TRIGGER set_timestamp_brands BEFORE UPDATE ON brands FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

-- Função para atualizar vetor de busca
CREATE OR REPLACE FUNCTION trigger_update_products_search_vector()
RETURNS TRIGGER AS $$
BEGIN
    NEW.search_vector =
        setweight(to_tsvector('portuguese', COALESCE(NEW.name, '')), 'A') ||
        setweight(to_tsvector('portuguese', COALESCE(NEW.base_sku, '')), 'A') ||
        setweight(to_tsvector('portuguese', COALESCE(NEW.description, '')), 'B');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Tabela de produtos
CREATE TABLE products (
    product_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    base_sku VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    slug VARCHAR(200) NOT NULL UNIQUE,
    description TEXT,
    category_id UUID NOT NULL REFERENCES categories(category_id) ON DELETE RESTRICT,
    brand_id UUID REFERENCES brands(brand_id) ON DELETE SET NULL,
    base_price NUMERIC(10,2) NOT NULL CHECK (base_price >= 0),
    sale_price NUMERIC(10,2) CHECK (sale_price IS NULL OR sale_price >= 0),
    sale_price_start_date TIMESTAMPTZ,
    sale_price_end_date TIMESTAMPTZ,
    stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    weight_kg NUMERIC(6,3) CHECK (weight_kg IS NULL OR weight_kg > 0),
    height_cm INTEGER CHECK (height_cm IS NULL OR height_cm > 0),
    width_cm INTEGER CHECK (width_cm IS NULL OR width_cm > 0),
    depth_cm INTEGER CHECK (depth_cm IS NULL OR depth_cm > 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1,
    search_vector TSVECTOR,
    CONSTRAINT chk_sale_price CHECK (sale_price IS NULL OR sale_price < base_price),
    CONSTRAINT chk_sale_dates CHECK ((sale_price IS NULL) OR (sale_price IS NOT NULL AND sale_price_start_date IS NOT NULL AND sale_price_end_date IS NOT NULL))
);
CREATE INDEX idx_products_category_id ON products (category_id);
CREATE INDEX idx_products_brand_id ON products (brand_id);
CREATE INDEX idx_products_is_active ON products (is_active) WHERE deleted_at IS NULL;
CREATE INDEX idx_products_search_vector ON products USING GIN (search_vector);
CREATE TRIGGER set_timestamp_products BEFORE UPDATE ON products FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();
CREATE TRIGGER update_search_vector_trigger BEFORE INSERT OR UPDATE ON products FOR EACH ROW EXECUTE FUNCTION trigger_update_products_search_vector();

-- Imagens dos produtos
CREATE TABLE product_images (
    product_image_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    image_url VARCHAR(255) NOT NULL,
    alt_text VARCHAR(255),
    is_cover BOOLEAN NOT NULL DEFAULT FALSE,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX idx_product_images_product_id ON product_images (product_id);
CREATE UNIQUE INDEX uq_product_images_cover_per_product ON product_images (product_id) WHERE is_cover = TRUE AND deleted_at IS NULL;
CREATE TRIGGER set_timestamp_product_images BEFORE UPDATE ON product_images FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

-- Cores e tamanhos para variações
CREATE TABLE product_colors (
    color_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    hex_code CHAR(7) UNIQUE CHECK (hex_code ~ '^#[0-9a-fA-F]{6}$'),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1
);
CREATE TRIGGER set_timestamp_product_colors BEFORE UPDATE ON product_colors FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

CREATE TABLE product_sizes (
    size_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE,
    size_code VARCHAR(20) UNIQUE,
    sort_order INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1
);
CREATE TRIGGER set_timestamp_product_sizes BEFORE UPDATE ON product_sizes FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

-- Variações de produtos
CREATE TABLE product_variants (
    product_variant_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES products(product_id) ON DELETE CASCADE,
    sku VARCHAR(50) NOT NULL UNIQUE,
    color_id UUID REFERENCES product_colors(color_id) ON DELETE RESTRICT,
    size_id UUID REFERENCES product_sizes(size_id) ON DELETE RESTRICT,
    stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    additional_price NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    image_url VARCHAR(255),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ,
    version INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT uq_product_variant_attributes UNIQUE (product_id, color_id, size_id)
);
CREATE INDEX idx_product_variants_product_id ON product_variants (product_id);
CREATE INDEX idx_product_variants_color_id ON product_variants (color_id);
CREATE INDEX idx_product_variants_size_id ON product_variants (size_id);
CREATE TRIGGER set_timestamp_product_variants BEFORE UPDATE ON product_variants FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();