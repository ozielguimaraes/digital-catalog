#!/bin/bash

# Script para debug em produção no macOS/Linux
echo "=== DEBUG PRODUCTION STARTUP ===" | tee -a debug.log

# Configurar variáveis de ambiente
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_DETAILEDERRORS="true"
export ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT="Information"
export ASPNETCORE_LOGGING__LOGLEVEL__MICROSOFT="Information"

echo "Environment Variables:" | tee -a debug.log
echo "ASPNETCORE_ENVIRONMENT: $ASPNETCORE_ENVIRONMENT" | tee -a debug.log
echo "ASPNETCORE_DETAILEDERRORS: $ASPNETCORE_DETAILEDERRORS" | tee -a debug.log

# Verificar se os arquivos existem
echo "" | tee -a debug.log
echo "Checking files..." | tee -a debug.log
files=(
    "MeuCatalogo.API.dll"
    "appsettings.json"
    "appsettings.Production.json"
)

for file in "${files[@]}"; do
    if [ -f "$file" ]; then
        echo "✓ $file exists" | tee -a debug.log
    else
        echo "✗ $file MISSING" | tee -a debug.log
    fi
done

# Verificar dependências
echo "" | tee -a debug.log
echo "Checking .NET runtime..." | tee -a debug.log
if command -v dotnet &> /dev/null; then
    dotnet_version=$(dotnet --version)
    echo "✓ .NET Version: $dotnet_version" | tee -a debug.log
else
    echo "✗ .NET not found" | tee -a debug.log
    echo "Please install .NET 6.0 or later" | tee -a debug.log
    exit 1
fi

# Verificar se o projeto está compilado
echo "" | tee -a debug.log
echo "Checking if project is built..." | tee -a debug.log
if [ ! -f "bin/Release/net6.0/MeuCatalogo.API.dll" ]; then
    echo "Project not built. Building..." | tee -a debug.log
    dotnet build --configuration Release
    if [ $? -ne 0 ]; then
        echo "✗ Build failed" | tee -a debug.log
        exit 1
    fi
    echo "✓ Build completed" | tee -a debug.log
fi

# Executar com logs detalhados
echo "" | tee -a debug.log
echo "Starting application with detailed logging..." | tee -a debug.log
echo "Press Ctrl+C to stop" | tee -a debug.log
echo "================================" | tee -a debug.log

# Criar diretório de logs se não existir
mkdir -p logs

# Executar a aplicação
echo "Starting MeuCatalogo API..." | tee -a debug.log
echo "Logs will be saved to debug.log" | tee -a debug.log
echo "" | tee -a debug.log

# Executar e capturar output
dotnet bin/Release/net6.0/MeuCatalogo.API.dll --environment Production --verbosity detailed 2>&1 | tee -a debug.log

# Verificar se a aplicação falhou
if [ ${PIPESTATUS[0]} -ne 0 ]; then
    echo "" | tee -a debug.log
    echo "=== APPLICATION FAILED ===" | tee -a debug.log
    echo "Check debug.log for detailed error information" | tee -a debug.log
    echo "Last 20 lines of debug.log:" | tee -a debug.log
    echo "================================" | tee -a debug.log
    tail -20 debug.log
else
    echo "" | tee -a debug.log
    echo "=== APPLICATION STOPPED ===" | tee -a debug.log
fi

echo "" | tee -a debug.log
echo "=== DEBUG SESSION ENDED ===" | tee -a debug.log
