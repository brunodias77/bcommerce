# E-commerce Microservices Infrastructure

Uma infraestrutura completa para e-commerce baseada em microserviços, incluindo bancos de dados, autenticação, mensageria, monitoramento e observabilidade.

## Arquitetura

### Serviços de Infraestrutura
- **PostgreSQL**: Bancos de dados para cada microserviço
- **Redis**: Cache distribuído e gerenciamento de carrinho
- **RabbitMQ**: Mensageria assíncrona entre serviços
- **Keycloak**: Autenticação e autorização OAuth2/OpenID Connect
- **Elasticsearch**: Busca e indexação de produtos
- **Grafana + Prometheus**: Monitoramento e métricas
- **Jaeger**: Distributed tracing
- **MinIO**: Armazenamento de arquivos (S3-compatible)

### Microserviços (A serem implementados)
1. **Identity Service** - Gestão de perfis de usuário
2. **Catalog Service** - Catálogo de produtos e categorias  
3. **Inventory Service** - Gestão de estoque
4. **Order Service** - Processamento de pedidos
5. **Payment Service** - Processamento de pagamentos
6. **Notification Service** - Envio de notificações
7. **API Gateway** - Ponto de entrada único

## Pré-requisitos

- Docker 20.10+
- Docker Compose 2.0+
- 8GB RAM mínimo (recomendado 16GB)
- 20GB espaço em disco disponível
- Portas disponíveis: 5432, 6379, 5672, 8080, 9200, 3000, 9090, 16686, 9000, 5050, 15672, 5601, 9001

## Instalação e Configuração

### 1. Clone e Setup Inicial

```bash
# Clone o repositório (ou crie os arquivos manualmente)
git clone <repository-url>
cd ecommerce-microservices

# Ou crie a estrutura manualmente
mkdir ecommerce-microservices && cd ecommerce-microservices
mkdir -p {scripts,keycloak,monitoring/{prometheus,grafana/{dashboards,provisioning}}}
```

### 2. Criar Arquivos de Configuração

Crie os seguintes arquivos com o conteúdo fornecido:
- `docker-compose.yml`
- `scripts/init-databases.sql`
- `scripts/seed-data.sql` 
- `keycloak/realm-export.json`
- `scripts/setup.sh`
- `Makefile`
- `.env`

### 3. Setup Completo

```bash
# Torna o script executável
chmod +x scripts/setup.sh

# Executa setup completo
make setup
# OU
./scripts/setup.sh
```

### 4. Verificar Instalação

```bash
# Verificar status dos serviços
make status

# Verificar saúde dos serviços  
make health

# Ver logs
make logs
```

## Comandos Disponíveis (Makefile)

### Gerenciamento de Infraestrutura
```bash
make setup          # Setup completo com dados de exemplo
make start           # Iniciar todos os serviços
make stop            # Parar todos os serviços
make restart         # Reiniciar todos os serviços
make status          # Status dos serviços
make health          # Health check completo
```

### Logs e Monitoramento
```bash
make logs            # Logs de todos os serviços
make logs-service SERVICE=postgres  # Logs de serviço específico
make monitor         # Abrir dashboards de monitoramento
make stats           # Estatísticas do sistema
```

### Gerenciamento de Banco de Dados
```bash
make db-shell        # Acesso ao PostgreSQL
make db-backup       # Backup de todos os bancos
make db-restore BACKUP=backup.sql  # Restore do backup
make seed-data       # Re-executar dados de exemplo
```

### Gerenciamento Redis
```bash
make redis-shell     # Acesso ao Redis
make redis-info      # Informações do Redis
make redis-flush     # Limpar dados do Redis
make create-sample-cart  # Criar carrinho de exemplo
```

### Gerenciamento RabbitMQ
```bash
make rabbitmq-shell  # Acesso ao RabbitMQ
make rabbitmq-queues # Listar filas
make rabbitmq-exchanges # Listar exchanges
```

### Testes
```bash
make test-connections    # Testar conexões
make test-keycloak      # Testar autenticação Keycloak
make test-realm         # Testar realm ecommerce
```

### Limpeza
```bash
make clean-containers   # Remover containers
make clean-volumes      # Remover volumes (CUIDADO: apaga dados)
make clean             # Limpeza completa
make reset             # Reset e setup completo
```

## Acessos e Credenciais

