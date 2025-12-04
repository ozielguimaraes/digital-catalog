#!/bin/bash

# Script para deploy da aplicação Angular no macOS
# Gera um arquivo ZIP com versão e data para hospedagem

set -e  # Para o script se houver erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

print_header "🚀 DEPLOY ANGULAR - macOS"

# Obter versão do package.json
VERSION=$(node -p "require('./package.json').version" 2>/dev/null || echo "1.0.0")
print_message "📦 Versão detectada: $VERSION" $GREEN

# Obter data atual no formato YYYYMMDD
DATE=$(date +"%Y%m%d")
print_message "📅 Data: $DATE" $GREEN

# Nome do arquivo ZIP
ZIP_NAME="meucatalogo-v${VERSION}-${DATE}.zip"
print_message "📁 Nome do arquivo: $ZIP_NAME" $YELLOW

print_header "🔧 FAZENDO BUILD DE PRODUÇÃO"

# Limpar build anterior se existir
if [ -d "dist" ]; then
    print_message "🧹 Limpando build anterior..." $YELLOW
    rm -rf dist
fi

# Instalar dependências se necessário
if [ ! -d "node_modules" ]; then
    print_message "📥 Instalando dependências..." $YELLOW
    npm install
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
zip -r "../$ZIP_NAME" ng-tailadmin/ -x "*.DS_Store" "*/.*" "*/node_modules/*"

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

print_header "📊 RESUMO DO DEPLOY"

print_message "📁 Arquivo gerado: $ZIP_NAME" $BLUE
print_message "📏 Tamanho: $ZIP_SIZE" $BLUE
print_message "📂 Localização: $(pwd)/$ZIP_NAME" $BLUE
print_message "🌐 Conteúdo: Aplicação Angular pronta para hospedagem" $BLUE

print_header "📋 INSTRUÇÕES PARA HOSPEDAGEM"

print_message "1. Faça upload do arquivo '$ZIP_NAME' para seu servidor" $YELLOW
print_message "2. Acesse o painel de controle da hospedagem" $YELLOW
print_message "3. Navegue até a pasta wwwroot (ou public_html)" $YELLOW
print_message "4. Descompacte o arquivo ZIP" $YELLOW
print_message "5. Mova o conteúdo da pasta 'ng-tailadmin' para a raiz do wwwroot" $YELLOW
print_message "6. Certifique-se de que o arquivo web.config está na raiz" $YELLOW
print_message "7. Teste o acesso ao site" $YELLOW

print_header "🔧 COMANDOS PARA HOSPEDAGEM (Linux/Windows)"

print_message "No servidor, execute:" $BLUE
echo "unzip $ZIP_NAME"
echo "mv ng-tailadmin/* ./"
echo "rmdir ng-tailadmin"
echo "rm $ZIP_NAME"

print_header "✅ DEPLOY CONCLUÍDO!"

print_message "🎉 Arquivo pronto para hospedagem!" $GREEN
print_message "📁 Localização: $(pwd)/$ZIP_NAME" $GREEN

# Abrir pasta no Finder (opcional)
read -p "Deseja abrir a pasta no Finder? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    open .
fi

print_message "🚀 Boa sorte com o deploy!" $GREEN
