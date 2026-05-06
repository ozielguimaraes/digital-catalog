# 🍎 Executando em Produção no macOS

## Scripts Disponíveis

### 1. Script Simples (Recomendado)
```bash
./run-production.sh
```

### 2. Script de Debug Completo
```bash
./debug-production.sh
```

## Como Usar

### Opção 1: Execução Direta
```bash
# Navegar para o diretório
cd /Users/oziel/projects/digital-catalog/backend/MeuCatalogo.API

# Executar em produção
./run-production.sh
```

### Opção 2: Execução Manual
```bash
# Configurar ambiente
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_DETAILEDERRORS="true"

# Executar
dotnet MeuCatalogo.API.dll --environment Production --verbosity detailed
```

## O que os Scripts Fazem

### `run-production.sh`
- ✅ Verifica se .NET está instalado
- ✅ Compila o projeto se necessário
- ✅ Configura variáveis de ambiente
- ✅ Executa com logs detalhados

### `debug-production.sh`
- ✅ Todas as funcionalidades do `run-production.sh`
- ✅ Verifica arquivos de configuração
- ✅ Salva logs em `debug.log`
- ✅ Mostra informações detalhadas do sistema

## Logs Esperados

Se tudo funcionar, você verá:
```
🚀 Starting MeuCatalogo API in Production Mode...
🔨 Building project...
✅ Starting application...
📝 Logs will appear below:
================================
=== MEUCATALOGO API STARTUP ===
Environment: Production
✓ Connection string configured successfully.
✓ Database context configured successfully.
✓ JWT settings configured successfully.
✓ Application built successfully.
✓ Database migration completed successfully.
✓ Application configured successfully.
=== MEUCATALOGO API READY ===
```

## Troubleshooting

### Se der erro de permissão:
```bash
chmod +x run-production.sh
chmod +x debug-production.sh
```

### Se .NET não for encontrado:
```bash
# Instalar .NET 6.0
brew install dotnet
```

### Se a aplicação não compilar:
```bash
# Limpar e recompilar
dotnet clean
dotnet build --configuration Release
```

## Verificar se Está Funcionando

### 1. Health Check
```bash
curl http://localhost:7000/health
```

### 2. Swagger
```bash
open http://localhost:7000/swagger
```

### 3. Verificar Processos
```bash
ps aux | grep MeuCatalogo
```

## Parar a Aplicação

```bash
# Encontrar o processo
ps aux | grep MeuCatalogo

# Parar (substitua PID pelo número do processo)
kill PID
```

## Logs de Debug

Se usar `debug-production.sh`, os logs serão salvos em:
- `debug.log` - Log completo da execução
- Console - Logs em tempo real
