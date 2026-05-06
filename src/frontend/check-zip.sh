#!/bin/bash

# Script para verificar o conteúdo do arquivo ZIP gerado

echo "🔍 Verificando arquivo ZIP..."
echo "=============================="

# Encontrar o arquivo ZIP mais recente
ZIP_FILE=$(ls -t *.zip 2>/dev/null | head -n1)

if [ -z "$ZIP_FILE" ]; then
    echo "❌ Nenhum arquivo ZIP encontrado!"
    exit 1
fi

echo "📁 Arquivo: $ZIP_FILE"
echo "📏 Tamanho: $(du -h "$ZIP_FILE" | cut -f1)"
echo

echo "📋 Conteúdo do ZIP:"
echo "==================="
unzip -l "$ZIP_FILE" | head -20

echo
echo "📊 Resumo:"
echo "=========="
TOTAL_FILES=$(unzip -l "$ZIP_FILE" | tail -1 | awk '{print $2}')
echo "Total de arquivos: $TOTAL_FILES"

echo
echo "🔧 Arquivos essenciais:"
echo "======================="

# Verificar arquivos essenciais
unzip -l "$ZIP_FILE" | grep -E "(index\.html|web\.config|main-.*\.js|styles-.*\.css)" | while read line; do
    echo "✅ $line"
done

echo
echo "📄 Informações de versão:"
echo "========================="
unzip -p "$ZIP_FILE" "ng-tailadmin/version.txt" 2>/dev/null || echo "❌ Arquivo version.txt não encontrado"

echo
echo "🎯 Para hospedagem:"
echo "==================="
echo "1. Upload: $ZIP_FILE"
echo "2. Descompactar: unzip $ZIP_FILE"
echo "3. Mover: mv ng-tailadmin/* ./"
echo "4. Limpar: rmdir ng-tailadmin && rm $ZIP_FILE"
