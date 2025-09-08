#!/bin/bash

echo "🚀 Iniciando infraestrutura do BCommerce..."

# Subir os serviços
echo "📦 Subindo containers..."
docker-compose up -d

echo "⏳ Aguardando serviços iniciarem..."
sleep 30

# Executar configuração do Keycloak
echo "🔧 Configurando Keycloak..."
./scripts/setup-keycloak.sh

echo "✅ Infraestrutura pronta!"
echo ""
echo "📋 Serviços disponíveis:"
echo "   Keycloak: http://localhost:8080"
echo "   RabbitMQ Management: http://localhost:15672 (admin/admin123)"
echo "   Redis: localhost:6379"
echo ""
echo "🗄️ Bancos de dados:"
echo "   Keycloak DB: localhost:5433"
echo "   Identity Service: localhost:5434"
echo "   Catalog Service: localhost:5435"
echo "   Inventory Service: localhost:5436"
echo "   Order Service: localhost:5437"
echo "   Payment Service: localhost:5438"
echo "   Notification Service: localhost:5439"