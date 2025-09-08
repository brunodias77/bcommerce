-- =====================================================
-- PAYMENT SERVICE DATABASE SCHEMA
-- =====================================================

-- Create database (run separately if needed)
-- CREATE DATABASE payment_service;
-- \c payment_service;

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Payments Table
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    order_id UUID NOT NULL,
    customer_id UUID NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'BRL',
    payment_method VARCHAR(50) NOT NULL, -- 'CREDIT_CARD', 'DEBIT_CARD', 'PIX', 'BOLETO', 'PAYPAL'
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- 'PENDING', 'PROCESSING', 'COMPLETED', 'FAILED', 'CANCELLED', 'REFUNDED'
    
    -- Payment Gateway Info
    gateway_provider VARCHAR(50), -- 'STRIPE', 'MERCADOPAGO', 'PAGSEGURO', etc.
    gateway_transaction_id VARCHAR(100),
    gateway_payment_id VARCHAR(100),
    
    -- Card Info (encrypted/tokenized)
    card_last_four VARCHAR(4),
    card_brand VARCHAR(20),
    card_token VARCHAR(200),
    
    -- PIX Info
    pix_key VARCHAR(200),
    pix_qr_code TEXT,
    
    -- Boleto Info
    boleto_barcode VARCHAR(100),
    boleto_due_date DATE,
    
    failure_reason VARCHAR(500),
    processed_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    deleted_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT chk_amount_positive CHECK (amount > 0)
);

-- Payment Attempts Table (for retry logic)
CREATE TABLE payment_attempts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_id UUID NOT NULL REFERENCES payments(id) ON DELETE CASCADE,
    attempt_number INTEGER NOT NULL,
    status VARCHAR(20) NOT NULL,
    gateway_response JSONB,
    error_message VARCHAR(500),
    attempted_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Refunds Table
CREATE TABLE refunds (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    payment_id UUID NOT NULL REFERENCES payments(id),
    amount DECIMAL(10,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'BRL',
    reason VARCHAR(200),
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- 'PENDING', 'PROCESSING', 'COMPLETED', 'FAILED'
    gateway_refund_id VARCHAR(100),
    processed_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT chk_refund_amount_positive CHECK (amount > 0)
);

-- Indexes
CREATE INDEX idx_payments_order_id ON payments(order_id);
CREATE INDEX idx_payments_customer_id ON payments(customer_id);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_payments_payment_method ON payments(payment_method);
CREATE INDEX idx_payments_gateway_transaction_id ON payments(gateway_transaction_id);
CREATE INDEX idx_payments_created_at ON payments(created_at);

CREATE INDEX idx_payment_attempts_payment_id ON payment_attempts(payment_id);
CREATE INDEX idx_payment_attempts_attempted_at ON payment_attempts(attempted_at);

CREATE INDEX idx_refunds_payment_id ON refunds(payment_id);
CREATE INDEX idx_refunds_status ON refunds(status);
CREATE INDEX idx_refunds_created_at ON refunds(created_at);

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