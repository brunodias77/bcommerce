#!/bin/bash

# Configurações
KEYCLOAK_URL="http://localhost:8080"
ADMIN_USER="admin"
ADMIN_PASSWORD="admin123"
REALM_NAME="bcommerce"
CLIENT_ID="bcommerce-client"
CLIENT_SECRET="bcommerce-secret-2024"

echo "🚀 Configurando Keycloak para o projeto BCommerce..."

# Aguardar Keycloak estar disponível
echo "⏳ Aguardando Keycloak inicializar..."
until curl -s $KEYCLOAK_URL/health/ready | grep -q "UP"; do
    echo "Aguardando Keycloak..."
    sleep 5
done

echo "✅ Keycloak está disponível!"

# Obter token de acesso do admin
echo "🔑 Obtendo token de acesso..."
TOKEN=$(curl -s -X POST "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=$ADMIN_USER" \
  -d "password=$ADMIN_PASSWORD" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" | jq -r '.access_token')

if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
    echo "❌ Erro ao obter token de acesso"
    exit 1
fi

echo "✅ Token obtido com sucesso"

# Criar Realm
echo "🏗️ Criando realm '$REALM_NAME'..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "realm": "'$REALM_NAME'",
    "enabled": true,
    "displayName": "BCommerce Realm",
    "registrationAllowed": true,
    "loginWithEmailAllowed": true,
    "duplicateEmailsAllowed": false,
    "resetPasswordAllowed": true,
    "editUsernameAllowed": false,
    "bruteForceProtected": true,
    "accessTokenLifespan": 3600,
    "refreshTokenMaxReuse": 0,
    "ssoSessionMaxLifespan": 36000
  }'

echo "✅ Realm criado"

# Criar Client
echo "🔧 Criando client '$CLIENT_ID'..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "'$CLIENT_ID'",
    "name": "BCommerce Client",
    "description": "Client para aplicação BCommerce",
    "enabled": true,
    "clientAuthenticatorType": "client-secret",
    "secret": "'$CLIENT_SECRET'",
    "redirectUris": [
      "http://localhost:3000/*",
      "http://localhost:5000/*",
      "http://localhost:7000/*"
    ],
    "webOrigins": [
      "http://localhost:3000",
      "http://localhost:5000",
      "http://localhost:7000"
    ],
    "protocol": "openid-connect",
    "publicClient": false,
    "serviceAccountsEnabled": true,
    "authorizationServicesEnabled": true,
    "directAccessGrantsEnabled": true,
    "implicitFlowEnabled": false,
    "standardFlowEnabled": true,
    "attributes": {
      "access.token.lifespan": "3600",
      "client.secret.creation.time": "1640995200"
    }
  }'

echo "✅ Client criado"

# Criar usuário admin
echo "👤 Criando usuário admin (bruno@admin.com)..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "bruno.admin",
    "email": "bruno@admin.com",
    "firstName": "Bruno",
    "lastName": "Admin",
    "enabled": true,
    "emailVerified": true,
    "credentials": [{
      "type": "password",
      "value": "admin123",
      "temporary": false
    }],
    "attributes": {
      "role": ["admin"]
    }
  }'

echo "✅ Usuário admin criado"

# Criar usuário comum
echo "👤 Criando usuário comum (bruno@teste.com)..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "bruno.user",
    "email": "bruno@teste.com",
    "firstName": "Bruno",
    "lastName": "User",
    "enabled": true,
    "emailVerified": true,
    "credentials": [{
      "type": "password",
      "value": "user123",
      "temporary": false
    }],
    "attributes": {
      "role": ["user"]
    }
  }'

echo "✅ Usuário comum criado"

# Criar roles
echo "🎭 Criando roles..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "admin",
    "description": "Administrator role"
  }'

curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "user",
    "description": "Regular user role"
  }'

curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/roles" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "customer",
    "description": "Customer role"
  }'

echo "✅ Roles criadas"

echo ""
echo "🎉 Configuração do Keycloak concluída!"
echo ""
echo "📋 Informações de acesso:"
echo "   Keycloak Admin: http://localhost:8080"
echo "   Admin User: admin / admin123"
echo ""
echo "📋 Realm: $REALM_NAME"
echo "   Client ID: $CLIENT_ID"
echo "   Client Secret: $CLIENT_SECRET"
echo ""
echo "👥 Usuários criados:"
echo "   Admin: bruno@admin.com / admin123"
echo "   User: bruno@teste.com / user123"
echo ""
echo "🔗 URLs importantes:"
echo "   Keycloak Console: http://localhost:8080/admin"
echo "   Realm URL: http://localhost:8080/realms/$REALM_NAME"
echo "   Token Endpoint: http://localhost:8080/realms/$REALM_NAME/protocol/openid-connect/token"
echo ""