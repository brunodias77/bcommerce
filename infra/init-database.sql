-- scripts/init-databases.sql
-- =====================================================
-- E-COMMERCE MICROSERVICES DATABASE INITIALIZATION
-- =====================================================

-- Create Keycloak Database
CREATE DATABASE keycloak;
GRANT ALL PRIVILEGES ON DATABASE keycloak TO postgres;

-- =====================================================
-- 1. IDENTITY SERVICE DATABASE
-- =====================================================
CREATE DATABASE identity_service;
\c identity_service;

CREATE SCHEMA IF NOT EXISTS identity;

-- User Profiles Table
CREATE TABLE identity.user_profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    keycloak_user_id UUID NOT NULL UNIQUE,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    birth_date DATE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE NULL
);

CREATE INDEX idx_user_profiles_keycloak_id ON identity.user_profiles(keycloak_user_id);
CREATE INDEX idx_user_profiles_deleted_at ON identity.user_profiles(deleted_at) WHERE deleted_at IS NULL;

-- Outbox Events Table for Domain Events
CREATE TABLE identity.outbox_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    version INTEGER DEFAULT 1
);

CREATE INDEX idx_outbox_events_processed ON identity.outbox_events(processed_at) WHERE processed_at IS NULL;
CREATE INDEX idx_outbox_events_aggregate ON identity.outbox_events(aggregate_id, aggregate_type);

-- =====================================================
-- 2. CATALOG SERVICE DATABASE
-- =====================================================
\c postgres;
CREATE DATABASE catalog_service;
\c catalog_service;

CREATE SCHEMA IF NOT EXISTS catalog;

-- Categories Table
CREATE TABLE catalog.categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    parent_id UUID REFERENCES catalog.categories(id),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE NULL
);

-- Products Table
CREATE TABLE catalog.products (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(200) NOT NULL UNIQUE,
    description TEXT,
    short_description VARCHAR(500),
    sku VARCHAR(50) NOT NULL UNIQUE,
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    compare_price DECIMAL(10,2) CHECK (compare_price >= 0),
    currency VARCHAR(3) DEFAULT 'BRL',
    category_id UUID NOT NULL REFERENCES catalog.categories(id),
    brand VARCHAR(100),
    weight DECIMAL(8,3) CHECK (weight >= 0),
    dimensions JSONB,
    is_active BOOLEAN DEFAULT true,
    is_featured BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE NULL
);

-- Product Images Table
CREATE TABLE catalog.product_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    url VARCHAR(500) NOT NULL,
    alt_text VARCHAR(200),
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Product Attributes Table
CREATE TABLE catalog.product_attributes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES catalog.products(id) ON DELETE CASCADE,
    name VARCHAR(50) NOT NULL,
    value VARCHAR(100) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Outbox Events Table
CREATE TABLE catalog.outbox_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    version INTEGER DEFAULT 1
);

-- Indexes for performance
CREATE INDEX idx_categories_parent ON catalog.categories(parent_id);
CREATE INDEX idx_categories_active ON catalog.categories(is_active) WHERE is_active = true;
CREATE INDEX idx_categories_slug ON catalog.categories(slug);

CREATE INDEX idx_products_category ON catalog.products(category_id);
CREATE INDEX idx_products_active ON catalog.products(is_active) WHERE is_active = true;
CREATE INDEX idx_products_featured ON catalog.products(is_featured) WHERE is_featured = true;
CREATE INDEX idx_products_sku ON catalog.products(sku);
CREATE INDEX idx_products_slug ON catalog.products(slug);
CREATE INDEX idx_products_price ON catalog.products(price);
CREATE INDEX idx_products_search ON catalog.products USING gin(to_tsvector('portuguese', name || ' ' || coalesce(description, '') || ' ' || coalesce(brand, '')));

CREATE INDEX idx_product_images_product ON catalog.product_images(product_id);
CREATE INDEX idx_product_images_sort ON catalog.product_images(product_id, sort_order);

