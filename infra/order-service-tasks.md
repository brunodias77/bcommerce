# 🛒 ORDER SERVICE - TASKS

## 📋 Overview
Serviço responsável por gerenciar pedidos, itens, endereços e workflow de status.

## 🏗️ Infrastructure & Setup
- [ ] Criar projeto .NET 8 com Minimal APIs
- [ ] Configurar Entity Framework Core com PostgreSQL
- [ ] Configurar conexão com banco order_service
- [ ] Implementar DbContext para Orders, OrderItems, Addresses, OrderStatusHistory
- [ ] Configurar migrations com function de número de pedido
- [ ] Configurar Serilog, FluentValidation, Polly

## 📋 Orders API
- [ ] **Order Management**
  - [ ] GET /api/orders (usuário logado)
  - [ ] GET /api/orders/{id}
  - [ ] POST /api/orders (criar pedido)
  - [ ] PUT /api/orders/{id}/status
  - [ ] DELETE /api/orders/{id} (cancel order)
- [ ] **Order Items**
  - [ ] GET /api/orders/{orderId}/items
  - [ ] Validação de itens no momento da criação

## 🏠 Address Management
- [ ] **Addresses API**
  - [ ] GET /api/addresses (usuário logado)
  - [ ] POST /api/addresses
  - [ ] PUT /api/addresses/{id}
  - [ ] DELETE /api/addresses/{id}
  - [ ] PUT /api/addresses/{id}/set-default

## 🔄 Order Workflow
- [ ] Implementar state machine para status de pedidos
- [ ] Validação de estoque antes de criar pedido
- [ ] Cálculo automático de totais e taxas
- [ ] Integração com carrinho Redis
- [ ] Histórico completo de mudanças de status

## 💰 Order Calculations
- [ ] Cálculo de subtotal baseado nos itens
- [ ] Cálculo de frete (integração futura)
- [ ] Aplicação de descontos/cupons
- [ ] Cálculo de impostos (se aplicável)
- [ ] Validação de preços com Catalog Service

## 📡 Integration
- [ ] gRPC cliente para Catalog Service (validar produtos)
- [ ] gRPC cliente para Inventory Service (reservar estoque)
- [ ] MassTransit events: OrderCreated, OrderConfirmed, OrderCancelled
- [ ] Consumir eventos de pagamento: PaymentConfirmed, PaymentFailed

## 🧪 Testing
- [ ] Testes de workflow completo de pedidos
- [ ] Testes de cálculos e validações
- [ ] Testes de integração com outros serviços
- [ ] Testes de concorrência para criação simultânea

## 🎯 Priority
**MVP Phase 1** - Core do processo de compra

## 📝 Notes
- State machine crítica para workflow
- Integração com múltiplos serviços
- Cálculos precisos essenciais