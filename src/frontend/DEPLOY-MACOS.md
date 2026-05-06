# 🚀 Deploy da Aplicação Angular - macOS

Este guia explica como gerar um pacote de deploy da aplicação Angular no macOS para hospedagem.

## 📋 Pré-requisitos

- macOS (qualquer versão)
- Node.js e npm instalados
- Terminal (aplicação padrão do macOS)

## 🚀 Deploy Rápido

### Opção 1: Script Completo (Recomendado)

```bash
# Execute no terminal dentro da pasta frontend
./deploy-macos.sh
```

### Opção 2: Deploy Rápido

```bash
# Para deploy mais rápido
./quick-deploy.sh
```

### Opção 3: Deploy Manual

```bash
# 1. Fazer build
npm run build

# 2. Gerar informações do build
node build-info.js

# 3. Criar ZIP manualmente
cd dist
zip -r "../meucatalogo-$(date +%Y%m%d).zip" ng-tailadmin/ -x "*.DS_Store" "*/.*"
cd ..
```

## 📦 O que é Gerado

Após executar o script, você terá:

```
meucatalogo-v1.0.0-20241219.zip
├── ng-tailadmin/
│   ├── index.html
│   ├── web.config
│   ├── version.txt
│   ├── build-info.json
│   ├── *.js (arquivos JavaScript)
│   ├── *.css (arquivos de estilo)
│   └── assets/ (imagens e recursos)
```

## 📊 Informações do Build

O script gera automaticamente:

- **version.txt**: Informações legíveis do build
- **build-info.json**: Dados estruturados para integração
- **Arquivo ZIP**: Nomeado com versão e data (ex: `meucatalogo-v1.0.0-20241219.zip`)

## 🌐 Deploy na Hospedagem

### 1. Upload do Arquivo

- Faça upload do arquivo ZIP para seu servidor
- Use FTP, SFTP ou painel de controle da hospedagem

### 2. Descompactar

```bash
# No servidor (Linux/Windows)
unzip meucatalogo-v1.0.0-20241219.zip
```

### 3. Mover Arquivos

```bash
# Mover conteúdo para a raiz do wwwroot
mv ng-tailadmin/* ./
rmdir ng-tailadmin
rm meucatalogo-v1.0.0-20241219.zip
```

### 4. Verificar

- Acesse seu domínio
- Verifique se a aplicação carrega corretamente
- Teste as funcionalidades (carrinho, navegação, etc.)

## 🔧 Configurações da Hospedagem

### Para Hospedagem Windows (IIS)

- O arquivo `web.config` já está incluído
- Configure o site no IIS Manager
- Defina o Application Pool para .NET Framework

### Para Hospedagem Linux (Apache/Nginx)

- Configure redirecionamento para `index.html`
- Habilite compressão gzip
- Configure cache para arquivos estáticos

## 📋 Exemplo de Uso

```bash
# 1. Navegar para a pasta frontend
cd /caminho/para/frontend

# 2. Executar deploy
./deploy-macos.sh

# 3. Aguardar conclusão
# ✅ Build concluído com sucesso!
# ✅ Arquivo ZIP criado: meucatalogo-v1.0.0-20241219.zip (2.5MB)

# 4. Upload para hospedagem
# 5. Descompactar no servidor
# 6. Testar aplicação
```

## 🐛 Solução de Problemas

### Erro: "Permission denied"

```bash
# Dar permissão de execução
chmod +x deploy-macos.sh
chmod +x quick-deploy.sh
```

### Erro: "npm not found"

```bash
# Instalar Node.js
brew install node
# ou baixar de https://nodejs.org
```

### Erro: "Build failed"

```bash
# Limpar cache e reinstalar
rm -rf node_modules package-lock.json
npm install
npm run build
```

### Arquivo ZIP muito grande

- Verifique se não há arquivos desnecessários
- Use compressão máxima: `zip -9`
- Considere usar CDN para arquivos estáticos

## 📊 Monitoramento

### Verificar Build

```bash
# Ver informações do build
cat dist/ng-tailadmin/version.txt
cat dist/ng-tailadmin/build-info.json
```

### Verificar Tamanho

```bash
# Ver tamanho do ZIP
ls -lh *.zip

# Ver tamanho do conteúdo
du -sh dist/ng-tailadmin/
```

## 🔄 Atualizações

Para atualizar a aplicação:

1. Faça as alterações no código
2. Execute o script de deploy novamente
3. Faça upload do novo arquivo ZIP
4. Substitua os arquivos na hospedagem

## 📞 Suporte

Se encontrar problemas:

1. Verifique os logs do terminal
2. Confirme se todas as dependências estão instaladas
3. Teste o build localmente com `ng serve`
4. Verifique as configurações da hospedagem

---

**Nota**: Este processo é otimizado para hospedagem compartilhada e VPS. Para ambientes enterprise, considere usar CI/CD com GitHub Actions ou Azure DevOps.
