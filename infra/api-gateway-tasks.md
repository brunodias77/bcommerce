# 🚪 API GATEWAY - TASKS

## 📋 Overview
Gateway central para roteamento, autenticação e cross-cutting concerns.

## 🏗️ Infrastructure & Setup
- [ ] Criar projeto .NET 8 com YARP
- [ ] Configurar YARP reverse proxy
- [ ] Configurar OIDC authentication com Keycloak
- [ ] Implementar DelegatingHandlers customizados
- [ ] Configurar Serilog para request/response logging
- [ ] Configurar Polly para circuit breakers

## 🛣️ Route Configuration
- [ ] **Service Routes**
  - [ ] /api/identity/** -> Identity Service
  - [ ] /api/catalog/** -> Catalog Service
  - [ ] /api/inventory/** -> Inventory Service (internal only)
  - [ ] /api/orders/** -> Order Service
  - [ ] /api/payments/** -> Payment Service
  - [ ] /api/notifications/** -> Notification Service (admin only)

## 🔐 Security & Auth
- [ ] JWT token validation middleware
- [ ] Role-based routing (admin vs user)
- [ ] API key authentication para serviços internos
- [ ] Rate limiting global e por usuário
- [ ] CORS configuration

## 🛡️ Cross-Cutting Concerns
- [ ] **Global Exception Handler**
  - [ ] Consistent error response format
  - [ ] Error logging e correlation IDs
- [ ] **Request/Response Transformation**
  - [ ] Header manipulation
  - [ ] Response aggregation (se necessário)
- [ ] **Caching Strategy**
  - [ ] Response caching para endpoints públicos
  - [ ] Cache invalidation headers

## 📊 Monitoring & Observability
- [ ] Request metrics collection
- [ ] Response time tracking
- [ ] Error rate monitoring
- [ ] Health checks consolidados
- [ ] Distributed tracing preparation

## 🚀 Load Balancing & Resilience
- [ ] Service discovery integration
- [ ] Load balancing strategies
- [ ] Circuit breaker per service
- [ ] Timeout configurations
- [ ] Fallback responses

## 🧪 Testing
- [ ] Integration tests com todos os services
- [ ] Performance testing
- [ ] Security testing (auth, authorization)
- [ ] Chaos engineering tests

## 🎯 Priority
**MVP Phase 1** - Ponto de entrada único

## 📝 Notes
- YARP para performance
- Keycloak integration crítica
- Monitoring centralizado