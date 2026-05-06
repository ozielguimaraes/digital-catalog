# Deploy da Aplicação Angular para IIS

Este guia explica como publicar a aplicação Angular no Internet Information Services (IIS).

## 📋 Pré-requisitos

- Windows Server ou Windows 10/11 com IIS instalado
- Node.js e npm instalados
- Privilégios de administrador
- .NET Framework 4.0 ou superior

## 🚀 Passos para Deploy

### 1. Build da Aplicação

```bash
# Navegue para o diretório frontend
cd frontend

# Instale as dependências (se necessário)
npm install

# Faça o build de produção
npm run build
```

### 2. Opção A: Deploy Automático (PowerShell)

Execute o script PowerShell como Administrador:

```powershell
# Execute no PowerShell como Administrador
.\deploy-to-iis.ps1 -SiteName "MeuCatalogo"
```

### 3. Opção B: Deploy Manual

1. **Copie os arquivos**:
   - Copie todo o conteúdo da pasta `dist/ng-tailadmin/` para o diretório do IIS
   - Exemplo: `C:\inetpub\wwwroot\MeuCatalogo\`

2. **Configure o IIS**:
   - Abra o IIS Manager
   - Crie um novo site ou configure um existente
   - Defina o caminho físico para a pasta onde copiou os arquivos
   - Configure a porta (padrão: 80)

3. **Configure o Application Pool**:
   - Crie um novo Application Pool
   - Defina o .NET Framework version para v4.0 ou superior
   - Defina o Managed Pipeline Mode para "Integrated"

### 4. Opção C: Deploy com Script Batch

Execute o arquivo `deploy-to-iis.bat` como Administrador:

```cmd
deploy-to-iis.bat
```

## ⚙️ Configurações Importantes

### web.config

O arquivo `web.config` já está incluído no build e contém:

- **URL Rewriting**: Redireciona todas as rotas para `index.html` (necessário para SPA)
- **MIME Types**: Configuração correta para arquivos estáticos
- **Cache**: Otimização de cache para arquivos estáticos
- **Compressão**: Habilita compressão gzip
- **Segurança**: Headers de segurança básicos

### Estrutura de Arquivos

```
C:\inetpub\wwwroot\MeuCatalogo\
├── index.html
├── web.config
├── assets/
├── *.js (arquivos JavaScript)
├── *.css (arquivos de estilo)
└── outros arquivos estáticos
```

## 🔧 Configurações Avançadas

### 1. Configurar HTTPS

Para habilitar HTTPS:

1. Instale um certificado SSL no IIS
2. Configure binding HTTPS no site
3. Adicione redirecionamento HTTP para HTTPS no web.config

### 2. Configurar Domínio Personalizado

1. Configure DNS para apontar para o servidor
2. Adicione binding no IIS com o domínio
3. Configure certificado SSL se necessário

### 3. Otimizações de Performance

- **Compressão**: Já habilitada no web.config
- **Cache**: Configurado para arquivos estáticos
- **CDN**: Considere usar CDN para arquivos estáticos

## 🐛 Solução de Problemas

### Erro 404 em rotas do Angular

**Problema**: Páginas do Angular retornam 404 ao acessar diretamente.

**Solução**: Verifique se o web.config está presente e se o URL Rewriting está habilitado no IIS.

### Erro de MIME Type

**Problema**: Arquivos .js ou .css não carregam.

**Solução**: Verifique se os MIME types estão configurados no web.config.

### Problemas de Permissão

**Problema**: Erro 500 ou arquivos não encontrados.

**Solução**: Configure as permissões corretas:
- IIS_IUSRS: Leitura e Execução
- IUSR: Leitura

### Performance Lenta

**Soluções**:
1. Habilite compressão gzip
2. Configure cache apropriado
3. Use CDN para arquivos estáticos
4. Otimize imagens

## 📊 Monitoramento

### Logs do IIS

Os logs ficam em: `C:\inetpub\logs\LogFiles\`

### Performance

Use ferramentas como:
- IIS Manager (Performance)
- Application Insights
- Google PageSpeed Insights

## 🔄 Atualizações

Para atualizar a aplicação:

1. Faça o build novamente: `npm run build`
2. Copie os novos arquivos para o diretório do IIS
3. Reinicie o Application Pool se necessário

## 📞 Suporte

Se encontrar problemas:

1. Verifique os logs do IIS
2. Teste localmente com `ng serve`
3. Verifique as configurações do web.config
4. Confirme as permissões de arquivo

---

**Nota**: Este guia assume que você tem conhecimento básico de IIS e Windows Server. Para ambientes de produção, considere usar ferramentas de CI/CD como Azure DevOps ou GitHub Actions.
