# Makefile for E-commerce Microservices Infrastructure
# =====================================================

.PHONY: help setup start stop restart clean logs status test backup restore

# Default target
help: ## Show this help message
	@echo "E-commerce Microservices - Available Commands"
	@echo "=============================================="
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@echo ""
	@echo "Examples:"
	@echo "  make setup     # Complete infrastructure setup"
	@echo "  make start     # Start all services"
	@echo "  make logs      # Show logs from all services"
	@echo "  make clean     # Remove all containers and volumes"

# Infrastructure Management
setup: ## Complete infrastructure setup with sample data
	@echo "🚀 Starting complete infrastructure setup..."
	@chmod +x scripts/setup.sh
	@./scripts/setup.sh

start: ## Start all infrastructure services
	@echo "🔄 Starting all services..."
	@docker-compose up -d
	@echo "✅ All services started"
	@make status

stop: ## Stop all services
	@echo "🛑 Stopping all services..."
	@docker-compose down
	@echo "✅ All services stopped"

restart: ## Restart all services
	@echo "🔄 Restarting all services..."
	@docker-compose restart
	@echo "✅ All services restarted"
	@make status

# Monitoring and Logs
logs: ## Show logs from all services
	@docker-compose logs -f

logs-service: ## Show logs from specific service (use: make logs-service SERVICE=postgres)
	@docker-compose logs -f $(SERVICE)

status: ## Show status of all services
	@echo "📊 Service Status:"
	@echo "=================="
	@docker-compose ps

health: ## Check health of all services
	@echo "🏥 Health Check:"
	@echo "================"
	@echo "PostgreSQL:"
	@docker exec ecommerce_postgres pg_isready -U postgres || echo "❌ PostgreSQL not ready"
	@echo "Redis:"
	@docker exec ecommerce_redis redis-cli -a redis_password ping || echo "❌ Redis not ready"
	@echo "RabbitMQ:"
	@docker exec ecommerce_rabbitmq rabbitmq-diagnostics -q ping || echo "❌ RabbitMQ not ready"
	@echo "Keycloak:"
	@curl -f http://localhost:8080/health/ready >/dev/null 2>&1 && echo "✅ Keycloak ready" || echo "❌ Keycloak not ready"
	@echo "Elasticsearch:"
	@curl -f http://localhost:9200/_cluster/health >/dev/null 2>&1 && echo "✅ Elasticsearch ready" || echo "❌ Elasticsearch not ready"

# Database Management
db-shell: ## Access PostgreSQL shell
	@docker exec -it ecommerce_postgres psql -U postgres

db-backup: ## Backup all databases
	@echo "💾 Creating database backup..."
	@mkdir -p backups
	@docker exec ecommerce_postgres pg_dumpall -U postgres > backups/backup_$(shell date +%Y%m%d_%H%M%S).sql
	@echo "✅ Backup created in backups/ directory"

db-restore: ## Restore database from backup (use: make db-restore BACKUP=backup_file.sql)
	@echo "🔄 Restoring database from $(BACKUP)..."
	@docker exec -i ecommerce_postgres psql -U postgres < backups/$(BACKUP)
	@echo "✅ Database restored"

db-migrate: ## Run database migrations (placeholder for when services are implemented)
	@echo "📋 Running database migrations..."
	@echo "⚠️  Migrations will be implemented when services are created"

# Redis Management
redis-shell: ## Access Redis shell
	@docker exec -it ecommerce_redis redis-cli -a redis_password

redis-flush: ## Clear all Redis data
	@echo "🗑️  Clearing Redis data..."
	@docker exec ecommerce_redis redis-cli -a redis_password FLUSHALL
	@echo "✅ Redis data cleared"

redis-info: ## Show Redis information
	@docker exec ecommerce_redis redis-cli -a redis_password INFO

# RabbitMQ Management
rabbitmq-shell: ## Access RabbitMQ shell
	@docker exec -it ecommerce_rabbitmq rabbitmqctl

