# 🌐 Deploy Automático via FTP

Scripts para publicar a aplicação Angular diretamente no servidor via FTP.

## 📋 Configurações do Servidor

- **Host**: 198.50.203.165
- **Usuário**: 9295_sanyz
- **Senha**: ugzxiohyenmjqpfrk4cw
- **Diretório**: public_html

## 🚀 Scripts Disponíveis

### 1. Script Principal (Recomendado)

```bash
# Mostrar opções
./deploy.sh

# Deploy local (gerar ZIP)
./deploy.sh local

# Deploy via FTP (Python - mais confiável)
./deploy.sh python

# Testar conexão FTP
./deploy.sh test
```

### 2. Scripts Individuais

#### Deploy Local (ZIP)
```bash
./quick-deploy.sh
```

#### Deploy FTP com Python
```bash
./deploy-ftp.py
```

#### Deploy FTP com curl
```bash
./deploy-ftp-curl.sh
```

#### Deploy FTP com lftp
```bash
./deploy-ftp.sh
```

#### Testar Conexão
```bash
./test-ftp.py
```

## 🔧 Pré-requisitos

### Para Deploy Local
- Node.js e npm
- macOS/Linux

### Para Deploy FTP
- Python 3 (recomendado)
- Ou lftp: `brew install lftp`
- Ou curl (já incluído no macOS)

## 📦 O que Acontece no Deploy

1. **Build**: Compila a aplicação Angular
2. **ZIP**: Cria arquivo compactado com versão e data
3. **Upload**: Envia via FTP para o servidor
4. **Limpeza**: Remove arquivos temporários locais

## 🌐 Processo de Deploy

### Deploy Automático (Python)
```bash
./deploy.sh python
```

**O que faz:**
- ✅ Build de produção
- ✅ Cria ZIP com versão e data
- ✅ Upload via FTP
- ✅ Limpeza local

**O que você precisa fazer:**
- Descompactar o ZIP no servidor (via painel de controle)

### Deploy Manual
```bash
./deploy.sh local
```

**O que faz:**
- ✅ Build de produção
- ✅ Cria ZIP local

**O que você precisa fazer:**
- Upload manual do ZIP
- Descompactar no servidor

## 🔍 Verificar Deploy

### 1. Testar Conexão
```bash
./deploy.sh test
```

### 2. Verificar no Servidor
- Acesse seu domínio
- Verifique se a aplicação carrega
- Teste funcionalidades (carrinho, navegação)

## 🐛 Solução de Problemas

### Erro de Conexão FTP
```bash
# Testar conexão
./deploy.sh test

# Verificar credenciais
# Verificar se servidor está ativo
```

### Erro de Permissão
```bash
# Dar permissão de execução
chmod +x *.sh *.py
```

### Erro de Python
```bash
# Instalar dependências
pip3 install ftplib
```

### Erro de lftp
```bash
# Instalar lftp
brew install lftp
```

## 📊 Informações do Deploy

### Arquivo Gerado
- **Nome**: `meucatalogo-v{VERSÃO}-{DATA}.zip`
- **Tamanho**: ~9-10MB
- **Conteúdo**: Aplicação Angular completa + web.config

### Estrutura no Servidor
```
public_html/
├── index.html
├── web.config
├── *.js
├── *.css
├── assets/
└── version.txt
```

## 🔄 Atualizações

Para atualizar a aplicação:

1. **Faça as alterações** no código
2. **Execute o deploy**:
   ```bash
   ./deploy.sh python
   ```
3. **Descompacte no servidor** (via painel de controle)
4. **Teste a aplicação**

## 📞 Suporte

### Logs de Erro
- Verifique a saída do terminal
- Teste a conexão FTP primeiro
- Verifique as credenciais

### Comandos Úteis
```bash
# Ver opções
./deploy.sh help

# Testar conexão
./deploy.sh test

# Deploy local
./deploy.sh local

# Deploy automático
./deploy.sh python
```

---

**Nota**: O deploy automático via Python é o mais confiável. Use as outras opções apenas se necessário.