CREATE INDEX idx_product_attributes_product ON catalog.product_attributes(product_id);
CREATE INDEX idx_product_attributes_name_value ON catalog.product_attributes(name, value);

CREATE INDEX idx_catalog_outbox_processed ON catalog.outbox_events(processed_at) WHERE processed_at IS NULL;

-- =====================================================
-- 3. INVENTORY SERVICE DATABASE
-- =====================================================
\c postgres;
CREATE DATABASE inventory_service;
\c inventory_service;

CREATE SCHEMA IF NOT EXISTS inventory;

-- Stock Table
CREATE TABLE inventory.stock (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL UNIQUE,
    available_quantity INTEGER NOT NULL DEFAULT 0 CHECK (available_quantity >= 0),
    reserved_quantity INTEGER NOT NULL DEFAULT 0 CHECK (reserved_quantity >= 0),
    min_quantity INTEGER DEFAULT 0 CHECK (min_quantity >= 0),
    max_quantity INTEGER CHECK (max_quantity IS NULL OR max_quantity >= min_quantity),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    version INTEGER DEFAULT 1 -- For optimistic locking
);

-- Stock Movements Table
CREATE TABLE inventory.stock_movements (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL,
    movement_type VARCHAR(20) NOT NULL CHECK (movement_type IN ('IN', 'OUT', 'RESERVED', 'RELEASED')),
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    reference_id UUID,
    reference_type VARCHAR(50),
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Outbox Events Table
CREATE TABLE inventory.outbox_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    version INTEGER DEFAULT 1
);

-- Indexes
CREATE INDEX idx_stock_product ON inventory.stock(product_id);
CREATE INDEX idx_stock_low_stock ON inventory.stock(available_quantity, min_quantity) WHERE available_quantity <= min_quantity;

CREATE INDEX idx_stock_movements_product ON inventory.stock_movements(product_id);
CREATE INDEX idx_stock_movements_reference ON inventory.stock_movements(reference_id, reference_type);
CREATE INDEX idx_stock_movements_type ON inventory.stock_movements(movement_type);
CREATE INDEX idx_stock_movements_date ON inventory.stock_movements(created_at);

CREATE INDEX idx_inventory_outbox_processed ON inventory.outbox_events(processed_at) WHERE processed_at IS NULL;

-- =====================================================
-- 4. ORDER SERVICE DATABASE
-- =====================================================
\c postgres;
CREATE DATABASE order_service;
\c order_service;

CREATE SCHEMA IF NOT EXISTS orders;

-- Customer Addresses Table
CREATE TABLE orders.customer_addresses (
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
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE NULL
);

-- Orders Table
CREATE TABLE orders.orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number VARCHAR(20) NOT NULL UNIQUE,
    user_id UUID NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK (status IN ('PENDING', 'CONFIRMED', 'PROCESSING', 'SHIPPED', 'DELIVERED', 'CANCELLED')),
    subtotal DECIMAL(10,2) NOT NULL CHECK (subtotal >= 0),
    shipping_cost DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (shipping_cost >= 0),
    tax_amount DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (tax_amount >= 0),
    discount_amount DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (discount_amount >= 0),
    total_amount DECIMAL(10,2) NOT NULL CHECK (total_amount >= 0),
    currency VARCHAR(3) DEFAULT 'BRL',
    shipping_address_id UUID REFERENCES orders.customer_addresses(id),
    payment_status VARCHAR(20) DEFAULT 'PENDING' CHECK (payment_status IN ('PENDING', 'PAID', 'FAILED', 'REFUNDED')),
    payment_method VARCHAR(50),
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    deleted_at TIMESTAMP WITH TIME ZONE NULL,
    version INTEGER DEFAULT 1
);

