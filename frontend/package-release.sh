#!/bin/bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
DIST_DIR="$ROOT_DIR/dist/ng-tailadmin"
PUBLISH_DIR="$DIST_DIR"
ARTIFACTS_DIR="$ROOT_DIR/artifacts"
VERSION="$(node -p "require('./package.json').version" 2>/dev/null || echo "0.0.0")"
TIMESTAMP="$(date +"%Y%m%d_%H%M%S")"
ZIP_NAME="meucatalogo-frontend-v${VERSION}-${TIMESTAMP}.zip"
ZIP_PATH="$ARTIFACTS_DIR/$ZIP_NAME"

cd "$ROOT_DIR"

echo "🚀 Gerando pacote de publicação do frontend"
echo "📦 Versão: $VERSION"
echo "🕒 Timestamp: $TIMESTAMP"

rm -rf "$DIST_DIR"
mkdir -p "$ARTIFACTS_DIR"

npm run build:prod
node build-info.js

if [ ! -d "$DIST_DIR" ]; then
  echo "❌ Build não encontrado em $DIST_DIR"
  exit 1
fi

if [ -d "$DIST_DIR/browser" ]; then
  PUBLISH_DIR="$DIST_DIR/browser"
fi

rm -f "$ZIP_PATH"

(
  cd "$PUBLISH_DIR"
  zip -r "$ZIP_PATH" . -x "*.DS_Store" "*/.DS_Store" "__MACOSX/*"
)

echo "✅ Pacote gerado com sucesso"
echo "📁 Arquivo: $ZIP_PATH"
