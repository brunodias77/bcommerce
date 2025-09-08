# 🚀 Prompt para Criação de E-commerce com Microsserviços
Baseado na análise dos arquivos fornecidos ( `data-base.sql` , `init.cs` , e `tasks.md` ), aqui está um prompt completo para uma IA criar seu e-commerce:

## 🎯 PROMPT PARA IA: Criação de E-commerce com Microsserviços
### Contexto e Objetivo
Crie um sistema de e-commerce completo usando arquitetura de microsserviços com .NET 8, seguindo os padrões DDD (Domain-Driven Design), CQRS, e Event-Driven Architecture. O sistema deve ser escalável, resiliente e seguir as melhores práticas de desenvolvimento.

### 🏗️ Arquitetura Geral
```
ecommerce-microservices/
├── src/
│   ├── services/
│   │   ├── identity-service/     # 
Gestão de perfis de usuário
│   │   ├── catalog-service/      # 
Catálogo de produtos
│   │   ├── inventory-service/    # 
Controle de estoque
│   │   ├── order-service/        # 
Gestão de pedidos
│   │   ├── payment-service/      # 
Processamento de pagamentos
│   │   ├── notification-service/ # 
Notificações (email, SMS, push)
│   │   └── api-gateway/          # 
Gateway de APIs
│   ├── shared/                   # 
Bibliotecas compartilhadas
│   └── frontend/                 # 
Frontend Angular 20
├── infrastructure/               # 
Docker, K8s, Terraform
└── docs/                        # 
Documentação
```
### 🛠️ Stack Tecnológica
- Backend : .NET 8 com Minimal APIs
- Banco de Dados : PostgreSQL (um por serviço)
- Cache : Redis (carrinho, sessões, cache)
- Mensageria : RabbitMQ com MassTransit
- Autenticação : Keycloak (OIDC/OAuth2)
- Frontend : Angular 20 com TailwindCSS
- Containerização : Docker + Docker Compose
- Observabilidade : Serilog, OpenTelemetry
- Pagamentos : Stripe, MercadoPago, PagSeguro
### 📊 Modelos de Domínio (Base)
Implemente as seguintes entidades base já definidas:
 Shared Domain (Common)
```
// Base classes para todas as entidades
public abstract class Entity { /* 
implementação com Id, timestamps, soft 
delete */ }
public abstract class AggregateRoot : 
Entity { /* eventos de domínio */ }
public abstract class ValueObject { /* 
objetos de valor imutáveis */ }
public abstract record DomainEvent { /* 
eventos base */ }
``` Identity Service Domain
```
// Value Objects
PersonName, PhoneNumber

// Entities
UserProfile (integração com Keycloak)

// Events
UserProfileCreated, UserProfileUpdated
``` Catalog Service Domain
```
// Value Objects
Slug, Money, ProductDimensions

// Entities
Category, Product, ProductImage, 
ProductAttribute

// Events
CategoryCreated, ProductCreated, 
ProductUpdated, ProductPriceUpdated
``` Inventory Service Domain
```
// Value Objects
StockQuantity

// Entities
Stock, StockMovement

// Events
StockReserved, StockReleased, 
StockAdjusted
``` Order Service Domain
```
// Value Objects
Address, OrderNumber

// Entities
Order, OrderItem, OrderStatusHistory

// Events
OrderCreated, OrderConfirmed, 
OrderCancelled, OrderShipped
``` Payment Service Domain
```
// Entities
Payment, PaymentWebhook

// Events
PaymentCreated, PaymentConfirmed, 
PaymentFailed, PaymentRefunded
```
### 🗄️ Esquema de Banco de Dados
Crie 6 bancos PostgreSQL separados:

1. 1.
   identity_service : user_profiles
2. 2.
   catalog_service : categories, products, product_images, product_attributes
3. 3.
   inventory_service : stock, stock_movements
4. 4.
   order_service : orders, order_items, addresses, order_status_history
5. 5.
   payment_service : payments, payment_webhooks
6. 6.
   notification_service : templates, notifications