### Dashboards de Administração
- **Keycloak Admin**: http://localhost:8080
  - User: `admin` | Password: `admin123`
- **RabbitMQ Management**: http://localhost:15672  
  - User: `admin` | Password: `admin123`
- **PgAdmin**: http://localhost:5050
  - Email: `admin@ecommerce.local` | Password: `admin123`
- **Grafana**: http://localhost:3000
  - User: `admin` | Password: `admin123`
- **Prometheus**: http://localhost:9090
- **Jaeger UI**: http://localhost:16686
- **MinIO Console**: http://localhost:9001
  - User: `minioadmin` | Password: `minioadmin123`

### Usuários de Teste (Realm: ecommerce)
- **Admin**:
  - Username: `admin` | Password: `admin123`
  - Email: `admin@ecommerce.local`
  - Roles: `admin`

- **Cliente**:
  - Username: `customer1` | Password: `customer123`  
  - Email: `customer@ecommerce.local`
  - Roles: `customer`

- **Gerente**:
  - Username: `manager1` | Password: `manager123`
  - Email: `manager@ecommerce.local`
  - Roles: `manager`, `customer`

### Conexões de Serviços
- **PostgreSQL**: `localhost:5432` (user: `postgres`, pass: `dev_password`)
- **Redis**: `localhost:6379` (password: `redis_password`)
- **RabbitMQ AMQP**: `localhost:5672` (user: `admin`, pass: `admin123`)
- **Elasticsearch**: `localhost:9200`

## Dados de Exemplo

A infraestrutura inclui dados de exemplo para desenvolvimento:

### Produtos
- iPhone 15 Pro, Samsung Galaxy S24 Ultra, MacBook Air M3
- Roupas, móveis, livros técnicos
- Imagens, atributos e categorias

### Pedidos
- 3 pedidos de exemplo com diferentes status
- Endereços de entrega configurados
- Histórico de status dos pedidos

### Estoque
- Quantidades iniciais para todos os produtos
- Movimentações de estoque registradas
- Configurações de estoque mínimo/máximo

### Notificações
- Templates para emails (confirmação, envio, falha no pagamento)
- Templates para SMS
- Templates para push notifications

## Estrutura dos Bancos de Dados

### identity_service
- `user_profiles`: Perfis complementares aos usuários do Keycloak
- `outbox_events`: Eventos de domínio para processamento assíncrono

### catalog_service  
- `categories`: Categorias hierárquicas de produtos
- `products`: Produtos com preços e informações
- `product_images`: Imagens dos produtos
- `product_attributes`: Atributos customizados (cor, tamanho, etc.)

### inventory_service
- `stock`: Controle de estoque por produto
- `stock_movements`: Histórico de movimentações

### order_service
- `customer_addresses`: Endereços de entrega dos clientes
- `orders`: Pedidos dos clientes
- `order_items`: Itens dos pedidos
- `order_status_history`: Histórico de mudanças de status

### payment_service
- `payments`: Registros de pagamentos
- `payment_webhooks`: Webhooks recebidos dos provedores

### notification_service
- `templates`: Templates de notificação
- `notifications`: Histórico de notificações enviadas

## Keycloak - Configuração

### Realm: ecommerce
- **Clients**:
  - `ecommerce-api`: Client confidencial para APIs (secret: `ecommerce-api-secret-2024`)
  - `ecommerce-frontend`: Client público para frontend

- **Roles**:
  - `admin`: Acesso administrativo completo
  - `customer`: Cliente regular
  - `manager`: Gerente com acessos limitados

- **Groups**: Administradores, Clientes, Gerentes

### Testando Autenticação

```bash
# Obter token de cliente
curl -X POST http://localhost:8080/realms/ecommerce/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=customer1" \
  -d "password=customer123" \
  -d "grant_type=password" \
  -d "client_id=ecommerce-api" \
  -d "client_secret=ecommerce-api-secret-2024"
```

## Redis - Estruturas de Dados

### Carrinho de Compras
```json
// Key: cart:{userId}
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
```

### Cache de Sessões
```json
// Key: session:{sessionId}
{
  "userId": "uuid",
  "roles": ["customer"],
  "lastActivity": "2024-01-01T10:00:00Z"
}
```

## RabbitMQ - Configuração

### Exchanges
- `ecommerce.events`: Exchange topic para eventos de domínio
- `ecommerce.commands`: Exchange direct para comandos

