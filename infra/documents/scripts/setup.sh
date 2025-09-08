#!/bin/bash
# scripts/setup.sh
# =====================================================
# E-COMMERCE MICROSERVICES INFRASTRUCTURE SETUP
# =====================================================

set -e  # Exit on any error

echo "🚀 Iniciando setup da infraestrutura E-commerce..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "\n${BLUE}=== $1 ===${NC}"
}

# Check if Docker and Docker Compose are installed
check_prerequisites() {
    print_header "Verificando Pré-requisitos"
    
    if ! command -v docker &> /dev/null; then
        print_error "Docker não está instalado. Por favor, instale o Docker primeiro."
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        print_error "Docker Compose não está instalado. Por favor, instale o Docker Compose primeiro."
        exit 1
    fi
    
    print_status "Docker e Docker Compose encontrados ✓"
}

# Create necessary directories
create_directories() {
    print_header "Criando Diretórios"
    
    mkdir -p {scripts,keycloak,monitoring/{prometheus,grafana/{dashboards,provisioning}},logs}
    
    print_status "Diretórios criados ✓"
}

# Create monitoring configuration files
create_monitoring_configs() {
    print_header "Criando Configurações de Monitoramento"
    
    # Prometheus configuration
    cat > monitoring/prometheus.yml << 'EOF'
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  # - "first_rules.yml"
  # - "second_rules.yml"

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: 'api-gateway'
    static_configs:
      - targets: ['api-gateway:8080']
    metrics_path: '/metrics'
    scrape_interval: 10s

  - job_name: 'identity-service'
    static_configs:
      - targets: ['identity-service:5001']
    metrics_path: '/metrics'

  - job_name: 'catalog-service'
    static_configs:
      - targets: ['catalog-service:5002']
    metrics_path: '/metrics'

  - job_name: 'inventory-service'
    static_configs:
      - targets: ['inventory-service:5003']
    metrics_path: '/metrics'

  - job_name: 'order-service'
    static_configs:
      - targets: ['order-service:5004']
    metrics_path: '/metrics'

  - job_name: 'payment-service'
    static_configs:
      - targets: ['payment-service:5005']
    metrics_path: '/metrics'

  - job_name: 'notification-service'
    static_configs:
      - targets: ['notification-service:5006']
    metrics_path: '/metrics'
EOF

    # Grafana provisioning
    mkdir -p monitoring/grafana/provisioning/{dashboards,datasources}
    
    cat > monitoring/grafana/provisioning/datasources/prometheus.yml << 'EOF'
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: true
EOF

    cat > monitoring/grafana/provisioning/dashboards/dashboard.yml << 'EOF'
apiVersion: 1

providers:
  - name: 'E-commerce Dashboards'
    orgId: 1
    folder: ''
    type: file
    disableDeletion: false
    updateIntervalSeconds: 10
    allowUiUpdates: true
    options:
      path: /var/lib/grafana/dashboards
EOF

    print_status "Configurações de monitoramento criadas ✓"
}

