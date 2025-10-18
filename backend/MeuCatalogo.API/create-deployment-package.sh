#!/bin/bash

# Script para criar pacote de deploy completo
echo "🚀 MeuCatalogo API - Creating Deployment Package"
echo "================================================"

# Configurar variáveis
PACKAGE_NAME="MeuCatalogoAPI_$(date '+%Y%m%d_%H%M')"
PUBLISH_DIR="bin/Release/net6.0/publish"
PACKAGE_DIR="deployment-packages/$PACKAGE_NAME"

# Verificar se .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET not found. Please install .NET 6.0 or later"
    exit 1
fi

# Limpar builds anteriores
echo "🧹 Cleaning previous builds..."
rm -rf bin/Release/*
rm -rf deployment-packages/*

# Criar diretório de pacotes
mkdir -p deployment-packages

# Publicar aplicação
echo "🔨 Publishing application..."
dotnet publish -c Release -o $PUBLISH_DIR

if [ $? -ne 0 ]; then
    echo "❌ Publish failed"
    exit 1
fi

echo "✅ Publish completed successfully"

# Criar diretório do pacote
mkdir -p $PACKAGE_DIR

# Copiar arquivos da aplicação
echo "📦 Copying application files..."
cp -r $PUBLISH_DIR/* $PACKAGE_DIR/

# Copiar arquivos de configuração (garantir que existam)
echo "📋 Ensuring configuration files..."
cp appsettings.json $PACKAGE_DIR/
cp appsettings.Production.json $PACKAGE_DIR/

# Copiar scripts de execução
echo "📜 Copying execution scripts..."
cp server-run.sh $PACKAGE_DIR/
chmod +x $PACKAGE_DIR/server-run.sh

# Copiar web.config se existir
if [ -f "web.config" ]; then
    echo "🌐 Copying web.config..."
    cp web.config $PACKAGE_DIR/
fi

# Criar README para o servidor
echo "📝 Creating server README..."
cat > $PACKAGE_DIR/README-SERVER.md << 'EOF'
# MeuCatalogo API - Server Deployment

## Quick Start
```bash
./server-run.sh
```

## Requirements
- .NET 6.0 Runtime
- SQL Server access

## Configuration
- appsettings.json: Base configuration
- appsettings.Production.json: Production overrides

## Logs
- server.log: Application logs
- logs/: Detailed application logs

## Health Check
- HTTP: http://localhost:7000/health
- HTTPS: https://localhost:7001/health
- Swagger: https://localhost:7001/swagger

## Troubleshooting
1. Check .NET version: `dotnet --version`
2. Check file permissions: `ls -la MeuCatalogo.API.dll`
3. Check configuration: `cat appsettings.json`
4. Check logs: `tail -f server.log`
EOF

# Verificar conteúdo do pacote
echo "🔍 Verifying package contents..."
echo "Files in package:"
ls -la $PACKAGE_DIR/

echo ""
echo "Configuration files:"
ls -la $PACKAGE_DIR/appsettings*.json

echo ""
echo "Scripts:"
ls -la $PACKAGE_DIR/*.sh

# Criar ZIP do pacote
echo "📦 Creating deployment package..."
cd deployment-packages
zip -r "$PACKAGE_NAME.zip" "$PACKAGE_NAME/"

# Mover para Downloads
echo "📁 Moving package to Downloads..."
cp "$PACKAGE_NAME.zip" ~/Downloads/

echo ""
echo "✅ Deployment package created successfully!"
echo "📁 Package location: ~/Downloads/$PACKAGE_NAME.zip"
echo "📁 Package size: $(du -h ~/Downloads/$PACKAGE_NAME.zip | cut -f1)"
echo ""
echo "🚀 Ready for deployment!"
echo "1. Upload $PACKAGE_NAME.zip to your server"
echo "2. Extract: unzip $PACKAGE_NAME.zip"
echo "3. Run: cd $PACKAGE_NAME && ./server-run.sh"
