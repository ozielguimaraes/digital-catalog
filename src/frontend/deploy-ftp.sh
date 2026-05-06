#!/bin/bash

# Script para deploy automático via FTP
# Configurações do servidor FTP

set -e  # Para o script se houver erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configurações FTP
FTP_HOST="198.50.203.165"
FTP_USER="9295_sanyz"
FTP_PASS="ugzxiohyenmjqpfrk4cw"
FTP_DIR="/public_html"  # Diretório padrão para hospedagem compartilhada

# Função para exibir mensagens coloridas
print_message() {
    echo -e "${2}${1}${NC}"
}

# Função para exibir header
print_header() {
    echo
    print_message "===============================================" $BLUE
    print_message "$1" $BLUE
    print_message "===============================================" $BLUE
    echo
}

# Verificar se estamos no diretório correto
if [ ! -f "package.json" ]; then
    print_message "ERRO: Execute este script no diretório frontend da aplicação Angular" $RED
    exit 1
fi

print_header "🚀 DEPLOY AUTOMÁTICO VIA FTP"

# Obter versão e data
VERSION=$(node -p "require('./package.json').version" 2>/dev/null || echo "1.0.0")
DATE=$(date +"%Y%m%d_%H%M")
ZIP_NAME="meucatalogo-v${VERSION}-${DATE}.zip"

print_message "📦 Versão: $VERSION" $GREEN
print_message "📅 Data: $DATE" $GREEN
print_message "📁 Arquivo: $ZIP_NAME" $YELLOW
print_message "🌐 Servidor: $FTP_HOST" $BLUE

print_header "🔧 FAZENDO BUILD DE PRODUÇÃO"

# Limpar build anterior se existir
if [ -d "dist" ]; then
    print_message "🧹 Limpando build anterior..." $YELLOW
    rm -rf dist
fi

# Fazer build de produção
print_message "🏗️  Executando build de produção..." $YELLOW
npm run build

# Verificar se o build foi bem-sucedido
if [ ! -d "dist/ng-tailadmin" ]; then
    print_message "❌ ERRO: Build falhou! Diretório dist/ng-tailadmin não encontrado" $RED
    exit 1
fi

print_message "✅ Build concluído com sucesso!" $GREEN

print_header "📋 CRIANDO ARQUIVO DE VERSÃO"

# Gerar informações detalhadas do build
print_message "📊 Gerando informações do build..." $YELLOW
node build-info.js

print_message "📄 Arquivos de versão criados: version.txt e build-info.json" $GREEN

print_header "🗜️  CRIANDO ARQUIVO ZIP"

# Navegar para o diretório dist
cd dist

# Criar arquivo ZIP
print_message "📦 Criando arquivo ZIP..." $YELLOW
zip -r "../$ZIP_NAME" ng-tailadmin/ -x "*.DS_Store" "*/.*"

# Voltar para o diretório original
cd ..

# Verificar se o ZIP foi criado
if [ ! -f "$ZIP_NAME" ]; then
    print_message "❌ ERRO: Falha ao criar arquivo ZIP" $RED
    exit 1
fi

# Obter tamanho do arquivo
ZIP_SIZE=$(du -h "$ZIP_NAME" | cut -f1)
print_message "✅ Arquivo ZIP criado: $ZIP_NAME ($ZIP_SIZE)" $GREEN

print_header "🌐 CONECTANDO VIA FTP"

# Verificar se o lftp está instalado
if ! command -v lftp &> /dev/null; then
    print_message "❌ ERRO: lftp não está instalado. Instale com: brew install lftp" $RED
    exit 1
fi

print_message "🔌 Conectando ao servidor FTP..." $YELLOW

# Criar script FTP
FTP_SCRIPT=$(mktemp)
cat > "$FTP_SCRIPT" << EOF
open $FTP_HOST
user $FTP_USER $FTP_PASS
cd $FTP_DIR
put $ZIP_NAME
quit
EOF

# Executar upload via FTP
if lftp -f "$FTP_SCRIPT"; then
    print_message "✅ Upload do ZIP concluído com sucesso!" $GREEN
else
    print_message "❌ ERRO: Falha no upload via FTP" $RED
    rm -f "$FTP_SCRIPT"
    exit 1
fi

# Limpar script temporário
rm -f "$FTP_SCRIPT"

print_header "📦 DESCOMPACTANDO NO SERVIDOR"

# Criar script para descompactar no servidor
UNZIP_SCRIPT=$(mktemp)
cat > "$UNZIP_SCRIPT" << EOF
open $FTP_HOST
user $FTP_USER $FTP_PASS
cd $FTP_DIR
!unzip -o $ZIP_NAME
!mv ng-tailadmin/* ./
!rmdir ng-tailadmin
!rm $ZIP_NAME
quit
EOF

print_message "📂 Descompactando arquivos no servidor..." $YELLOW

# Executar descompactação via FTP
if lftp -f "$UNZIP_SCRIPT"; then
    print_message "✅ Descompactação concluída com sucesso!" $GREEN
else
    print_message "⚠️  Aviso: Descompactação automática falhou. Faça manualmente no painel de controle" $YELLOW
fi

# Limpar script temporário
rm -f "$UNZIP_SCRIPT"

print_header "🧹 LIMPEZA LOCAL"

# Remover arquivo ZIP local
rm -f "$ZIP_NAME"
print_message "🗑️  Arquivo ZIP local removido" $YELLOW

print_header "✅ DEPLOY CONCLUÍDO!"

print_message "🎉 Aplicação publicada com sucesso!" $GREEN
print_message "🌐 Servidor: $FTP_HOST" $BLUE
print_message "📁 Diretório: $FTP_DIR" $BLUE
print_message "📦 Versão: $VERSION" $BLUE

print_header "🔍 VERIFICAÇÃO"

print_message "Para verificar se o deploy foi bem-sucedido:" $YELLOW
print_message "1. Acesse seu domínio no navegador" $YELLOW
print_message "2. Verifique se a aplicação carrega corretamente" $YELLOW
print_message "3. Teste as funcionalidades (carrinho, navegação, etc.)" $YELLOW

print_message "🚀 Deploy automático finalizado!" $GREEN
