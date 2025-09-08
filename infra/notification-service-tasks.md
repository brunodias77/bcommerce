# 📧 NOTIFICATION SERVICE - TASKS

## 📋 Overview
Serviço responsável por enviar notificações via email, SMS e push notifications.

## 🏗️ Infrastructure & Setup
- [ ] Criar projeto .NET 8 com Minimal APIs
- [ ] Configurar Entity Framework Core com PostgreSQL
- [ ] Configurar conexão com banco notification_service
- [ ] Implementar DbContext para Templates e Notifications
- [ ] Configurar migrations
- [ ] Configurar Serilog, FluentValidation, Polly

## 📨 Notification APIs
- [ ] **Templates Management**
  - [ ] GET /api/templates
  - [ ] POST /api/templates (admin only)
  - [ ] PUT /api/templates/{id} (admin only)
  - [ ] DELETE /api/templates/{id} (admin only)
- [ ] **Notification History**
  - [ ] GET /api/notifications/{userId}
  - [ ] GET /api/notifications/{id}
  - [ ] POST /api/notifications/send (internal)

## 📮 Notification Providers
- [ ] **Email Provider (SendGrid)**
  - [ ] Configurar SendGrid SDK
  - [ ] Template rendering com variáveis
  - [ ] HTML/Text dual format
  - [ ] Delivery status tracking
- [ ] **SMS Provider (Twilio)**
  - [ ] Configurar Twilio SDK
  - [ ] SMS templates
  - [ ] Delivery confirmation
- [ ] **Push Notifications (Firebase)**
  - [ ] FCM integration
  - [ ] Device token management
  - [ ] Topic subscriptions

## 🎨 Template System
- [ ] Template engine com variáveis dinâmicas
- [ ] Support para HTML e plain text
- [ ] Preview de templates
- [ ] Validação de templates e variáveis
- [ ] Multi-language support (futuro)

## ⏰ Scheduling & Queue
- [ ] Background service para processar notificações
- [ ] Scheduled notifications
- [ ] Retry logic para falhas
- [ ] Priority queue para notificações críticas
- [ ] Rate limiting por provider

## 📡 Integration
- [ ] MassTransit consumers para eventos de negócio:
  - [ ] OrderCreated -> order_confirmation
  - [ ] OrderShipped -> order_shipped  
  - [ ] PaymentFailed -> payment_failed
  - [ ] UserRegistered -> welcome_email
- [ ] Webhook receivers para status de entrega

## 🧪 Testing
- [ ] Testes de template rendering
- [ ] Mock providers para desenvolvimento
- [ ] Testes de delivery status
- [ ] Testes de scheduling

## 🎯 Priority
**MVP Phase 2** - Importante para UX

## 📝 Notes
- Templates flexíveis essenciais
- Queue para alta performance
- Múltiplos providers para redundância