rabbitmq-queues: ## List all RabbitMQ queues
	@docker exec ecommerce_rabbitmq rabbitmqctl list_queues -p ecommerce

rabbitmq-exchanges: ## List all RabbitMQ exchanges
	@docker exec ecommerce_rabbitmq rabbitmqctl list_exchanges -p ecommerce

rabbitmq-reset: ## Reset RabbitMQ (clear all data)
	@echo "🗑️  Resetting RabbitMQ..."
	@docker exec ecommerce_rabbitmq rabbitmqctl stop_app
	@docker exec ecommerce_rabbitmq rabbitmqctl reset
	@docker exec ecommerce_rabbitmq rabbitmqctl start_app
	@echo "✅ RabbitMQ reset complete"

# Development Utilities
seed-data: ## Re-run database seed data
	@echo "🌱 Seeding database with sample data..."
	@docker exec -i ecommerce_postgres psql -U postgres < scripts/seed-data.sql
	@echo "✅ Sample data seeded"

create-sample-cart: ## Create sample cart data in Redis
	@echo "🛒 Creating sample cart data..."
	@docker exec ecommerce_redis redis-cli -a redis_password SET "cart:11111111-1111-1111-1111-111111111111" '{"userId":"11111111-1111-1111-1111-111111111111","items":[{"productId":"660e8400-e29b-41d4-a716-446655440006","productName":"Camiseta Básica","sku":"CLOTH-MEN-TSHIRT-BASIC","quantity":2,"unitPrice":39.90,"totalPrice":79.80}],"totalItems":2,"totalAmount":79.80}'
	@docker exec ecommerce_redis redis-cli -a redis_password EXPIRE "cart:11111111-1111-1111-1111-111111111111" 604800
	@echo "✅ Sample cart created"

# Testing
test-connections: ## Test all service connections
	@echo "🧪 Testing service connections..."
	@make health

test-keycloak: ## Test Keycloak authentication
	@echo "🔐 Testing Keycloak authentication..."
	@echo "Getting admin token..."
	@curl -s -X POST http://localhost:8080/realms/master/protocol/openid-connect/token \
		-H "Content-Type: application/x-www-form-urlencoded" \
		-d "username=admin&password=admin123&grant_type=password&client_id=admin-cli" \
		| jq -r '.access_token' | head -c 50
	@echo "..."
	@echo "✅ Keycloak authentication test complete"

test-realm: ## Test ecommerce realm authentication
	@echo "🏪 Testing ecommerce realm authentication..."
	@echo "Customer login test:"
	@curl -s -X POST http://localhost:8080/realms/ecommerce/protocol/openid-connect/token \
		-H "Content-Type: application/x-www-form-urlencoded" \
		-d "username=customer1&password=customer123&grant_type=password&client_id=ecommerce-api&client_secret=ecommerce-api-secret-2024" \
		| jq -r '.access_token' | head -c 50
	@echo "..."
	@echo "✅ Realm authentication test complete"

# Performance and Monitoring  
monitor: ## Open monitoring dashboards
	@echo "📊 Opening monitoring dashboards..."
	@echo "Grafana: http://localhost:3000 (admin/admin123)"
	@echo "Prometheus: http://localhost:9090"
	@echo "Jaeger: http://localhost:16686"
	@echo "RabbitMQ Management: http://localhost:15672 (admin/admin123)"

stats: ## Show system statistics
	@echo "📈 System Statistics:"
	@echo "===================="
	@echo "Docker containers:"
	@docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
	@echo ""
	@echo "Volume usage:"
	@docker system df -v | grep ecommerce || echo "No ecommerce volumes found"
	@echo ""
	@echo "Memory usage:"
	@docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}"

# Cleanup
clean-containers: ## Remove all containers
	@echo "🧹 Removing all containers..."
	@docker-compose down --remove-orphans
	@echo "✅ Containers removed"

