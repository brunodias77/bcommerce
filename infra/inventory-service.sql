-- =====================================================
-- INVENTORY SERVICE DATABASE SCHEMA
-- =====================================================

-- Create database (run separately if needed)
-- CREATE DATABASE inventory_service;
-- \c inventory_service;

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Inventory Items Table
CREATE TABLE inventory_items (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id UUID NOT NULL UNIQUE,
    sku VARCHAR(50) NOT NULL UNIQUE,
    available_quantity INTEGER NOT NULL DEFAULT 0,
    reserved_quantity INTEGER NOT NULL DEFAULT 0,
    minimum_stock_level INTEGER DEFAULT 0,
    maximum_stock_level INTEGER,
    reorder_point INTEGER DEFAULT 0,
    location VARCHAR(100),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT chk_available_quantity_positive CHECK (available_quantity >= 0),
    CONSTRAINT chk_reserved_quantity_positive CHECK (reserved_quantity >= 0),
    CONSTRAINT chk_minimum_stock_positive CHECK (minimum_stock_level >= 0),
    CONSTRAINT chk_reorder_point_positive CHECK (reorder_point >= 0)
);

-- Stock Movements Table (for tracking stock changes)
CREATE TABLE stock_movements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    inventory_item_id UUID NOT NULL REFERENCES inventory_items(id),
    movement_type VARCHAR(20) NOT NULL, -- 'IN', 'OUT', 'RESERVE', 'RELEASE', 'ADJUSTMENT'
    quantity INTEGER NOT NULL,
    reference_id UUID, -- Order ID, Purchase ID, etc.
    reference_type VARCHAR(50), -- 'ORDER', 'PURCHASE', 'ADJUSTMENT', etc.
    reason VARCHAR(200),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    
    CONSTRAINT chk_quantity_not_zero CHECK (quantity != 0)
);

-- Stock Reservations Table
CREATE TABLE stock_reservations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    inventory_item_id UUID NOT NULL REFERENCES inventory_items(id),
    order_id UUID NOT NULL,
    quantity INTEGER NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    status VARCHAR(20) DEFAULT 'ACTIVE', -- 'ACTIVE', 'CONFIRMED', 'EXPIRED', 'CANCELLED'
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT chk_reservation_quantity_positive CHECK (quantity > 0)
);

-- Indexes
CREATE INDEX idx_inventory_items_product_id ON inventory_items(product_id);
CREATE INDEX idx_inventory_items_sku ON inventory_items(sku);
CREATE INDEX idx_inventory_items_available_quantity ON inventory_items(available_quantity);
CREATE INDEX idx_inventory_items_location ON inventory_items(location);

CREATE INDEX idx_stock_movements_inventory_item_id ON stock_movements(inventory_item_id);
CREATE INDEX idx_stock_movements_movement_type ON stock_movements(movement_type);
CREATE INDEX idx_stock_movements_reference ON stock_movements(reference_id, reference_type);
CREATE INDEX idx_stock_movements_created_at ON stock_movements(created_at);

CREATE INDEX idx_stock_reservations_inventory_item_id ON stock_reservations(inventory_item_id);
CREATE INDEX idx_stock_reservations_order_id ON stock_reservations(order_id);
CREATE INDEX idx_stock_reservations_status ON stock_reservations(status);
CREATE INDEX idx_stock_reservations_expires_at ON stock_reservations(expires_at);

-- Domain Events Table
CREATE TABLE domain_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    event_id UUID NOT NULL UNIQUE,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    processed_at TIMESTAMP WITH TIME ZONE
);

CREATE INDEX idx_domain_events_aggregate_id ON domain_events(aggregate_id);
CREATE INDEX idx_domain_events_event_type ON domain_events(event_type);