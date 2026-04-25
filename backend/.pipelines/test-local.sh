#!/bin/bash

# Script para testar build e testes localmente antes de commitar
# Executa os mesmos passos da pipeline de CI

echo "🚀 Iniciando testes locais de CI/CD..."

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configurações
BUILD_CONFIG="Release"
BACKEND_DIR="."
SOLUTION_FILE="$BACKEND_DIR/MeuCatalogo.sln"
API_PROJECT="$BACKEND_DIR/MeuCatalogo.API/MeuCatalogo.API.csproj"

echo -e "${YELLOW}📁 Diretório do backend: $BACKEND_DIR${NC}"
echo -e "${YELLOW}🔧 Configuração de build: $BUILD_CONFIG${NC}"
echo ""

# Função para verificar resultado
check_result() {
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ $1 concluído com sucesso${NC}"
    else
        echo -e "${RED}❌ $1 falhou${NC}"
        exit 1
    fi
}

# 1. Restaurar dependências
echo -e "${YELLOW}📦 Restaurando dependências...${NC}"
dotnet restore "$API_PROJECT"
check_result "Restauração de dependências"

# 2. Build do projeto
echo -e "${YELLOW}🔨 Executando build...${NC}"
dotnet build "$API_PROJECT" --configuration "$BUILD_CONFIG" --no-restore --verbosity normal
check_result "Build do projeto"

# 3. Executar testes (se existirem)
if find "$BACKEND_DIR" -name "*Tests.csproj" -type f | grep -q .; then
    echo -e "${YELLOW}🧪 Executando testes unitários...${NC}"
    dotnet test "$SOLUTION_FILE" --configuration "$BUILD_CONFIG" --no-build --logger "console;verbosity=detailed"
    check_result "Testes unitários"
else
    echo -e "${YELLOW}⚠️  Nenhum projeto de teste encontrado, pulando testes${NC}"
fi

# 4. Publicar projeto (simulando deploy)
echo -e "${YELLOW}📤 Publicando projeto...${NC}"
PUBLISH_DIR="./publish-test"
mkdir -p "$PUBLISH_DIR"

dotnet publish "$API_PROJECT" --configuration "$BUILD_CONFIG" --output "$PUBLISH_DIR" --verbosity quiet
check_result "Publicação do projeto"

# 5. Verificar arquivos publicados
echo -e "${YELLOW}📋 Verificando arquivos publicados...${NC}"
if [ -f "$PUBLISH_DIR/MeuCatalogo.API.dll" ]; then
    echo -e "${GREEN}✅ Arquivo principal encontrado: MeuCatalogo.API.dll${NC}"
else
    echo -e "${RED}❌ Arquivo principal não encontrado${NC}"
    exit 1
fi

# 6. Limpar diretório de teste
rm -rf "$PUBLISH_DIR"

echo ""
echo -e "${GREEN}🎉 Todos os testes passaram! Seu código está pronto para commit.${NC}"
echo -e "${YELLOW}💡 Dica: Faça commit e push para executar a pipeline de CI no Azure DevOps${NC}"