# Create RabbitMQ configuration
create_rabbitmq_config() {
    print_header "Criando Configuração do RabbitMQ"
    
    cat > scripts/rabbitmq.conf << 'EOF'
default_vhost = ecommerce
default_user = admin
default_pass = admin123
default_permissions.configure = .*
default_permissions.read = .*
default_permissions.write = .*
management.tcp.port = 15672
EOF

    cat > scripts/rabbitmq-definitions.json << 'EOF'
{
  "vhosts": [
    {"name": "ecommerce"}
  ],
  "users": [
    {
      "name": "admin",
      "password_hash": "JL2Cj+3F0DFo3ZTNcFH/YpCULdw=",
      "hashing_algorithm": "rabbit_password_hashing_sha256",
      "tags": "administrator"
    }
  ],
  "permissions": [
    {
      "user": "admin",
      "vhost": "ecommerce",
      "configure": ".*",
      "write": ".*",
      "read": ".*"
    }
  ],
  "exchanges": [
    {
      "name": "ecommerce.events",
      "vhost": "ecommerce",
      "type": "topic",
      "durable": true,
      "auto_delete": false,
      "internal": false,
      "arguments": {}
    }
  ],
  "queues": [
    {
      "name": "identity.events",
      "vhost": "ecommerce",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "catalog.events", 
      "vhost": "ecommerce",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "inventory.events",
      "vhost": "ecommerce", 
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "order.events",
      "vhost": "ecommerce",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "payment.events",
      "vhost": "ecommerce",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    },
    {
      "name": "notification.commands",
      "vhost": "ecommerce",
      "durable": true,
      "auto_delete": false,
      "arguments": {}
    }
  ],
  "bindings": [
    {
      "source": "ecommerce.events",
      "vhost": "ecommerce",
      "destination": "identity.events",
      "destination_type": "queue",
      "routing_key": "identity.*",
      "arguments": {}
    },
    {
      "source": "ecommerce.events",
      "vhost": "ecommerce", 
      "destination": "catalog.events",
      "destination_type": "queue",
      "routing_key": "catalog.*",
      "arguments": {}
    },
    {
      "source": "ecommerce.events",
      "vhost": "ecommerce",
      "destination": "inventory.events", 
      "destination_type": "queue",
      "routing_key": "inventory.*",
      "arguments": {}
    },
    {
      "source": "ecommerce.events",
      "vhost": "ecommerce",
      "destination": "order.events",
      "destination_type": "queue", 
      "routing_key": "order.*",
      "arguments": {}
    },
    {
      "source": "ecommerce.events",
      "vhost": "ecommerce",
      "destination": "payment.events",
      "destination_type": "queue",
      "routing_key": "payment.*", 
      "arguments": {}
    },
    {
      "source": "ecommerce.commands",
      "vhost": "ecommerce",
      "destination": "notification.commands",
      "destination_type": "queue",
      "routing_key": "notification",
      "arguments": {}
    }
  ]
}
EOF

    print_status "Configuração do RabbitMQ criada ✓"
}

# Create PgAdmin configuration
create_pgadmin_config() {
    print_header "Criando Configuração do PgAdmin"
    
    cat > scripts/pgadmin-servers.json << 'EOF'
{
  "Servers": {
    "1": {
      "Name": "E-commerce PostgreSQL",
      "Group": "Servers",
      "Host": "postgres",
      "Port": 5432,
      "MaintenanceDB": "postgres",
      "Username": "postgres",
      "SSLMode": "prefer",
      "SSLCert": "<STORAGE_DIR>/.postgresql/postgresql.crt",
      "SSLKey": "<STORAGE_DIR>/.postgresql/postgresql.key",
      "SSLCompression": 0,
      "Timeout": 10,
      "UseSSHTunnel": 0,
      "TunnelPort": "22",
      "TunnelAuthentication": 0
    }
  }
}
EOF

    print_status "Configuração do PgAdmin criada ✓"
}

# Start infrastructure services
start_infrastructure() {
    print_header "Iniciando Serviços de Infraestrutura"
    
    print_status "Parando containers existentes..."
    docker-compose down --remove-orphans
    
    print_status "Removendo volumes antigos (se existirem)..."
    docker volume rm $(docker volume ls -q | grep ecommerce) 2>/dev/null || true
    
    print_status "Construindo e iniciando containers..."
    docker-compose up -d --build
    
    print_status "Aguardando serviços ficarem prontos..."
}

# Wait for services to be ready
wait_for_services() {
    print_header "Aguardando Serviços"
    
    print_status "Aguardando PostgreSQL..."
    until docker exec ecommerce_postgres pg_isready -U postgres; do
        sleep 2
    done
    
    print_status "Aguardando Redis..."
    until docker exec ecommerce_redis redis-cli ping | grep PONG; do
        sleep 2
    done
    
    print_status "Aguardando RabbitMQ..."
    until docker exec ecommerce_rabbitmq rabbitmq-diagnostics -q ping; do
        sleep 5
    done
    
    print_status "Aguardando Keycloak..."
    until curl -f http://localhost:8080/health/ready >/dev/null 2>&1; do
        print_status "Keycloak ainda não está pronto, aguardando..."
        sleep 10
    done
    
    print_status "Todos os serviços estão prontos ✓"
}