### Queues
- `identity.events`: Eventos do serviço de identidade
- `catalog.events`: Eventos do catálogo
- `inventory.events`: Eventos de estoque
- `order.events`: Eventos de pedidos
- `payment.events`: Eventos de pagamento
- `notification.commands`: Comandos de notificação

### Routing Keys
- `identity.*`: Eventos de usuário
- `catalog.*`: Eventos de produtos
- `inventory.*`: Eventos de estoque
- `order.*`: Eventos de pedidos
- `payment.*`: Eventos de pagamento

## Monitoramento

### Prometheus Targets
Configurado para coletar métricas dos microserviços (quando implementados):
- API Gateway: `api-gateway:8080/metrics`
- Identity Service: `identity-service:5001/metrics`
- Catalog Service: `catalog-service:5002/metrics`
- Inventory Service: `inventory-service:5003/metrics`
- Order Service: `order-service:5004/metrics`
- Payment Service: `payment-service:5005/metrics`
- Notification Service: `notification-service:5006/metrics`

### Grafana Dashboards
- Dashboards serão provisionados automaticamente
- DataSource Prometheus configurado automaticamente
- Acesso: http://localhost:3000 (admin/admin123)

### Jaeger Tracing
- Coleta traces via OTLP (gRPC: 4317, HTTP: 4318)
- UI disponível em: http://localhost:16686

## Troubleshooting

### Problemas Comuns

#### 1. Keycloak não inicia
```bash
# Verificar logs
docker logs ecommerce_keycloak

# Reiniciar Keycloak
docker restart ecommerce_keycloak
```

#### 2. PostgreSQL connection refused
```bash
# Verificar se PostgreSQL está rodando
docker ps | grep postgres

# Verificar logs
docker logs ecommerce_postgres

# Testar conexão
make db-shell
```

#### 3. Redis authentication failed
```bash
# Verificar senha no Redis
docker exec ecommerce_redis redis-cli -a redis_password ping

# Limpar dados se necessário  
make redis-flush
```

#### 4. RabbitMQ management não acessível
```bash
# Verificar se RabbitMQ está rodando
docker ps | grep rabbitmq

# Reiniciar RabbitMQ
docker restart ecommerce_rabbitmq

# Verificar logs
docker logs ecommerce_rabbitmq
```

### Logs e Debugging
```bash
# Logs de todos os serviços
make logs

# Logs de serviço específico
docker logs ecommerce_<service_name>

# Logs em tempo real
docker logs -f ecommerce_<service_name>
```

### Performance

#### Requisitos de Sistema
- **Desenvolvimento**: 8GB RAM, 4 CPU cores
- **Produção**: 16GB RAM, 8 CPU cores
- **Armazenamento**: SSD recomendado para melhor performance do PostgreSQL

#### Otimizações
- PostgreSQL configurado para desenvolvimento
- Redis com persistência AOF
- Elasticsearch com 512MB heap size
- Todos os serviços com health checks

## Desenvolvimento

### Próximos Passos
1. Implementar os microserviços em .NET 8
2. Configurar CI/CD com GitHub Actions
3. Implementar testes de integração
4. Configurar deployment em Kubernetes
5. Implementar frontend em Angular

### Estrutura de Projeto Recomendada
```
ecommerce-microservices/
├── src/
│   ├── services/
│   │   ├── identity-service/
│   │   ├── catalog-service/
│   │   ├── inventory-service/
│   │   ├── order-service/
│   │   ├── payment-service/
│   │   ├── notification-service/
│   │   └── api-gateway/
│   ├── shared/
│   └── frontend/
├── infrastructure/ (este repositório)
├── tests/
└── docs/
```

## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -am 'Add nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE](LICENSE) para detalhes.

## Suporte

Para problemas e dúvidas:
1. Verifique a seção [Troubleshooting](#troubleshooting)
2. Consulte os logs dos serviços
3. Abra uma issue no repositório

---

## Checklist de Instalação

- [ ] Docker e Docker Compose instalados
- [ ] Portas necessárias disponíveis
- [ ] Arquivos de configuração criados
- [ ] `make setup` executado com sucesso
- [ ] Todos os serviços healthy (`make health`)
- [ ] Keycloak acessível e realm criado
- [ ] Dados de exemplo carregados
- [ ] Usuários de teste funcionando
- [ ] Dashboards de monitoramento acessíveis

**Pronto para desenvolvimento! 🚀**