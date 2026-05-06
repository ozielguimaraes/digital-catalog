# Configuração de Variáveis e Secrets para Azure DevOps

## 1. Configurar Service Connection

1. No Azure DevOps, vá para Project Settings → Service connections
2. Clique em "New service connection" → Azure Resource Manager
3. Selecione "Service principal (automatic)" ou "Service principal (manual)"
4. Preencha os dados:
   - Subscription Id: b42a60f5-3c02-4a26-851c-36d2273fb56a
   - Service principal name: GTech
   - Resource Group: Catalogo

## 2. Configurar Variáveis de Pipeline

### Pipeline de CI (ci-pipeline.yml)
Nenhuma variável adicional necessária - todas estão hardcoded no YAML.

### Pipeline de CD (cd-pipeline.yml)
As variáveis já estão configuradas no YAML, mas você pode criar um grupo de variáveis se preferir:

1. Vá para Pipelines → Library → Variable groups
2. Crie um novo grupo chamado "prod-variables"
3. Adicione as seguintes variáveis:
   ```
   azureSubscription = GTech(b42a60f5-3c02-4a26-851c-36d2273fb56a)
   appName = catalogo-api
   resourceGroupName = Catalogo
   slotName = production
   runtimeStack = DOTNETCORE|6.0
   ```

## 3. Configurar Secrets (se necessário)

Se houver secrets sensíveis (connection strings, API keys, etc.):

1. Vá para Pipelines → Library → Variable groups
2. Crie um grupo chamado "prod-secrets"
3. Marque as variáveis como "Keep this value secret"
4. Adicione ao seu appsettings.Production.json:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "#{SqlConnection}#"
     },
     "Azure": {
       "Storage": {
         "ConnectionString": "#{AzureStorageConnection}#"
       }
     }
   }
   ```

## 4. Configurar Ambientes

1. Vá para Pipelines → Environments
2. Crie um ambiente chamado "production"
3. Configure aprovações se necessário (para produção)
4. Adicione checks de segurança

## 5. Configurar Pipeline Triggers

### CI Pipeline
- Trigger: Todas as branches (push)
- PR: main e develop
- Path filter: backend/*

### CD Pipeline  
- Trigger: Após CI bem-sucedido na main
- No manual trigger
- Ambiente: production

## 6. Testar Pipelines

1. Faça um push para qualquer branch → CI deve executar
2. Faça um PR para main → CI deve executar
3. Faça merge na main → CI + CD devem executar

## 7. Monitoramento

- Ative logs detalhados nas pipelines
- Configure alertas para falhas
- Monitore o Azure Application Insights
- Configure dashboards no Azure Portal

## 8. Rollback (se necessário)

Para fazer rollback de um deploy:

1. Vá para Pipelines → Releases
2. Encontre a release anterior bem-sucedida
3. Clique em "Redeploy"
4. Selecione o slot de staging
5. Faça swap com production