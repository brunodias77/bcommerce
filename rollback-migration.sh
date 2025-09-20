#!/bin/bash

# Script para reverter migrations do Entity Framework
# Uso: ./rollback-migration.sh [nome-da-migration-destino]
# Se não especificar migration, reverte para a migration anterior

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

# Navegar para o diretório Infrastructure
cd "$INFRASTRUCTURE_DIR"

# Listar migrations disponíveis
print_info "Listando migrations disponíveis..."
dotnet ef migrations list --startup-project "$API_DIR" || {
    print_error "Falha ao listar migrations. Verifique a configuração do projeto."
    exit 1
}

echo ""

# Determinar migration de destino
if [ -z "$1" ]; then
    print_warning "Nenhuma migration de destino especificada."
    print_info "Isso irá reverter para a migration anterior."
    
    # Confirmar ação
    read -p "Deseja continuar? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "Operação cancelada pelo usuário."
        exit 0
    fi
    
    TARGET_MIGRATION=""
    ACTION_DESC="para a migration anterior"
else
    TARGET_MIGRATION="$1"
    ACTION_DESC="para a migration '$TARGET_MIGRATION'"
    
    print_info "Migration de destino: $TARGET_MIGRATION"
    
    # Confirmar ação
    read -p "Deseja reverter $ACTION_DESC? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "Operação cancelada pelo usuário."
        exit 0
    fi
fi

print_warning "ATENÇÃO: Esta operação irá modificar o banco de dados!"
print_info "Revertendo $ACTION_DESC..."

# Executar o rollback
if [ -z "$TARGET_MIGRATION" ]; then
    # Reverter para migration anterior (sem especificar target)
    print_info "Executando: dotnet ef database update --startup-project $API_DIR"
    
    # Obter a migration anterior
    CURRENT_MIGRATIONS=$(dotnet ef migrations list --startup-project "$API_DIR" --no-build 2>/dev/null | grep -E '^[0-9]' | tail -2 | head -1 || echo "")
    
    if [ -n "$CURRENT_MIGRATIONS" ]; then
        PREVIOUS_MIGRATION=$(echo "$CURRENT_MIGRATIONS" | awk '{print $2}')
        print_info "Revertendo para: $PREVIOUS_MIGRATION"
        
        if dotnet ef database update "$PREVIOUS_MIGRATION" --startup-project "$API_DIR"; then
            print_success "Rollback realizado com sucesso para '$PREVIOUS_MIGRATION'!"
        else
            print_error "Falha ao realizar rollback."
            exit 1
        fi
    else
        print_error "Não foi possível determinar a migration anterior."
        print_info "Use: $0 <nome-da-migration> para especificar o destino."
        exit 1
    fi
else
    # Reverter para migration específica
    print_info "Executando: dotnet ef database update $TARGET_MIGRATION --startup-project $API_DIR"
    
    if dotnet ef database update "$TARGET_MIGRATION" --startup-project "$API_DIR"; then
        print_success "Rollback realizado com sucesso para '$TARGET_MIGRATION'!"
    else
        print_error "Falha ao realizar rollback para '$TARGET_MIGRATION'."
        print_info "Verifique se a migration '$TARGET_MIGRATION' existe."
        exit 1
    fi
fi

print_info "Estado atual das migrations:"
dotnet ef migrations list --startup-project "$API_DIR" || true

print_success "Processo de rollback concluído!"
print_warning "Verifique se o banco de dados está no estado esperado."