# Setup Keycloak realm and users
setup_keycloak() {
    print_header "Configurando Keycloak"
    
    print_status "Aguardando Keycloak admin API..."
    sleep 30  # Give Keycloak more time to fully initialize
    
    # Get admin access token
    print_status "Obtendo token de acesso admin..."
    ADMIN_TOKEN=$(curl -s -X POST http://localhost:8080/realms/master/protocol/openid-connect/token \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "username=admin" \
        -d "password=admin123" \
        -d "grant_type=password" \
        -d "client_id=admin-cli" | jq -r '.access_token')
    
    if [ "$ADMIN_TOKEN" == "null" ] || [ -z "$ADMIN_TOKEN" ]; then
        print_error "Falha ao obter token de acesso do Keycloak"
        return 1
    fi
    
    print_status "Token obtido com sucesso ✓"
    
    # Check if realm already exists
    print_status "Verificando se realm 'ecommerce' existe..."
    REALM_EXISTS=$(curl -s -X GET http://localhost:8080/admin/realms/ecommerce \
        -H "Authorization: Bearer $ADMIN_TOKEN" \
        -w "%{http_code}" -o /dev/null)
    
    if [ "$REALM_EXISTS" == "200" ]; then
        print_warning "Realm 'ecommerce' já existe, pulando criação..."
    else
        print_status "Criando realm 'ecommerce'..."
        curl -s -X POST http://localhost:8080/admin/realms \
            -H "Authorization: Bearer $ADMIN_TOKEN" \
            -H "Content-Type: application/json" \
            -d @keycloak/realm-export.json
        
        if [ $? -eq 0 ]; then
            print_status "Realm 'ecommerce' criado com sucesso ✓"
        else
            print_error "Falha ao criar realm 'ecommerce'"
            return 1
        fi
    fi
    
    # The realm import should have created users, but let's verify
    print_status "Verificando usuários criados..."
    
    # Get realm-specific admin token
    REALM_TOKEN=$(curl -s -X POST http://localhost:8080/realms/ecommerce/protocol/openid-connect/token \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "username=admin" \
        -d "password=admin123" \
        -d "grant_type=password" \
        -d "client_id=ecommerce-api" \
        -d "client_secret=ecommerce-api-secret-2024" | jq -r '.access_token')
    
    if [ "$REALM_TOKEN" != "null" ] && [ -n "$REALM_TOKEN" ]; then
        print_status "Usuários disponíveis no realm 'ecommerce' ✓"
    fi
    
    print_status "Configuração do Keycloak concluída ✓"
}

# Test database connections
test_databases() {
    print_header "Testando Conexões com Bancos de Dados"
    
    databases=("keycloak" "identity_service" "catalog_service" "inventory_service" "order_service" "payment_service" "notification_service")
    
    for db in "${databases[@]}"; do
        print_status "Testando conexão com $db..."
        docker exec ecommerce_postgres psql -U postgres -d $db -c "SELECT 1;" >/dev/null
        if [ $? -eq 0 ]; then
            print_status "$db ✓"
        else
            print_error "Falha na conexão com $db"
        fi
    done
}

# Test Redis connection
test_redis() {
    print_header "Testando Conexão Redis"
    
    docker exec ecommerce_redis redis-cli -a redis_password ping
    if [ $? -eq 0 ]; then
        print_status "Redis ✓"
    else
        print_error "Falha na conexão com Redis"
    fi
}

