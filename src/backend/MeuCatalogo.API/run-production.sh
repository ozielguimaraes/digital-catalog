#!/bin/bash

# Script simples para executar em produção
echo "🚀 Starting MeuCatalogo API in Production Mode..."

# Configurar ambiente de produção
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_DETAILEDERRORS="true"

# Verificar se .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET not found. Please install .NET 6.0 or later"
    exit 1
fi

# Verificar se o projeto está compilado
if [ ! -f "bin/Release/net6.0/MeuCatalogo.API.dll" ]; then
    echo "🔨 Building project..."
    dotnet build --configuration Release
    if [ $? -ne 0 ]; then
        echo "❌ Build failed"
        exit 1
    fi
fi

# Executar a aplicação
echo "✅ Starting application..."
echo "📝 Logs will appear below:"
echo "================================"

dotnet bin/Release/net6.0/MeuCatalogo.API.dll --environment Production --verbosity detailed