### 🔧 Implementação por Serviço 1. Identity Service
- Integração com Keycloak para autenticação
- Gestão de perfis complementares
- APIs: GET/POST/PUT/DELETE profiles
- gRPC para comunicação inter-serviços
- Eventos: UserProfileCreated, UserProfileUpdated 2. Catalog Service
- CRUD completo de categorias e produtos
- Sistema de busca e filtros avançados
- Upload e gestão de imagens
- Cache Redis para consultas frequentes
- APIs públicas e administrativas 3. Inventory Service
- Controle de estoque em tempo real
- Reserva/liberação para pedidos
- Histórico de movimentações
- Alertas de estoque baixo
- Controle de concorrência 4. Order Service
- Workflow completo de pedidos
- Gestão de endereços
- Integração com carrinho Redis
- State machine para status
- Cálculos automáticos de totais 5. Payment Service
- Múltiplos provedores (Stripe, MercadoPago)
- Processamento de webhooks
- Retry automático e reconciliação
- Segurança PCI compliance
- Refunds e estornos 6. Notification Service
- Templates dinâmicos (email, SMS, push)
- Múltiplos provedores (SendGrid, Twilio, FCM)
- Scheduling e retry
- Histórico de notificações 7. API Gateway
- YARP reverse proxy
- Autenticação centralizada
- Rate limiting
- Circuit breakers
- Logging e observabilidade
### 🌐 Frontend Angular 20
- Standalone components
- TailwindCSS para styling
- OIDC Client para Keycloak
- RxJS e Signals
- PWA ready
- Responsive design
Páginas principais:

- Catálogo de produtos com filtros
- Detalhes do produto
- Carrinho de compras
- Checkout completo
- Perfil do usuário
- Histórico de pedidos
- Painel administrativo
### 🔄 Comunicação entre Serviços Síncrona (gRPC)
- Validação de produtos (Catalog → Order)
- Verificação de estoque (Inventory → Order)
- Dados de usuário (Identity → outros) Assíncrona (Events via RabbitMQ)
- OrderCreated → Inventory (reservar estoque)
- PaymentConfirmed → Order (confirmar pedido)
- OrderShipped → Notification (enviar email)
- UserRegistered → Notification (email boas-vindas)
### 🐳 Infraestrutura e DevOps Docker Compose (Desenvolvimento)
```
services:
  postgres: # Banco principal
  redis: # Cache e carrinho
  rabbitmq: # Mensageria
  keycloak: # Autenticação
  # + todos os microsserviços
``` Observabilidade
- Serilog para logging estruturado
- Health checks em todos os serviços
- Correlation IDs para tracing
- Metrics customizados Segurança
- JWT validation em todos os serviços
- Role-based access control
- Rate limiting
- CORS configurado
- Secrets management
### 📋 Plano de Implementação (MVP) Fase 1 (4-6 semanas)
1. 1.
   Setup da infraestrutura base
2. 2.
   Identity Service básico
3. 3.
   Catalog Service (produtos e categorias)
4. 4.
   Order Service (criação de pedidos)
5. 5.
   API Gateway (routing + auth)
6. 6.
   Frontend básico (listagem, checkout simples) Fase 2 (3-4 semanas)
1. 1.
   Payment Service (Stripe)
2. 2.
   Inventory Service (controle de estoque)
3. 3.
   Notification Service (emails)
4. 4.
   Carrinho Redis
5. 5.
   Frontend completo Fase 3 (4-6 semanas)
1. 1.
   Múltiplos payment providers
2. 2.
   Observabilidade completa
3. 3.
   Testes automatizados
4. 4.
   Deploy em Kubernetes
5. 5.
   Performance optimization
### 🧪 Requisitos de Qualidade
- Testes : Unitários, integração, E2E
- Performance : < 200ms para APIs críticas
- Disponibilidade : 99.9% uptime
- Segurança : OWASP compliance
- Escalabilidade : Horizontal scaling ready
### 📚 Padrões e Práticas
- Domain-Driven Design (DDD)
- CQRS para operações complexas
- Event Sourcing onde apropriado
- Clean Architecture
- Repository Pattern
- Unit of Work
- Specification Pattern
- Circuit Breaker
- Retry Policies
🎯 Resultado Esperado : Um e-commerce completo, escalável e production-ready seguindo as melhores práticas de microsserviços, com todas as funcionalidades essenciais implementadas e documentadas.

📝 Entregáveis :

- Código fonte completo de todos os serviços
- Frontend Angular funcional
- Docker Compose para desenvolvimento
- Documentação técnica e de APIs
- Scripts de banco de dados
- Testes automatizados
- Guias de deployment
Este prompt fornece uma base sólida para uma IA implementar seu sistema de e-commerce com microsserviços seguindo exatamente a arquitetura e especificações definidas nos seus arquivos.