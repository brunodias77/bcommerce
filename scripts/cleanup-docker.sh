#!/bin/bash

# =============================================================================
# Docker Cleanup Script (sem remover imagens)
# =============================================================================
# Este script remove TODOS os containers, volumes e redes Docker não utilizadas.
# As imagens Docker serão mantidas.
# =============================================================================

set -e  # Sair em caso de erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Funções utilitárias
print_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

confirm() {
    read -p "$1 (y/N): " -n 1 -r
    echo
    [[ $REPLY =~ ^[Yy]$ ]]
}

# Banner
echo -e "${RED}"
echo "=============================================================================="
echo "                        DOCKER CLEANUP SCRIPT"
echo "=============================================================================="
echo -e "${NC}"
print_warning "Este script irá remover TODOS os recursos Docker, exceto imagens:"
echo "  • Todos os containers (em execução e parados)"
echo "  • Todos os volumes Docker"
echo "  • Todas as redes Docker não utilizadas"
echo "  • Cache do Docker (sem afetar imagens)"
echo
print_warning "Volumes e containers serão perdidos permanentemente!"
echo

if ! confirm "Tem certeza que deseja continuar?"; then
    print_info "Operação cancelada pelo usuário."
    exit 0
fi

echo
print_info "Iniciando limpeza do Docker (sem apagar imagens)..."
echo

# 1. Parar todos os containers
print_info "Parando todos os containers em execução..."
if [ "$(docker ps -q)" ]; then
    docker stop $(docker ps -q)
    print_success "Containers parados com sucesso."
else
    print_info "Nenhum container em execução encontrado."
fi
echo

# 2. Remover todos os containers
print_info "Removendo todos os containers..."
if [ "$(docker ps -aq)" ]; then
    docker rm $(docker ps -aq)
    print_success "Containers removidos com sucesso."
else
    print_info "Nenhum container encontrado para remoção."
fi
echo

# 3. NÃO remover imagens
print_info "Mantendo todas as imagens Docker."
echo

# 4. Remover volumes
print_info "Removendo todos os volumes Docker..."
if [ "$(docker volume ls -q)" ]; then
    docker volume rm $(docker volume ls -q) --force
    print_success "Volumes removidos com sucesso."
else
    print_info "Nenhum volume encontrado para remoção."
fi
echo

# 5. Remover redes não utilizadas
print_info "Removendo redes Docker não utilizadas..."
docker network prune --force
print_success "Redes não utilizadas removidas com sucesso."
echo

# 6. Limpar cache (sem remover imagens)
print_info "Limpando cache do Docker (sem remover imagens)..."
docker system prune --force
print_success "Cache do Docker limpo com sucesso."
echo

# 7. Estatísticas
print_info "Estatísticas atuais do Docker:"
docker system df
echo

print_success "Limpeza finalizada com sucesso!"
print_info "Containers, volumes, redes e cache removidos. Imagens foram mantidas."
