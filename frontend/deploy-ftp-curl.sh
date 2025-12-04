#!/bin/bash

# Script para deploy automático via FTP usando curl
# Versão alternativa mais compatível

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configurações FTP
FTP_HOST="198.50.203.165"
FTP_USER="9295_sanyz"
FTP_PASS="ugzxiohyenmjqpfrk4cw"
FTP_DIR="public_html"

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

print_header "🚀 DEPLOY VIA FTP (CURL)"

# Build
VERSION=$(node -p "require('./package.json').version" 2>/dev/null || echo "1.0.0")
DATE=$(date +"%Y%m%d_%H%M")
ZIP_NAME="meucatalogo-v${VERSION}-${DATE}.zip"

print_message "📦 Versão: $VERSION" $GREEN
print_message "📅 Data: $DATE" $GREEN
print_message "📁 Arquivo: $ZIP_NAME" $YELLOW

# Build
print_message "🏗️  Fazendo build..." $YELLOW
npm run build
node build-info.js

# Criar ZIP
print_message "🗜️  Criando ZIP..." $YELLOW
cd dist
zip -r "../$ZIP_NAME" ng-tailadmin/ -x "*.DS_Store" "*/.*"
cd ..

ZIP_SIZE=$(du -h "$ZIP_NAME" | cut -f1)
print_message "✅ ZIP criado: $ZIP_NAME ($ZIP_SIZE)" $GREEN

# Upload via curl
print_message "🌐 Fazendo upload via FTP..." $YELLOW

if curl -T "$ZIP_NAME" "ftp://$FTP_HOST/$FTP_DIR/" --user "$FTP_USER:$FTP_PASS"; then
    print_message "✅ Upload concluído!" $GREEN
else
    print_message "❌ Erro no upload" $RED
    exit 1
fi

# Limpeza
rm -f "$ZIP_NAME"
print_message "✅ Deploy concluído!" $GREEN
print_message "🌐 Acesse seu domínio para verificar" $BLUE