# Create sample cart data in Redis
create_sample_data() {
    print_header "Criando Dados de Exemplo"
    
    print_status "Criando carrinho de exemplo no Redis..."
    
    # Sample cart for customer user
    CART_DATA='{
        "userId": "11111111-1111-1111-1111-111111111111",
        "items": [
            {
                "productId": "660e8400-e29b-41d4-a716-446655440006",
                "productName": "Camiseta Básica Masculina 100% Algodão",
                "sku": "CLOTH-MEN-TSHIRT-BASIC",
                "quantity": 2,
                "unitPrice": 39.90,
                "totalPrice": 79.80
            },
            {
                "productId": "660e8400-e29b-41d4-a716-446655440011",
                "productName": "Clean Code - Robert C. Martin",
                "sku": "BOOK-TECH-CLEANCODE-RCM",
                "quantity": 1,
                "unitPrice": 89.90,
                "totalPrice": 89.90
            }
        ],
        "totalItems": 3,
        "totalAmount": 169.70,
        "lastUpdated": "2024-01-01T10:00:00Z",
        "expiresAt": "2024-01-08T10:00:00Z"
    }'
    
    docker exec ecommerce_redis redis-cli -a redis_password SET "cart:11111111-1111-1111-1111-111111111111" "$CART_DATA"
    docker exec ecommerce_redis redis-cli -a redis_password EXPIRE "cart:11111111-1111-1111-1111-111111111111" 604800  # 7 days
    
    print_status "Dados de exemplo criados no Redis ✓"
}

# Show connection information
show_info() {
    print_header "Informações de Acesso"
    
    echo ""
    echo "🎉 Setup concluído com sucesso!"
    echo ""
    echo "📊 SERVIÇOS DISPONÍVEIS:"
    echo "========================================"
    echo "🔐 Keycloak Admin Console: http://localhost:8080"
    echo "   Username: admin"
    echo "   Password: admin123"
    echo ""
    echo "🐰 RabbitMQ Management: http://localhost:15672"
    echo "   Username: admin" 
    echo "   Password: admin123"
    echo ""
    echo "🐘 PgAdmin: http://localhost:5050"
    echo "   Email: admin@ecommerce.local"
    echo "   Password: admin123"
    echo ""
    echo "📊 Grafana: http://localhost:3000"
    echo "   Username: admin"
    echo "   Password: admin123"
    echo ""
    echo "🔍 Prometheus: http://localhost:9090"
    echo ""
    echo "📈 Jaeger UI: http://localhost:16686"
    echo ""
    echo "🗄️ MinIO Console: http://localhost:9001"
    echo "   Username: minioadmin"
    echo "   Password: minioadmin123"
    echo ""
    echo "🛒 USUÁRIOS DE TESTE (REALM: ecommerce):"
    echo "========================================"
    echo "👤 Admin:"
    echo "   Username: admin"
    echo "   Password: admin123"
    echo "   Email: admin@ecommerce.local"
    echo ""
    echo "👤 Cliente:"
    echo "   Username: customer1"
    echo "   Password: customer123"
    echo "   Email: customer@ecommerce.local"
    echo ""
    echo "👤 Gerente:"
    echo "   Username: manager1"  
    echo "   Password: manager123"
    echo "   Email: manager@ecommerce.local"
    echo ""
    echo "🔧 CONFIGURAÇÕES DE CONEXÃO:"
    echo "========================================"
    echo "PostgreSQL: localhost:5432"
    echo "Redis: localhost:6379 (password: redis_password)"
    echo "RabbitMQ: localhost:5672"
    echo ""
    echo "📝 PRÓXIMOS PASSOS:"
    echo "========================================"
    echo "1. Acesse o Keycloak e configure clients adicionais se necessário"
    echo "2. Verifique os dados de exemplo nos bancos via PgAdmin"  
    echo "3. Teste o carrinho de exemplo no Redis"
    echo "4. Configure monitoramento no Grafana"
    echo "5. Inicie o desenvolvimento dos microserviços"
    echo ""
}

# Cleanup function
cleanup_on_error() {
    print_error "Erro detectado. Executando limpeza..."
    docker-compose down --remove-orphans
    exit 1
}

# Set trap for cleanup on error
trap cleanup_on_error ERR

# Main execution
main() {
    print_header "E-COMMERCE MICROSERVICES SETUP"
    
    check_prerequisites
    create_directories
    create_monitoring_configs
    create_rabbitmq_config
    create_pgadmin_config
    start_infrastructure
    wait_for_services
    setup_keycloak
    test_databases
    test_redis
    create_sample_data
    show_info
}

# Run main function
main "$@""arguments": {}
    },
    {
      "name": "ecommerce.commands",
      "vhost": "ecommerce", 
      "type": "direct",
      "durable": true,
      "auto_delete": false,