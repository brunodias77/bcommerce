# 🔐 IDENTITY SERVICE - TASKS

## 📋 Overview
Serviço responsável por gerenciar perfis de usuários e integração com Keycloak para autenticação e autorização.

## 🏗️ Infrastructure & Setup
- [ ] Criar projeto .NET 8 com Minimal APIs
- [ ] Configurar Entity Framework Core com PostgreSQL
- [ ] Configurar conexão com banco identity_service
- [ ] Implementar DbContext e migrations iniciais
- [ ] Configurar Serilog para logging
- [ ] Configurar FluentValidation
- [ ] Configurar Polly para resilience patterns

## 🔑 Keycloak Integration
- [ ] Configurar OIDC Authentication com Keycloak
- [ ] Implementar middleware de autenticação
- [ ] Criar service para sincronizar usuários Keycloak com perfis locais
- [ ] Implementar validação de JWT tokens
- [ ] Configurar roles e permissions mapping

## 📊 Core Features
- [ ] **User Profiles API**
  - [ ] GET /api/profiles/{keycloakUserId}
  - [ ] POST /api/profiles (criar perfil)
  - [ ] PUT /api/profiles/{id} (atualizar perfil)
  - [ ] DELETE /api/profiles/{id}
- [ ] Implementar DTOs e mapeamento com AutoMapper
- [ ] Implementar validações de negócio
- [ ] Adicionar cache Redis para perfis frequentes

## 🚀 Advanced Features
- [ ] Implementar gRPC endpoints para comunicação inter-serviços
- [ ] Configurar MassTransit para eventos de usuário
- [ ] Publicar eventos: UserProfileCreated, UserProfileUpdated
- [ ] Implementar rate limiting
- [ ] Adicionar health checks

## 🧪 Testing
- [ ] Configurar xUnit e Testcontainers
- [ ] Testes unitários para services
- [ ] Testes de integração com banco
- [ ] Testes de API com WebApplicationFactory
- [ ] Mocks com Moq para dependencies

## 📦 DevOps
- [ ] Criar Dockerfile otimizado
- [ ] Configurar Docker Compose para desenvolvimento
- [ ] Setup GitHub Actions CI/CD
- [ ] Configurar SonarQube integration

## 🎯 Priority
**MVP Phase 1** - Essential para autenticação básica

## 📝 Notes
- Integração crítica com Keycloak
- Base para autorização em todos os outros serviços
- Cache Redis importante para performance