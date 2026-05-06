#!/bin/bash

# Script para deploy da aplicação Angular no Firebase
# Instala Firebase CLI se necessário e faz deploy

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

print_header "🚀 DEPLOY FIREBASE - MEU CATÁLOGO"

# Verificar se o Firebase CLI está instalado
if ! command -v firebase &> /dev/null; then
    print_message "📦 Firebase CLI não encontrado. Instalando..." $YELLOW
    
    # Instalar Firebase CLI via npm
    if command -v npm &> /dev/null; then
        print_message "🔄 Instalando Firebase CLI globalmente..." $YELLOW
        npm install -g firebase-tools
        
        if [ $? -eq 0 ]; then
            print_message "✅ Firebase CLI instalado com sucesso!" $GREEN
        else
            print_message "❌ Erro ao instalar Firebase CLI" $RED
            exit 1
        fi
    else
        print_message "❌ npm não encontrado. Instale Node.js primeiro." $RED
        exit 1
    fi
else
    print_message "✅ Firebase CLI já está instalado" $GREEN
fi

# Verificar se está logado no Firebase
print_message "🔐 Verificando login no Firebase..." $YELLOW
if ! firebase projects:list &> /dev/null; then
    print_message "🔑 Fazendo login no Firebase..." $YELLOW
    firebase login
    
    if [ $? -ne 0 ]; then
        print_message "❌ Erro no login do Firebase" $RED
        exit 1
    fi
else
    print_message "✅ Já está logado no Firebase" $GREEN
fi

# Verificar se o projeto Firebase está inicializado
if [ ! -f "firebase.json" ]; then
    print_message "🔧 Inicializando projeto Firebase..." $YELLOW
    
    # Perguntar se quer inicializar
    read -p "Deseja inicializar um novo projeto Firebase? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        firebase init hosting
        
        if [ $? -ne 0 ]; then
            print_message "❌ Erro ao inicializar projeto Firebase" $RED
            exit 1
        fi
    else
        print_message "❌ Projeto Firebase não inicializado. Execute 'firebase init hosting' primeiro." $RED
        exit 1
    fi
else
    print_message "✅ Projeto Firebase já está inicializado" $GREEN
fi

# Build da aplicação
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

# Configurar Firebase Hosting
print_header "⚙️  CONFIGURANDO FIREBASE HOSTING"

# Verificar se firebase.json existe e está configurado corretamente
if [ -f "firebase.json" ]; then
    print_message "📄 Verificando configuração do firebase.json..." $YELLOW
    
    # Verificar se a configuração está correta
    if grep -q "dist/ng-tailadmin" firebase.json; then
        print_message "✅ Configuração do firebase.json está correta" $GREEN
    else
        print_message "⚠️  Atualizando configuração do firebase.json..." $YELLOW
        
        # Criar/atualizar firebase.json
        cat > firebase.json << EOF
{
  "hosting": {
    "public": "dist/ng-tailadmin/browser",
    "ignore": [
      "firebase.json",
      "**/.*",
      "**/node_modules/**"
    ],
    "rewrites": [
      {
        "source": "**",
        "destination": "/index.html"
      }
    ],
    "headers": [
      {
        "source": "**/*.@(js|css)",
        "headers": [
          {
            "key": "Cache-Control",
            "value": "max-age=31536000"
          }
        ]
      }
    ]
  }
}
EOF
        print_message "✅ firebase.json atualizado" $GREEN
    fi
else
    print_message "📄 Criando firebase.json..." $YELLOW
    
    cat > firebase.json << EOF
{
  "hosting": {
    "public": "dist/ng-tailadmin/browser",
    "ignore": [
      "firebase.json",
      "**/.*",
      "**/node_modules/**"
    ],
    "rewrites": [
      {
        "source": "**",
        "destination": "/index.html"
      }
    ],
    "headers": [
      {
        "source": "**/*.@(js|css)",
        "headers": [
          {
            "key": "Cache-Control",
            "value": "max-age=31536000"
          }
        ]
      }
    ]
  }
}
EOF
    print_message "✅ firebase.json criado" $GREEN
fi

# Deploy no Firebase
print_header "🌐 FAZENDO DEPLOY NO FIREBASE"

print_message "🚀 Iniciando deploy..." $YELLOW

# Fazer deploy
firebase deploy --only hosting

if [ $? -eq 0 ]; then
    print_message "🎉 Deploy concluído com sucesso!" $GREEN
    
    # Obter URL do projeto
    PROJECT_URL=$(firebase hosting:channel:list 2>/dev/null | grep "live" | awk '{print $2}' || echo "https://seu-projeto.web.app")
    
    print_header "✅ DEPLOY CONCLUÍDO!"
    print_message "🌐 Aplicação publicada no Firebase!" $GREEN
    print_message "🔗 URL: $PROJECT_URL" $BLUE
    print_message "📱 Aplicação PWA pronta para uso!" $GREEN
    
    print_message "\n📋 Funcionalidades disponíveis:" $YELLOW
    print_message "✅ HTTPS automático" $GREEN
    print_message "✅ CDN global" $GREEN
    print_message "✅ Cache otimizado" $GREEN
    print_message "✅ SPA routing" $GREEN
    print_message "✅ PWA support" $GREEN
    
else
    print_message "❌ Erro no deploy do Firebase" $RED
    exit 1
fi

print_message "\n🚀 Deploy Firebase finalizado!" $GREEN
