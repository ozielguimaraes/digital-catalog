# MeuCatalogo API

Este é o backend da aplicação MeuCatalogo, uma API RESTful desenvolvida em ASP.NET Core.

## Requisitos

- .NET 6.0 SDK ou superior
- SQL Server (ou outro banco de dados compatível com Entity Framework Core)

## Como executar

1. Navegue até a pasta do projeto API:
   ```
   cd backend/MeuCatalogo.API
   ```

2. Execute a aplicação:
   ```
   dotnet run
   ```

3. Acesse a documentação da API no Swagger:
   ```
   https://localhost:5001/swagger
   ```

## Migrações de banco de dados

Para criar uma nova migração:

```
cd backend/MeuCatalogo.Application
dotnet ef migrations add Inicial --context ApplicationDbContext --startup-project ../MeuCatalogo.API/MeuCatalogo.API.csproj --output-dir Infrastructure/Data/Migrations
```

## Endpoints principais

- **Auth**: `/api/auth` - Registro e login de usuários
- **Catalogos**: `/api/catalogos` - Gerenciamento de catálogos
- **Produtos**: `/api/produtos` - Gerenciamento de produtos
- **Planos de Assinatura**: `/api/planoassinatura` - Gerenciamento de planos de assinatura

## Solução de problemas

- Se o Swagger não abrir, verifique se a aplicação está rodando na porta correta (5001 para HTTPS)
- Certifique-se de que o banco de dados está acessível e as credenciais estão corretas no arquivo `appsettings.json`