# Load Tests with k6

Este diretório contém testes de carga para a API do MeuCatalogo usando [k6](https://k6.io/).

## Pré-requisitos

1.  Instale o k6:
    *   **macOS** (Homebrew): `brew install k6`
    *   **Windows** (Chocolatey): `choco install k6`
    *   **Linux**: [Instruções oficiais](https://k6.io/docs/get-started/installation/)

## Como executar

Execute o script `load-test.js` usando o comando `k6 run`.

```bash
k6 run load-test.js
```

### Configurações personalizadas

Você pode alterar os parâmetros padrão usando variáveis de ambiente:

*   `BASE_URL`: URL base da API (padrão: `http://localhost:5000`)
*   `USER_EMAIL`: Email do usuário para autenticação (padrão: `teste@teste.com`)
*   `USER_PASSWORD`: Senha do usuário (padrão: `Senha123!`)

Exemplo:

```bash
k6 run -e BASE_URL=https://minha-api.com -e USER_EMAIL=admin@admin.com load-test.js
```

## Interpretação dos Resultados

O k6 gerará um relatório no terminal ao final da execução. Preste atenção nas seguintes métricas:

*   **http_req_duration**: Tempo total da requisição (latência). O teste falhará se 95% das requisições demorarem mais de 500ms.
*   **http_req_failed**: Porcentagem de requisições que falharam (status diferente de 2xx).
*   **vus**: Número de usuários virtuais simultâneos.
*   **iterations**: Quantas vezes o script completo foi executado.

## Cenário do Teste

1.  **Autenticação**: Faz login com as credenciais fornecidas.
2.  **Obter Catálogos**: Busca a lista de catálogos do usuário.
3.  **Obter Produtos**: Busca os produtos do primeiro catálogo encontrado.

O teste simula um ramp-up (aumento gradual) de usuários até 10 VUs, mantém por 1 minuto e depois diminui.
