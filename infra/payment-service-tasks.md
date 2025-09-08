# 💳 PAYMENT SERVICE - TASKS

## 📋 Overview
Serviço responsável por processar pagamentos, integrar com gateways e gerenciar transações.

## 🏗️ Infrastructure & Setup
- [ ] Criar projeto .NET 8 com Minimal APIs
- [ ] Configurar Entity Framework Core com PostgreSQL
- [ ] Configurar conexão com banco payment_service
- [ ] Implementar DbContext para Payments e PaymentWebhooks
- [ ] Configurar migrations
- [ ] Configurar Serilog, FluentValidation, Polly

## 💸 Payment APIs
- [ ] **Payment Processing**
  - [ ] POST /api/payments (iniciar pagamento)
  - [ ] GET /api/payments/{id}
  - [ ] GET /api/payments/order/{orderId}
  - [ ] POST /api/payments/{id}/refund
- [ ] **Webhook Handling**
  - [ ] POST /api/webhooks/stripe
  - [ ] POST /api/webhooks/mercadopago
  - [ ] GET /api/webhooks (admin only)

## 🔌 Payment Providers
- [ ] **Stripe Integration**
  - [ ] Configurar Stripe SDK
  - [ ] Implementar payment intents
  - [ ] Webhook signature validation
- [ ] **MercadoPago Integration**
  - [ ] SDK do MercadoPago
  - [ ] Suporte a PIX, cartão, boleto
  - [ ] Webhook processing
- [ ] **PagSeguro Integration** (futuro)
  - [ ] SDK básico
  - [ ] Payment methods locais

## 🔄 Payment Flow
- [ ] Implementar state machine para pagamentos
- [ ] Retry automatico para falhas temporárias
- [ ] Timeout handling para payments pendentes
- [ ] Reconciliação de pagamentos

## 🛡️ Security & Compliance
- [ ] Criptografia de dados sensíveis
- [ ] PCI compliance considerations
- [ ] Webhook signature validation
- [ ] Rate limiting para APIs sensíveis
- [ ] Audit logging de todas as operações

## 📡 Integration
- [ ] MassTransit events: PaymentCreated, PaymentConfirmed, PaymentFailed, PaymentRefunded
- [ ] Consumir eventos de pedidos: OrderCreated, OrderCancelled
- [ ] gRPC para consultas rápidas de status

## 🧪 Testing
- [ ] Testes com Stripe test mode
- [ ] Simulação de webhooks
- [ ] Testes de retry e timeout
- [ ] Testes de segurança e validação

## 🎯 Priority
**MVP Phase 2** - Essencial para monetização

## 📝 Notes
- Segurança crítica (PCI compliance)
- Webhooks para confirmação assíncrona
- Retry logic robusto