# Teste de Validação de Senha - Políticas do Keycloak

## Endpoint
```
POST http://localhost:5187/auth/register
Content-Type: application/json
```

## Validações Implementadas

As validações de senha agora seguem as políticas padrão do Keycloak:

1. **Comprimento mínimo**: 8 caracteres
2. **Pelo menos 1 letra maiúscula** (A-Z)
3. **Pelo menos 1 letra minúscula** (a-z)
4. **Pelo menos 1 dígito** (0-9)
5. **Pelo menos 1 caractere especial** (!@#$%^&*()_+-=[]{}|;:,.<>?)
6. **Não pode ser igual ao email ou nome do usuário**

## Exemplos de Teste no Postman

### ❌ Senha Inválida - Muito Curta
```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "123456",
  "phone": "+5511999999999"
}
```
**Erro esperado**: "A senha deve ter pelo menos 8 caracteres."

### ❌ Senha Inválida - Sem Letra Maiúscula
```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "abcd123!",
  "phone": "+5511999999999"
}
```
**Erro esperado**: "A senha deve conter pelo menos uma letra maiúscula."

### ❌ Senha Inválida - Sem Caractere Especial
```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "Abcd1234",
  "phone": "+5511999999999"
}
```
**Erro esperado**: "A senha deve conter pelo menos um caractere especial."

### ❌ Senha Inválida - Igual ao Email
```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "joao@email.com",
  "phone": "+5511999999999"
}
```
**Erro esperado**: "A senha não pode ser igual ao email ou nome do usuário."

### ✅ Senha Válida - Atende Todos os Critérios
```json
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "MinhaSenh@123",
  "phone": "+5511999999999"
}
```
**Resultado esperado**: Usuário criado com sucesso (Status 201)

### ✅ Outras Senhas Válidas
```json
// Exemplo 1
{
  "name": "Maria Santos",
  "email": "maria@email.com",
  "password": "Segur@2024",
  "phone": "+5511888888888"
}

// Exemplo 2
{
  "name": "Pedro Costa",
  "email": "pedro@email.com",
  "password": "MyP@ssw0rd",
  "phone": "+5511777777777"
}
```

## Como Testar

1. **Abra o Postman**
2. **Crie uma nova requisição POST**
3. **Configure a URL**: `http://localhost:5187/auth/register`
4. **Adicione o header**: `Content-Type: application/json`
5. **Cole um dos exemplos JSON no body**
6. **Envie a requisição**
7. **Verifique a resposta**:
   - **Status 400**: Erro de validação (senha inválida)
   - **Status 201**: Usuário criado com sucesso
   - **Status 409**: Email já existe

## Códigos de Resposta

- **201 Created**: Usuário criado com sucesso
- **400 Bad Request**: Dados inválidos (incluindo validações de senha)
- **409 Conflict**: Email já existe no sistema
- **500 Internal Server Error**: Erro interno do servidor

## Observações

- As validações são executadas **antes** da criação no Keycloak
- Se a senha não atender aos critérios, a requisição falha **imediatamente**
- As mensagens de erro são **específicas** para cada critério não atendido
- As validações são **case-sensitive** para letras maiúsculas e minúsculas