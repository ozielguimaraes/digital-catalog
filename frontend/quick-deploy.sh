#!/bin/bash

# Script rápido para deploy - versão simplificada
# Uso: ./quick-deploy.sh

set -e

echo "🚀 Deploy Rápido - Meu Catálogo"
echo "================================"

# Obter versão e data
VERSION=$(node -p "require('./package.json').version" 2>/dev/null || echo "1.0.0")
DATE=$(date +"%Y%m%d")
ZIP_NAME="meucatalogo-v${VERSION}-${DATE}.zip"

echo "📦 Versão: $VERSION"
echo "📅 Data: $DATE"
echo "📁 Arquivo: $ZIP_NAME"
echo

# Build
echo "🏗️  Fazendo build..."
npm run build

# Gerar informações de versão
echo "📊 Gerando informações de versão..."
node build-info.js

# Criar ZIP
echo "🗜️  Criando ZIP..."
cd dist
zip -r "../$ZIP_NAME" ng-tailadmin/ -x "*.DS_Store" "*/.*"
cd ..

# Resultado
ZIP_SIZE=$(du -h "$ZIP_NAME" | cut -f1)
echo
echo "✅ Concluído!"
echo "📁 Arquivo: $ZIP_NAME ($ZIP_SIZE)"
echo "📍 Local: $(pwd)/$ZIP_NAME"
echo
echo "📋 Para hospedagem:"
echo "1. Upload do arquivo para o servidor"
echo "2. Descompactar na pasta wwwroot"
echo "3. Mover conteúdo de ng-tailadmin/ para a raiz"