-- Order Items Table
CREATE TABLE orders.order_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    product_id UUID NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    product_sku VARCHAR(50) NOT NULL,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(10,2) NOT NULL CHECK (unit_price >= 0),
    total_price DECIMAL(10,2) NOT NULL CHECK (total_price >= 0),
    currency VARCHAR(3) DEFAULT 'BRL',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Order Status History Table
CREATE TABLE orders.order_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL REFERENCES orders.orders(id) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL,
    notes TEXT,
    changed_by UUID,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Outbox Events Table
CREATE TABLE orders.outbox_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    version INTEGER DEFAULT 1
);

-- Indexes
CREATE INDEX idx_addresses_user ON orders.customer_addresses(user_id);
CREATE INDEX idx_addresses_default ON orders.customer_addresses(user_id, is_default) WHERE is_default = true;

CREATE INDEX idx_orders_user ON orders.orders(user_id);
CREATE INDEX idx_orders_status ON orders.orders(status);
CREATE INDEX idx_orders_number ON orders.orders(order_number);
CREATE INDEX idx_orders_payment_status ON orders.orders(payment_status);
CREATE INDEX idx_orders_date ON orders.orders(created_at);

CREATE INDEX idx_order_items_order ON orders.order_items(order_id);
CREATE INDEX idx_order_items_product ON orders.order_items(product_id);

CREATE INDEX idx_order_status_history_order ON orders.order_status_history(order_id);
CREATE INDEX idx_order_status_history_date ON orders.order_status_history(created_at);

CREATE INDEX idx_orders_outbox_processed ON orders.outbox_events(processed_at) WHERE processed_at IS NULL;

-- Function to generate order number
CREATE OR REPLACE FUNCTION orders.generate_order_number()
RETURNS VARCHAR(20) AS $
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
$ LANGUAGE plpgsql;

-- Trigger to auto-generate order number
CREATE OR REPLACE FUNCTION orders.set_order_number()
RETURNS TRIGGER AS $
BEGIN
    IF NEW.order_number IS NULL OR NEW.order_number = '' THEN
        NEW.order_number := orders.generate_order_number();
    END IF;
    RETURN NEW;
END;
$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_set_order_number
    BEFORE INSERT ON orders.orders
    FOR EACH ROW
    EXECUTE FUNCTION orders.set_order_number();

-- =====================================================
-- 5. PAYMENT SERVICE DATABASE
-- =====================================================
\c postgres;
CREATE DATABASE payment_service;
\c payment_service;

CREATE SCHEMA IF NOT EXISTS payments;

-- Payments Table
CREATE TABLE payments.payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id UUID NOT NULL,
    amount DECIMAL(10,2) NOT NULL CHECK (amount > 0),
    currency VARCHAR(3) DEFAULT 'BRL',
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK (status IN ('PENDING', 'PROCESSING', 'SUCCESS', 'FAILED', 'CANCELLED', 'REFUNDED')),
    payment_method VARCHAR(50) NOT NULL CHECK (payment_method IN ('CREDIT_CARD', 'DEBIT_CARD', 'PIX', 'BOLETO')),
    provider VARCHAR(50) NOT NULL CHECK (provider IN ('STRIPE', 'MERCADOPAGO', 'PAGSEGURO')),
    provider_payment_id VARCHAR(100),
    provider_response JSONB,
    failure_reason TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    version INTEGER DEFAULT 1
);

-- Payment Webhooks Table
CREATE TABLE payments.payment_webhooks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_id UUID REFERENCES payments.payments(id),
    provider VARCHAR(50) NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    event_data JSONB NOT NULL,
    processed BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL
);

-- Outbox Events Table
CREATE TABLE payments.outbox_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    version INTEGER DEFAULT 1
);

-- Indexes
CREATE INDEX idx_payments_order ON payments.payments(order_id);
CREATE INDEX idx_payments_status ON payments.payments(status);
CREATE INDEX idx_payments_provider ON payments.payments(provider);
CREATE INDEX idx_payments_provider_payment_id ON payments.payments(provider_payment_id);
CREATE INDEX idx_payments_date ON payments.payments(created_at);

