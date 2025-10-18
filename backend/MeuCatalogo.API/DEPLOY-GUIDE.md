# 🚀 Guia de Deploy - MeuCatalogo API

## Cenários de Deploy

### 1. Desenvolvimento Local (Teste)
```bash
# Executar diretamente do código fonte
./run-production.sh
```

### 2. Publicação e Teste Local
```bash
# Publicar e executar localmente
./publish-and-run.sh
```

### 3. Deploy no Servidor de Produção

#### Passo 1: Preparar o Pacote (Recomendado)
```bash
# No seu Mac (desenvolvimento) - Use o script automático
cd ~/projects/digital-catalog/backend/MeuCatalogo.API
./create-deployment-package.sh
```

#### Passo 1: Preparar o Pacote (Manual)
```bash
# No seu Mac (desenvolvimento) - Método manual
cd ~/projects/digital-catalog/backend/MeuCatalogo.API
rm -rf bin/Release/*
dotnet publish -c Release -o bin/Release/net6.0/publish/
cd bin/Release/net6.0/publish/

# IMPORTANTE: Copiar arquivos de configuração
cp ../../../appsettings.json .
cp ../../../appsettings.Production.json .

# Criar ZIP
find . -type f -mtime -1 -print | zip "MeuCatalogoAPI_$(date '+%Y%m%d_%H%M').zip" -@
cp "MeuCatalogoAPI_$(date '+%Y%m%d_%H%M').zip" ~/Downloads/
```

#### Passo 2: No Servidor de Produção
```bash
# 1. Extrair o arquivo ZIP
unzip MeuCatalogoAPI_YYYYMMDD_HHMM.zip

# 2. Navegar para o diretório extraído
cd MeuCatalogoAPI_YYYYMMDD_HHMM/

# 3. Executar a aplicação
./server-run.sh
```

## Scripts Disponíveis

### `run-production.sh`
- **Uso**: Desenvolvimento local
- **Requisitos**: Código fonte compilado
- **Executa**: `dotnet bin/Release/net6.0/MeuCatalogo.API.dll`

### `publish-and-run.sh`
- **Uso**: Teste local da versão publicada
- **Requisitos**: Código fonte
- **Faz**: Publish + Execução

### `run-published.sh`
- **Uso**: Executar aplicação já publicada
- **Requisitos**: Diretório com arquivos publicados
- **Executa**: `dotnet MeuCatalogo.API.dll`

### `server-run.sh`
- **Uso**: Servidor de produção
- **Requisitos**: Aplicação publicada + .NET Runtime
- **Faz**: Verificações + Execução + Logs

## Estrutura de Arquivos Publicados

```
MeuCatalogoAPI_YYYYMMDD_HHMM/
├── MeuCatalogo.API.dll          # Aplicação principal
├── MeuCatalogo.Application.dll  # Biblioteca de aplicação
├── appsettings.json             # Configuração base
├── appsettings.Production.json  # Configuração de produção
├── server-run.sh               # Script de execução
├── logs/                       # Diretório de logs
└── [outras dependências...]
```

## Troubleshooting

### Erro: "appsettings.json not found!"

**Causa**: Arquivos de configuração não foram copiados durante a publicação
**Solução Rápida**:
```bash
# No diretório publicado, execute:
./fix-published-app.sh
```

**Solução Manual**:
```bash
# Copiar arquivos de configuração do diretório de desenvolvimento
cp ../../../appsettings.json .
cp ../../../appsettings.Production.json .
```

### Erro: "Could not execute because the specified command or file was not found"

**Causa**: Script executando do diretório errado
**Solução**: 
```bash
# Verificar se está no diretório correto
ls -la MeuCatalogo.API.dll

# Se não estiver, navegar para o diretório publicado
cd bin/Release/net6.0/publish/
```

### Erro: "Connection string not configured"

**Causa**: Arquivo de configuração não encontrado
**Solução**:
```bash
# Verificar se os arquivos de configuração existem
ls -la appsettings*.json

# Se não existirem, copiar do diretório de desenvolvimento
cp ../../../appsettings*.json .
```

### Erro: "Database migration failed"

**Causa**: Banco de dados inacessível
**Solução**:
1. Verificar connection string
2. Verificar se o servidor de banco está rodando
3. Verificar permissões de acesso

## Logs de Debug

### No Servidor
```bash
# Ver logs em tempo real
tail -f server.log

# Ver logs da aplicação
tail -f logs/log-$(date +%Y%m%d).txt
```

### Verificar Status
```bash
# Verificar se a aplicação está rodando
ps aux | grep MeuCatalogo

# Verificar portas
netstat -tlnp | grep :7000
netstat -tlnp | grep :7001
```

## Health Check

```bash
# Testar se a aplicação está funcionando
curl http://localhost:7000/health
curl https://localhost:7001/health
```

## Próximos Passos

1. **Teste local**: Use `./publish-and-run.sh`
2. **Deploy**: Siga o processo de publicação
3. **Monitoramento**: Use `server-run.sh` no servidor
4. **Logs**: Monitore `server.log` e `logs/`
