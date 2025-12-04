#!/bin/bash

# Script principal de deploy - escolha o método
# Uso: ./deploy.sh [opção]

set -e

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_message() {
    echo -e "${2}${1}${NC}"
}

print_header() {
    echo
    print_message "===============================================" $BLUE
    print_message "$1" $BLUE
    print_message "===============================================" $BLUE
    echo
}

show_help() {
    print_header "🚀 DEPLOY - MEU CATÁLOGO"
    echo
    print_message "Opções disponíveis:" $YELLOW
    echo
    print_message "1. local    - Gerar ZIP para upload manual" $GREEN
    print_message "2. ftp      - Deploy automático via FTP (lftp)" $GREEN
    print_message "3. curl     - Deploy via FTP usando curl" $GREEN
    print_message "4. python   - Deploy via FTP usando Python" $GREEN
    print_message "5. fixed    - Deploy via FTP (versão corrigida)" $GREEN
    print_message "6. direct   - Deploy direto via FTP (SEM ZIP)" $GREEN
    print_message "7. auto     - Deploy automático via FTP (ZIP + instruções)" $GREEN
    print_message "8. firebase - Deploy no Firebase Hosting (RECOMENDADO)" $GREEN
    print_message "9. test     - Testar conexão FTP" $GREEN
    echo
    print_message "Uso: ./deploy.sh [opção]" $BLUE
    echo
    print_message "Exemplos:" $YELLOW
    print_message "  ./deploy.sh local" $BLUE
    print_message "  ./deploy.sh python" $BLUE
    print_message "  ./deploy.sh test" $BLUE
}

# Verificar argumentos
if [ $# -eq 0 ]; then
    show_help
    exit 0
fi

case $1 in
    "local")
        print_message "🚀 Iniciando deploy local..." $YELLOW
        ./quick-deploy.sh
        ;;
    "ftp")
        print_message "🚀 Iniciando deploy via FTP (lftp)..." $YELLOW
        if command -v lftp &> /dev/null; then
            ./deploy-ftp.sh
        else
            print_message "❌ lftp não instalado. Instale com: brew install lftp" $RED
            print_message "💡 Ou use: ./deploy.sh python" $YELLOW
        fi
        ;;
    "curl")
        print_message "🚀 Iniciando deploy via FTP (curl)..." $YELLOW
        ./deploy-ftp-curl.sh
        ;;
    "python")
        print_message "🚀 Iniciando deploy via FTP (Python)..." $YELLOW
        python3 deploy-ftp.py
        ;;
    "fixed")
        print_message "🚀 Iniciando deploy via FTP (Versão Corrigida)..." $YELLOW
        python3 deploy-ftp-fixed.py
        ;;
    "direct")
        print_message "🚀 Iniciando deploy direto via FTP (SEM ZIP)..." $YELLOW
        python3 deploy-ftp-direct.py
        ;;
    "auto")
        print_message "🚀 Iniciando deploy automático via FTP (ZIP + instruções)..." $YELLOW
        python3 deploy-ftp-auto-unzip.py
        ;;
    "firebase")
        print_message "🔥 Iniciando deploy no Firebase..." $YELLOW
        ./deploy-firebase.sh
        ;;
    "test")
        print_message "🔍 Testando conexão FTP..." $YELLOW
        python3 test-ftp.py
        ;;
    "help"|"-h"|"--help")
        show_help
        ;;
    *)
        print_message "❌ Opção inválida: $1" $RED
        echo
        show_help
        exit 1
        ;;
esac