clean-volumes: ## Remove all volumes (WARNING: This will delete all data!)
	@echo "⚠️  WARNING: This will delete ALL data!"
	@echo "Are you sure? This action cannot be undone."
	@read -p "Type 'yes' to continue: " confirm && [ "$$confirm" = "yes" ] || exit 1
	@echo "🗑️  Removing all volumes..."
	@docker-compose down -v --remove-orphans
	@docker volume rm $$(docker volume ls -q | grep ecommerce) 2>/dev/null || true
	@echo "✅ Volumes removed"

clean: clean-containers clean-volumes ## Complete cleanup (containers + volumes)

clean-images: ## Remove all related Docker images
	@echo "🗑️  Removing Docker images..."
	@docker rmi $$(docker images | grep ecommerce | awk '{print $$3}') 2>/dev/null || true
	@docker image prune -f
	@echo "✅ Images removed"

reset: clean setup ## Complete reset and setup

# Development Environment
dev-setup: ## Setup for development (with additional tools)
	@echo "🛠️  Setting up development environment..."
	@make setup
	@echo "Installing additional development tools..."
	@echo "✅ Development environment ready"

# Port forwarding for external access
port-forward: ## Show port forwarding information
	@echo "🌐 Port Forwarding Information:"
	@echo "=============================="
	@echo "Service              | Local Port | Container Port"
	@echo "--------------------|------------|---------------"
	@echo "PostgreSQL          | 5432       | 5432"
	@echo "Redis               | 6379       | 6379"
	@echo "RabbitMQ (AMQP)     | 5672       | 5672"
	@echo "RabbitMQ (Management)| 15672      | 15672"
	@echo "Keycloak            | 8080       | 8080"
	@echo "PgAdmin             | 5050       | 80"
	@echo "Grafana             | 3000       | 3000"
	@echo "Prometheus          | 9090       | 9090"
	@echo "Jaeger UI           | 16686      | 16686"
	@echo "Elasticsearch       | 9200       | 9200"
	@echo "Kibana              | 5601       | 5601"
	@echo "MinIO API           | 9000       | 9000"
	@echo "MinIO Console       | 9001       | 9001"

# Backup and Restore
backup-all: ## Create complete backup (databases + configs)
	@echo "💾 Creating complete backup..."
	@mkdir -p backups/$(shell date +%Y%m%d_%H%M%S)
	@make db-backup
	@cp -r keycloak backups/$(shell date +%Y%m%d_%H%M%S)/
	@cp -r monitoring backups/$(shell date +%Y%m%d_%H%M%S)/
	@echo "✅ Complete backup created"

# Quick start combinations
quick-start: ## Quick start for development
	@echo "⚡ Quick start..."
	@make start
	@sleep 30
	@make health
	@make monitor

# Environment validation
validate: ## Validate environment setup
	@echo "✅ Validating environment..."
	@echo "Checking Docker..."
	@docker --version
	@echo "Checking Docker Compose..."
	@docker-compose --version
	@echo "Checking required ports..."
	@netstat -tuln | grep -E ':(5432|6379|5672|8080|15672)' || echo "Some ports may not be in use yet"
	@echo "✅ Environment validation complete"

# Documentation
docs: ## Generate and open documentation
	@echo "📚 Documentation links:"
	@echo "======================"
	@echo "Keycloak Admin: http://localhost:8080"
	@echo "API Documentation: Will be available when services are implemented"
	@echo "Database Schema: Check PgAdmin at http://localhost:5050"
	@echo "Message Queues: http://localhost:15672"

info: ## Show infrastructure information
	@echo "ℹ️  E-commerce Microservices Infrastructure"
	@echo "=========================================="
	@echo "Version: 1.0.0"
	@echo "Services: PostgreSQL, Redis, RabbitMQ, Keycloak, Elasticsearch, Grafana, Prometheus, Jaeger"
	@echo "Databases: 7 microservice databases + Keycloak"
	@echo "Sample Data: Products, Categories, Orders, Users"
	@echo ""
	@echo "🔗 Quick Links:"
	@make port-forward