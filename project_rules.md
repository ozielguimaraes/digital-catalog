# Regras do Projeto (Digital Catalog)

Estas regras devem ser aplicadas em todas as alterações no repositório.

---

### **Estilo e Formatação**

1.  **Respeitar `.editorconfig`**: Todas as configurações de estilo (indentação, quebras de linha, tamanho máximo de linha, nomes e preferências de C#) devem seguir estritamente o que está definido no arquivo `.editorconfig`.
2.  **Namespaces `file-scoped`**: Preferir o uso de namespaces no formato `file-scoped` para manter o código mais limpo e conciso, conforme as convenções modernas do C#.
3.  **Comentários**: Não adicionar comentários, a menos que o usuário solicite explicitamente. O código deve ser claro o suficiente para ser autoexplicativo.
4.  **Consistência de Estilo**: Ao editar um arquivo, manter o estilo de código existente, incluindo a organização de `imports`/`usings`, a ordenação de métodos e propriedades, e os padrões de validação e tratamento de exceções.

---

### **Arquitetura da Solução .NET**

#### **`api` (Projeto de Backend)**

1.  **`api/Entities`**: Entidades do Entity Framework Core devem ser localizadas neste diretório, sob o namespace `DigitalCatalog.Api.Entities`.
2.  **`api/DTOs`**: Data Transfer Objects (DTOs) devem ser colocados em `api/DTOs`, com o namespace `DigitalCatalog.Api.DTOs`.
3.  **`api/Data/ApiDbContext.cs`**:
    *   Mapeamentos do EF Core devem seguir o padrão de `ToTable("snake_case")`.
    *   Colunas de chave estrangeira devem usar o sufixo `_id` (ex: `product_id`).
    *   Preferir adicionar novos `DbSet<>` em arquivos parciais (`ApiDbContext.*.cs`) quando fizer sentido para organizar por domínio.
    *   Evitar o uso de Lazy Loading baseado em proxies. Para carregar dados relacionados, usar carregamento explícito (`.Include()`) ou seletivo para otimizar o desempenho e evitar problemas de N+1.
4.  **`api/Repositories`**:
    *   Implementações devem ficar em `api/Repositories/Implementations` e suas interfaces em `api/Repositories/Interfaces`.
    *   Para consultas críticas de performance, preferir o uso de SQL puro com Dapper, fazendo o bind direto para DTOs para minimizar a materialização de entidades completas.
5.  **`api/Services`**:
    *   Implementações devem ficar em `api/Services/Implementations` e suas interfaces em `api/Services/Interfaces`.
    *   Realizar a validação dos dados de entrada e lançar exceções apropriadas (ex: `BadHttpRequestException` ou uma exceção customizada como `UserFriendlyException`) para erros de regra de negócio.
    *   Utilizar injeção de dependência e manter os *services* "finos", delegando a lógica de acesso a dados para os *repositories*.

#### **`mobile/MeuCatalogo` (Projeto MAUI)**

1.  **Estrutura por `Features`**: Manter a organização do código baseada em funcionalidades dentro do diretório `Features`.
2.  **`UseCases`**: A lógica de negócio específica de uma funcionalidade deve ser encapsulada em *Use Cases* (ex: `DeleteProdutoUseCase.cs`).
3.  **MVVM**: Seguir o padrão Model-View-ViewModel. As `Views` (páginas XAML) devem ter seus `ViewModels` correspondentes, que orquestram as chamadas aos `UseCases` e gerenciam o estado da UI.
4.  **Serviços do App**: Serviços consumidos pelo aplicativo móvel (como clientes de API Refit) devem ser definidos no diretório `Services`.

---

### **Banco de Dados e Migrations**

1.  **Local das Migrations**: As migrations do `ApiDbContext` devem ser mantidas em `backend/MeuCatalogo.Application/Migrations`.
2.  **Schema Changes**: Para qualquer alteração no schema do banco de dados, uma nova migration deve ser criada via `dotnet ef migrations add`. Não editar o `Snapshot` manualmente, a menos que seja estritamente necessário.
3.  **Convenções de Nomenclatura**:
    *   Novas tabelas devem seguir o padrão `snake_case`.
    *   Tabelas de relacionamento N:N devem ter uma entidade de junção explícita com chave primária composta e FKs nomeadas.
    *   Todas as novas colunas, índices e chaves (PK/FK) devem ser criados em minúsculas (`snake_case`). As existentes não precisam ser alteradas.

---

### **Qualidade e Verificação**

1.  **Build**: Antes de finalizar uma tarefa ou `commit`, garantir que a solução compila sem erros executando `dotnet build` no(s) projeto(s) afetado(s).
2.  **Testes**:
    *   Ao adicionar ou modificar uma lógica de negócio nos *services* da API, adicionar ou atualizar os testes de unidade correspondentes no projeto de testes (ex: `tests/DigitalCatalog.Api.Tests`).
    *   Testes de performance com k6 estão em `tests/k6` e devem ser atualizados se novos endpoints críticos forem adicionados.
