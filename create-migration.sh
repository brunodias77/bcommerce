#!/bin/bash

# Script para criar uma nova migration do Entity Framework
# Uso: ./create-migration.sh <nome-da-migration>

set -e  # Sair em caso de erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para exibir mensagens coloridas
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Verificar se o nome da migration foi fornecido
if [ -z "$1" ]; then
    print_error "Nome da migration não fornecido!"
    echo "Uso: $0 <nome-da-migration>"
    echo "Exemplo: $0 AddUserProfileTable"
    exit 1
fi

MIGRATION_NAME="$1"

# Verificar se dotnet está instalado
if ! command -v dotnet &> /dev/null; then
    print_error "dotnet CLI não encontrado. Instale o .NET SDK primeiro."
    exit 1
fi

# Verificar se dotnet ef tools está instalado
if ! dotnet ef --version &> /dev/null; then
    print_error "Entity Framework tools não encontrado."
    print_info "Instalando dotnet ef tools..."
    dotnet tool install --global dotnet-ef
fi

# Diretórios do projeto
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRASTRUCTURE_DIR="$PROJECT_ROOT/backend/src/Services/UserService/UserService.Infrastructure"
API_DIR="$PROJECT_ROOT/backend/src/Services/UserService/UserService.Api"

# Verificar se os diretórios existem
if [ ! -d "$INFRASTRUCTURE_DIR" ]; then
    print_error "Diretório Infrastructure não encontrado: $INFRASTRUCTURE_DIR"
    exit 1
fi

if [ ! -d "$API_DIR" ]; then
    print_error "Diretório API não encontrado: $API_DIR"
    exit 1
fi

print_info "Criando migration '$MIGRATION_NAME'..."
print_info "Projeto Infrastructure: $INFRASTRUCTURE_DIR"
print_info "Projeto API (startup): $API_DIR"

# Navegar para o diretório Infrastructure
cd "$INFRASTRUCTURE_DIR"

# Executar o comando de criação da migration
print_info "Executando: dotnet ef migrations add $MIGRATION_NAME --startup-project $API_DIR"

if dotnet ef migrations add "$MIGRATION_NAME" --startup-project "$API_DIR"; then
    print_success "Migration '$MIGRATION_NAME' criada com sucesso!"
    print_info "Arquivos de migration criados em: $INFRASTRUCTURE_DIR/Migrations/"
    
    # Listar os arquivos de migration criados
    print_info "Arquivos criados:"
    ls -la Migrations/ | grep "$MIGRATION_NAME" || true
    
    print_warning "Lembre-se de revisar a migration antes de aplicá-la ao banco de dados."
    print_info "Para aplicar a migration, use: ./update-database.sh"
else
    print_error "Falha ao criar a migration '$MIGRATION_NAME'."
    print_info "Verifique os logs acima para mais detalhes."
    exit 1
fi

print_success "Processo concluído!"