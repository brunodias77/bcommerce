# 📦 INVENTORY SERVICE - TASKS

## 📋 Overview
Serviço responsável por gerenciar estoque, reservas e movimentações de produtos.

## 🏗️ Infrastructure & Setup
- [ ] Criar projeto .NET 8 com Minimal APIs
- [ ] Configurar Entity Framework Core com PostgreSQL
- [ ] Configurar conexão com banco inventory_service
- [ ] Implementar DbContext para Stock e StockMovements
- [ ] Configurar migrations
- [ ] Configurar Serilog, FluentValidation, Polly

## 📊 Stock Management APIs
- [ ] **Stock API**
  - [ ] GET /api/stock/{productId}
  - [ ] GET /api/stock (com filtros)
  - [ ] POST /api/stock/{productId}/adjust
  - [ ] POST /api/stock/{productId}/reserve
  - [ ] POST /api/stock/{productId}/release
- [ ] **Stock Movements API**
  - [ ] GET /api/stock/{productId}/movements
  - [ ] POST /api/stock/{productId}/movements (histórico)

## 🔄 Stock Operations
- [ ] Implementar reserva de estoque para pedidos
- [ ] Liberação automática de estoque não confirmado
- [ ] Baixa de estoque para pedidos confirmados
- [ ] Ajustes manuais de estoque com auditoria
- [ ] Alertas de estoque baixo

## 🎯 Business Rules
- [ ] Validar quantidade disponível vs reservada
- [ ] Implementar regras de estoque mínimo/máximo
- [ ] Controle de concorrência para atualizações de estoque
- [ ] Logs detalhados de todas as movimentações

## 📡 Integration
- [ ] gRPC para verificação rápida de disponibilidade
- [ ] MassTransit events: StockReserved, StockReleased, StockAdjusted
- [ ] Consumir eventos de pedidos: OrderCreated, OrderCancelled
- [ ] Consumir eventos de produtos: ProductCreated

## ⚡ Performance & Reliability
- [ ] Implementar distributed lock para operations críticas
- [ ] Cache Redis para consultas de estoque frequentes
- [ ] Retry policies com Polly para falhas temporárias
- [ ] Circuit breaker para dependencies

## 🧪 Testing
- [ ] Testes de concorrência para reservas simultâneas
- [ ] Testes de integração com eventos
- [ ] Testes de performance para operations críticas
- [ ] Simulation de cenários de alta carga

## 🎯 Priority
**MVP Phase 2** - Essencial para controle de estoque

## 📝 Notes
- Concorrência crítica para reservas
- Distributed locks necessários
- Auditoria completa de movimentações