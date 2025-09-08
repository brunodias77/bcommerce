# 📚 CATALOG SERVICE - TASKS

## 📋 Overview
Serviço responsável por gerenciar catálogo de produtos, categorias, imagens e atributos.

## 🏗️ Infrastructure & Setup
- [ ] Criar projeto .NET 8 com Minimal APIs
- [ ] Configurar Entity Framework Core com PostgreSQL
- [ ] Configurar conexão com banco catalog_service
- [ ] Implementar DbContext para Categories, Products, ProductImages, ProductAttributes
- [ ] Configurar migrations iniciais
- [ ] Configurar Serilog
- [ ] Configurar FluentValidation e Polly

## 📦 Core Entities & APIs
- [ ] **Categories API**
  - [ ] GET /api/categories (com filtros e paginação)
  - [ ] GET /api/categories/{id}
  - [ ] POST /api/categories (admin only)
  - [ ] PUT /api/categories/{id} (admin only)
  - [ ] DELETE /api/categories/{id} (admin only)
- [ ] **Products API**
  - [ ] GET /api/products (com filtros, busca, paginação)
  - [ ] GET /api/products/{id}
  - [ ] GET /api/products/by-category/{categoryId}
  - [ ] POST /api/products (admin only)
  - [ ] PUT /api/products/{id} (admin only)
  - [ ] DELETE /api/products/{id} (admin only)
- [ ] **Product Images API**
  - [ ] GET /api/products/{productId}/images
  - [ ] POST /api/products/{productId}/images
  - [ ] DELETE /api/products/{productId}/images/{imageId}

## 🔍 Search & Filtering
- [ ] Implementar busca por nome, descrição e SKU
- [ ] Filtros por categoria, preço, marca
- [ ] Ordenação por preço, nome, popularidade
- [ ] Implementar paginação eficiente
- [ ] Cache de consultas frequentes no Redis

## 🛠️ Business Logic
- [ ] Validação de SKU único
- [ ] Geração automática de slugs
- [ ] Validação de preços
- [ ] Upload e processamento de imagens
- [ ] Implementar soft delete para produtos

## 📡 Integration
- [ ] gRPC endpoints para outros serviços
- [ ] MassTransit para eventos: ProductCreated, ProductUpdated, ProductDeleted
- [ ] Consumir eventos de estoque para atualizar disponibilidade
- [ ] API para validação de produtos (usado pelo Order Service)

## 🧪 Testing
- [ ] Testes unitários para business logic
- [ ] Testes de integração com banco
- [ ] Testes de API endpoints
- [ ] Testes de performance para busca
- [ ] Testcontainers para PostgreSQL

## 🚀 Performance & Monitoring
- [ ] Implementar cache estratégico
- [ ] Otimizar consultas N+1 com Include
- [ ] Configurar health checks
- [ ] Metrics para busca e popularidade

## 🎯 Priority
**MVP Phase 1** - Core do e-commerce

## 📝 Notes
- Performance crítica para busca de produtos
- Cache Redis essencial
- Integração com CDN para imagens