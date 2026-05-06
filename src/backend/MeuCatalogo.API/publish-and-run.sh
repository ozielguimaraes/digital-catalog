#!/bin/bash

# Script completo: publicar e executar
echo "🚀 MeuCatalogo API - Publish and Run"
echo "====================================="

# Configurar ambiente de produção
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_DETAILEDERRORS="true"

# Verificar se .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET not found. Please install .NET 6.0 or later"
    exit 1
fi

# Limpar e publicar
echo "🧹 Cleaning previous build..."
rm -rf bin/Release/*

echo "🔨 Publishing application..."
dotnet publish -c Release -o bin/Release/net6.0/publish/

if [ $? -ne 0 ]; then
    echo "❌ Publish failed"
    exit 1
fi

echo "✅ Publish completed successfully"

# Copiar arquivos de configuração se não existirem
echo "📋 Copying configuration files..."
if [ ! -f "bin/Release/net6.0/publish/appsettings.json" ]; then
    echo "Copying appsettings.json..."
    cp appsettings.json bin/Release/net6.0/publish/
fi

if [ ! -f "bin/Release/net6.0/publish/appsettings.Production.json" ]; then
    echo "Copying appsettings.Production.json..."
    cp appsettings.Production.json bin/Release/net6.0/publish/
fi

# Copiar script de execução
echo "📜 Copying server run script..."
cp server-run.sh bin/Release/net6.0/publish/

echo "✅ Configuration files copied"

# Navegar para o diretório publicado
cd bin/Release/net6.0/publish/

echo "📁 Running from: $(pwd)"
echo "📋 Files in published directory:"
ls -la

# Verificar se o arquivo principal existe
if [ ! -f "MeuCatalogo.API.dll" ]; then
    echo "❌ MeuCatalogo.API.dll not found in published directory!"
    exit 1
fi

# Verificar arquivos de configuração
echo ""
echo "📋 Checking configuration files..."
if [ ! -f "appsettings.json" ]; then
    echo "❌ appsettings.json not found!"
    exit 1
fi

if [ ! -f "appsettings.Production.json" ]; then
    echo "⚠️  appsettings.Production.json not found, using appsettings.json"
else
    echo "✅ appsettings.Production.json found"
fi

# Executar a aplicação
echo ""
echo "✅ Starting published application..."
echo "📝 Logs will appear below:"
echo "================================"

dotnet MeuCatalogo.API.dll --environment Production --verbosity detailed
