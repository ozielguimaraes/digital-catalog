#!/bin/bash

# Script para executar no servidor de produção
# Assume que a aplicação já foi publicada e extraída

echo "🚀 MeuCatalogo API - Server Production Run"
echo "==========================================="

# Configurar ambiente de produção
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_DETAILEDERRORS="true"

# Verificar se .NET está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET not found. Please install .NET 6.0 or later"
    echo "Install instructions:"
    echo "  Ubuntu/Debian: sudo apt-get install dotnet-sdk-6.0"
    echo "  CentOS/RHEL: sudo yum install dotnet-sdk-6.0"
    echo "  macOS: brew install dotnet"
    exit 1
fi

# Verificar versão do .NET
echo "✅ .NET Version: $(dotnet --version)"

# Verificar se estamos no diretório correto
if [ ! -f "MeuCatalogo.API.dll" ]; then
    echo "❌ MeuCatalogo.API.dll not found!"
    echo "Current directory: $(pwd)"
    echo "Files in current directory:"
    ls -la
    echo ""
    echo "Please extract the published application and run from that directory."
    exit 1
fi

# Verificar arquivos de configuração
echo "📋 Checking configuration files..."
if [ ! -f "appsettings.json" ]; then
    echo "❌ appsettings.json not found!"
    echo "This file is required for the application to start."
    exit 1
fi

if [ ! -f "appsettings.Production.json" ]; then
    echo "⚠️  appsettings.Production.json not found, using appsettings.json"
else
    echo "✅ appsettings.Production.json found"
fi

# Verificar permissões
echo "🔐 Checking file permissions..."
if [ ! -r "MeuCatalogo.API.dll" ]; then
    echo "❌ No read permission for MeuCatalogo.API.dll"
    exit 1
fi

if [ ! -r "appsettings.json" ]; then
    echo "❌ No read permission for appsettings.json"
    exit 1
fi

echo "✅ File permissions OK"

# Criar diretório de logs se não existir
mkdir -p logs

# Executar a aplicação
echo ""
echo "✅ Starting MeuCatalogo API..."
echo "📁 Working directory: $(pwd)"
echo "📝 Logs will appear below:"
echo "================================"

# Executar com redirecionamento de logs
dotnet MeuCatalogo.API.dll --environment Production --verbosity detailed 2>&1 | tee -a server.log
