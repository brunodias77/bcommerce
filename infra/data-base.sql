-- =====================================================
-- E-COMMERCE MICROSERVICES DATABASE SCHEMAS
-- =====================================================

-- =====================================================
-- 1. IDENTITY SERVICE DATABASE (PostgreSQL)
-- =====================================================
-- Nota: Keycloak já gerencia usuários, mas podemos ter dados específicos

CREATE DATABASE identity_service;
\c identity_service;

CREATE SCHEMA identity;

-- Perfis de usuário complementares ao Keycloak
CREATE TABLE identity.user_profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    keycloak_user_id UUID NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    birth_date DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_user_profiles_keycloak_id ON identity.user_profiles(keycloak_user_id);

-- =====================================================
-- 2. PRODUCT CATALOG SERVICE DATABASE (PostgreSQL)
-- =====================================================

CREATE DATABASE catalog_service;
\c catalog_service;

CREATE SCHEMA catalog;

-- Categorias de produtos
CREATE TABLE catalog.categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    parent_id UUID REFERENCES catalog.categories(id),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Produtos
CREATE TABLE catalog.products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(200) NOT NULL UNIQUE,
    description TEXT,
    short_description VARCHAR(500),
    sku VARCHAR(50) NOT NULL UNIQUE,
    price DECIMAL(10,2) NOT NULL,
    compare_price DECIMAL(10,2),
    category_id UUID NOT NULL REFERENCES catalog.categories(id),
    brand VARCHAR(100),
    weight DECIMAL(8,3),
    dimensions JSONB, -- {width, height, depth}
    is_active BOOLEAN DEFAULT true,
    is_featured BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Imagens dos produtos
CREATE TABLE catalog.product_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    url VARCHAR(500) NOT NULL,
    alt_text VARCHAR(200),
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Atributos de produtos (cor, tamanho, etc.)
CREATE TABLE catalog.product_attributes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    value VARCHAR(100) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Índices para performance
CREATE INDEX idx_products_category ON catalog.products(category_id);
CREATE INDEX idx_products_active ON catalog.products(is_active);
CREATE INDEX idx_products_featured ON catalog.products(is_featured);
CREATE INDEX idx_product_images_product ON catalog.product_images(product_id);
CREATE INDEX idx_product_attributes_product ON catalog.product_attributes(product_id);

-- =====================================================
-- 3. INVENTORY SERVICE DATABASE (PostgreSQL)
-- =====================================================

CREATE DATABASE inventory_service;
\c inventory_service;

CREATE SCHEMA inventory;

