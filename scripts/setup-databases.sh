#!/bin/bash

echo "🗄️ Configurando bancos de dados dos microsserviços..."

# Aguardar bancos estarem disponíveis
echo "⏳ Aguardando bancos de dados iniciarem..."
sleep 10

# Identity Service
echo "📋 Configurando Identity Service Database..."
PGPASSWORD=identity123 psql -h localhost -p 5434 -U identity_user -d identity_service -f infra/identity-service.sql

# Catalog Service
echo "📋 Configurando Catalog Service Database..."
PGPASSWORD=catalog123 psql -h localhost -p 5435 -U catalog_user -d catalog_service -f infra/catalog-service.sql

# Inventory Service
echo "📋 Configurando Inventory Service Database..."
PGPASSWORD=inventory123 psql -h localhost -p 5436 -U inventory_user -d inventory_service -f infra/inventory-service.sql

# Order Service
echo "📋 Configurando Order Service Database..."
PGPASSWORD=order123 psql -h localhost -p 5437 -U order_user -d order_service -f infra/order-service.sql

# Payment Service
echo "📋 Configurando Payment Service Database..."
PGPASSWORD=payment123 psql -h localhost -p 5438 -U payment_user -d payment_service -f infra/payment-service.sql

# Notification Service
echo "📋 Configurando Notification Service Database..."
PGPASSWORD=notification123 psql -h localhost -p 5439 -U notification_user -d notification_service -f infra/notification-service.sql

echo "✅ Todos os bancos de dados foram configurados!"