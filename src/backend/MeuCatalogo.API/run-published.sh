#!/bin/bash

# Script para executar aplicação publicada
echo "🚀 Starting MeuCatalogo API (Published Version)..."

# Configurar ambiente de produção
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_DETAILEDERRORS="true"

# Verificar se .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET not found. Please install .NET 6.0 or later"
    exit 1
fi

# Verificar se a aplicação foi publicada
if [ ! -f "MeuCatalogo.API.dll" ]; then
    echo "❌ Published application not found!"
    echo "Expected: MeuCatalogo.API.dll in current directory"
    echo "Current directory: $(pwd)"
    echo "Files in current directory:"
    ls -la
    echo ""
    echo "Please run from the published directory:"
    echo "cd bin/Release/net6.0/publish/"
    exit 1
fi

# Verificar arquivos de configuração
echo "📋 Checking configuration files..."
if [ ! -f "appsettings.json" ]; then
    echo "❌ appsettings.json not found!"
    exit 1
fi

if [ ! -f "appsettings.Production.json" ]; then
    echo "⚠️  appsettings.Production.json not found, using appsettings.json"
fi

echo "✅ Configuration files found"

# Executar a aplicação
echo "✅ Starting published application..."
echo "📝 Logs will appear below:"
echo "================================"

dotnet MeuCatalogo.API.dll --environment Production --verbosity detailed