-- Estoque dos produtos
CREATE TABLE inventory.stock (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL UNIQUE,
    quantity INTEGER NOT NULL DEFAULT 0,
    reserved_quantity INTEGER NOT NULL DEFAULT 0,
    min_quantity INTEGER DEFAULT 0,
    max_quantity INTEGER,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Movimentações de estoque
CREATE TABLE inventory.stock_movements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL,
    movement_type VARCHAR(20) NOT NULL, -- 'IN', 'OUT', 'RESERVED', 'RELEASED'
    quantity INTEGER NOT NULL,
    reference_id UUID, -- ID do pedido, ajuste, etc.
    reference_type VARCHAR(50), -- 'ORDER', 'ADJUSTMENT', 'RETURN'
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_stock_product ON inventory.stock(product_id);
CREATE INDEX idx_stock_movements_product ON inventory.stock_movements(product_id);
CREATE INDEX idx_stock_movements_reference ON inventory.stock_movements(reference_id, reference_type);

-- =====================================================
-- 4. ORDER SERVICE DATABASE (PostgreSQL)
-- =====================================================

CREATE DATABASE order_service;
\c order_service;

CREATE SCHEMA orders;

-- Endereços de entrega
CREATE TABLE orders.addresses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    street VARCHAR(200) NOT NULL,
    number VARCHAR(20) NOT NULL,
    complement VARCHAR(100),
    neighborhood VARCHAR(100) NOT NULL,
    city VARCHAR(100) NOT NULL,
    state VARCHAR(50) NOT NULL,
    zip_code VARCHAR(10) NOT NULL,
    country VARCHAR(50) DEFAULT 'Brasil',
    is_default BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Pedidos
CREATE TABLE orders.orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number VARCHAR(20) NOT NULL UNIQUE,
    user_id UUID NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING, CONFIRMED, PROCESSING, SHIPPED, DELIVERED, CANCELLED
    subtotal DECIMAL(10,2) NOT NULL,
    shipping_cost DECIMAL(10,2) NOT NULL DEFAULT 0,
    tax_amount DECIMAL(10,2) NOT NULL DEFAULT 0,
    discount_amount DECIMAL(10,2) NOT NULL DEFAULT 0,
    total_amount DECIMAL(10,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'BRL',
    shipping_address_id UUID REFERENCES orders.addresses(id),
    payment_status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, PAID, FAILED, REFUNDED
    payment_method VARCHAR(50),
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Itens do pedido
CREATE TABLE orders.order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    product_id UUID NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    product_sku VARCHAR(50) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Histórico de status do pedido
CREATE TABLE orders.order_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL,
    notes TEXT,
    changed_by UUID,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_orders_user ON orders.orders(user_id);
CREATE INDEX idx_orders_status ON orders.orders(status);
CREATE INDEX idx_order_items_order ON orders.order_items(order_id);
CREATE INDEX idx_order_items_product ON orders.order_items(product_id);

-- =====================================================
-- 5. PAYMENT SERVICE DATABASE (PostgreSQL)
-- =====================================================

CREATE DATABASE payment_service;
\c payment_service;

CREATE SCHEMA payments;

-- Pagamentos
CREATE TABLE payments.payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'BRL',
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING, PROCESSING, SUCCESS, FAILED, CANCELLED, REFUNDED
    payment_method VARCHAR(50) NOT NULL, -- CREDIT_CARD, DEBIT_CARD, PIX, BOLETO
    provider VARCHAR(50) NOT NULL, -- STRIPE, MERCADOPAGO, PAGSEGURO
    provider_payment_id VARCHAR(100),
    provider_response JSONB,
    failure_reason TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Webhooks de pagamento
CREATE TABLE payments.payment_webhooks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID REFERENCES payments.payments(id),
    provider VARCHAR(50) NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    event_data JSONB NOT NULL,
    processed BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_payments_order ON payments.payments(order_id);
CREATE INDEX idx_payments_status ON payments.payments(status);
CREATE INDEX idx_payment_webhooks_payment ON payments.payment_webhooks(payment_id);
CREATE INDEX idx_payment_webhooks_processed ON payments.payment_webhooks(processed);

-- =====================================================
-- 6. NOTIFICATION SERVICE DATABASE (PostgreSQL)
-- =====================================================

CREATE DATABASE notification_service;
\c notification_service;

CREATE SCHEMA notifications;

-- Templates de notificação
CREATE TABLE notifications.templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    type VARCHAR(20) NOT NULL, -- EMAIL, SMS, PUSH
    subject VARCHAR(200),
    body TEXT NOT NULL,
    variables JSONB, -- Variáveis disponíveis no template
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Notificações enviadas
CREATE TABLE notifications.notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    template_id UUID REFERENCES notifications.templates(id),
    type VARCHAR(20) NOT NULL,
    recipient VARCHAR(200) NOT NULL, -- email, phone, device_token
    subject VARCHAR(200),
    body TEXT NOT NULL,
    status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, SENT, FAILED, DELIVERED
    provider VARCHAR(50), -- SENDGRID, TWILIO, FCM
    provider_response JSONB,
    scheduled_at TIMESTAMP WITH TIME ZONE,
    sent_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_notifications_user ON notifications.notifications(user_id);
CREATE INDEX idx_notifications_status ON notifications.notifications(status);
CREATE INDEX idx_notifications_scheduled ON notifications.notifications(scheduled_at);

-- =====================================================
-- REDIS SCHEMAS (Cart/Session)
-- =====================================================

-- Redis será usado para:
-- 1. Shopping Cart: "cart:{user_id}" -> JSON com itens do carrinho
-- 2. User Sessions: "session:{session_id}" -> dados da sessão
-- 3. Cache: "cache:{key}" -> dados em cache
-- 4. Rate Limiting: "ratelimit:{user_id}:{endpoint}" -> contadores

-- Estrutura JSON para carrinho no Redis:
/*
{
  "userId": "uuid",
  "items": [
    {
      "productId": "uuid",
      "productName": "Nome do Produto",
      "sku": "SKU123",
      "quantity": 2,
      "unitPrice": 99.90,
      "totalPrice": 199.80
    }
  ],
  "totalItems": 2,
  "totalAmount": 199.80,
  "lastUpdated": "2024-01-01T10:00:00Z",
  "expiresAt": "2024-01-08T10:00:00Z"
}
*/

-- =====================================================
-- SEQUENCES E FUNCTIONS ÚTEIS
-- =====================================================

-- Function para gerar números de pedido sequenciais
CREATE OR REPLACE FUNCTION orders.generate_order_number()
RETURNS VARCHAR(20) AS $$
DECLARE
    next_val BIGINT;
    order_num VARCHAR(20);
BEGIN
    SELECT COALESCE(MAX(CAST(SUBSTRING(order_number FROM 5) AS BIGINT)), 0) + 1
    INTO next_val
    FROM orders.orders
    WHERE order_number LIKE CONCAT(TO_CHAR(NOW(), 'YYYY'), '%');
    
    order_num := CONCAT(TO_CHAR(NOW(), 'YYYY'), LPAD(next_val::TEXT, 8, '0'));
    RETURN order_num;
END;
$$ LANGUAGE plpgsql;

-- Trigger para gerar número do pedido automaticamente
CREATE OR REPLACE FUNCTION orders.set_order_number()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.order_number IS NULL OR NEW.order_number = '' THEN
        NEW.order_number := orders.generate_order_number();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_set_order_number
    BEFORE INSERT ON orders.orders
    FOR EACH ROW
    EXECUTE FUNCTION orders.set_order_number();

-- =====================================================
-- DADOS DE EXEMPLO PARA DESENVOLVIMENTO
-- =====================================================

-- Categorias
INSERT INTO catalog.categories (name, slug, description) VALUES
('Eletrônicos', 'eletronicos', 'Produtos eletrônicos e gadgets'),
('Roupas', 'roupas', 'Vestuário e acessórios'),
('Casa e Jardim', 'casa-jardim', 'Produtos para casa e jardim'),
('Livros', 'livros', 'Livros e e-books'),
('Esportes', 'esportes', 'Artigos esportivos e fitness');

-- Produtos de exemplo
INSERT INTO catalog.products (name, slug, description, sku, price, category_id) 
SELECT 
    'Smartphone XYZ', 
    'smartphone-xyz', 
    'Smartphone com 128GB de armazenamento e câmera de 48MP',
    'PHONE001',
    899.90,
    id
FROM catalog.categories WHERE slug = 'eletronicos';

INSERT INTO catalog.products (name, slug, description, sku, price, category_id)
SELECT 
    'Camiseta Básica',
    'camiseta-basica',
    'Camiseta 100% algodão disponível em várias cores',
    'SHIRT001',
    39.90,
    id
FROM catalog.categories WHERE slug = 'roupas';

-- Templates de notificação
INSERT INTO notifications.templates (name, type, subject, body) VALUES
('order_confirmation', 'EMAIL', 'Pedido Confirmado - #{orderNumber}', 
 'Olá #{customerName}, seu pedido #{orderNumber} foi confirmado com sucesso!'),
('order_shipped', 'EMAIL', 'Pedido Enviado - #{orderNumber}',
 'Seu pedido #{orderNumber} foi enviado e chegará em breve!'),
('payment_failed', 'EMAIL', 'Problema no Pagamento - #{orderNumber}',
 'Houve um problema com o pagamento do seu pedido #{orderNumber}. Tente novamente.');

-- =====================================================
-- DOCKER COMPOSE PARA DESENVOLVIMENTO
-- =====================================================

-- Salve este conteúdo em docker-compose.yml:
/*
version: '3.8'

services:
  # PostgreSQL Principal
  postgres:
    image: postgres:15-alpine
    container_name: ecommerce_postgres
    environment:
      POSTGRES_PASSWORD: dev_password
      POSTGRES_DB: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./init-databases.sql:/docker-entrypoint-initdb.d/init-databases.sql

  # Redis para Cache e Carrinho
  redis:
    image: redis:7-alpine
    container_name: ecommerce_redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  # RabbitMQ para Mensageria
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: ecommerce_rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq

  # Keycloak para Autenticação
  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    container_name: ecommerce_keycloak
    environment:
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin123
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: postgres
      KC_DB_PASSWORD: dev_password
    ports:
      - "8080:8080"
    depends_on:
      - postgres
    command: start-dev

volumes:
  postgres_data:
  redis_data:
  rabbitmq_data:
*/

-- =====================================================
-- COMANDOS PARA INICIALIZAR
-- =====================================================

-- 1. Salvar este script como init-databases.sql
-- 2. Executar: docker-compose up -d
-- 3. Conectar no PostgreSQL e executar este script
-- 4. Configurar Keycloak em http://localhost:8080
-- 5. Acessar RabbitMQ Management em http://localhost:15672