CREATE INDEX idx_payment_webhooks_payment ON payments.payment_webhooks(payment_id);
CREATE INDEX idx_payment_webhooks_processed ON payments.payment_webhooks(processed) WHERE processed = false;
CREATE INDEX idx_payment_webhooks_provider ON payments.payment_webhooks(provider, event_type);

CREATE INDEX idx_payments_outbox_processed ON payments.outbox_events(processed_at) WHERE processed_at IS NULL;

-- =====================================================
-- 6. NOTIFICATION SERVICE DATABASE
-- =====================================================
\c postgres;
CREATE DATABASE notification_service;
\c notification_service;

CREATE SCHEMA IF NOT EXISTS notifications;

-- Notification Templates Table
CREATE TABLE notifications.templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    type VARCHAR(20) NOT NULL CHECK (type IN ('EMAIL', 'SMS', 'PUSH')),
    subject VARCHAR(200),
    body TEXT NOT NULL,
    variables JSONB,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Notifications Table
CREATE TABLE notifications.notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    template_id UUID REFERENCES notifications.templates(id),
    type VARCHAR(20) NOT NULL CHECK (type IN ('EMAIL', 'SMS', 'PUSH')),
    recipient VARCHAR(200) NOT NULL,
    subject VARCHAR(200),
    body TEXT NOT NULL,
    status VARCHAR(20) DEFAULT 'PENDING' CHECK (status IN ('PENDING', 'SENT', 'FAILED', 'DELIVERED')),
    provider VARCHAR(50),
    provider_response JSONB,
    scheduled_at TIMESTAMP WITH TIME ZONE,
    sent_at TIMESTAMP WITH TIME ZONE,
    delivered_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    version INTEGER DEFAULT 1
);

-- Outbox Events Table
CREATE TABLE notifications.outbox_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL,
    aggregate_type VARCHAR(100) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE NULL,
    version INTEGER DEFAULT 1
);

-- Indexes
CREATE INDEX idx_templates_name ON notifications.templates(name);
CREATE INDEX idx_templates_type_active ON notifications.templates(type, is_active) WHERE is_active = true;

CREATE INDEX idx_notifications_user ON notifications.notifications(user_id);
CREATE INDEX idx_notifications_status ON notifications.notifications(status);
CREATE INDEX idx_notifications_scheduled ON notifications.notifications(scheduled_at) WHERE status = 'PENDING' AND scheduled_at IS NOT NULL;
CREATE INDEX idx_notifications_type ON notifications.notifications(type);
CREATE INDEX idx_notifications_date ON notifications.notifications(created_at);

CREATE INDEX idx_notifications_outbox_processed ON notifications.outbox_events(processed_at) WHERE processed_at IS NULL;

-- =====================================================
-- COMMON FUNCTIONS FOR ALL SERVICES
-- =====================================================

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$ LANGUAGE plpgsql;

-- Apply update timestamp trigger to relevant tables
\c identity_service;
CREATE TRIGGER update_user_profiles_updated_at BEFORE UPDATE ON identity.user_profiles FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

\c catalog_service;
CREATE TRIGGER update_categories_updated_at BEFORE UPDATE ON catalog.categories FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();
CREATE TRIGGER update_products_updated_at BEFORE UPDATE ON catalog.products FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

\c inventory_service;
CREATE TRIGGER update_stock_updated_at BEFORE UPDATE ON inventory.stock FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

\c order_service;
CREATE TRIGGER update_addresses_updated_at BEFORE UPDATE ON orders.customer_addresses FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();
CREATE TRIGGER update_orders_updated_at BEFORE UPDATE ON orders.orders FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

\c payment_service;
CREATE TRIGGER update_payments_updated_at BEFORE UPDATE ON payments.payments FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();

\c notification_service;
CREATE TRIGGER update_templates_updated_at BEFORE UPDATE ON notifications.templates FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();