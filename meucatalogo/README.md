# Meu Catálogo

Uma aplicação de catálogo de produtos desenvolvida com Angular e Ant Design.

## Requisitos

- Node.js (versão 14 ou superior)
- npm (versão 6 ou superior)

## Instalação

1. Clone este repositório
2. Navegue até a pasta do projeto
3. Instale as dependências:

```bash
npm install
```

## Executando o projeto

Para iniciar o servidor de desenvolvimento:

```bash
npm start
```

A aplicação estará disponível em `http://localhost:4200/`.

## Funcionalidades

- Cadastro de produtos com as seguintes informações:
  - Nome do produto
  - Categoria
  - Variações do produto
  - Preço
  - Preço com desconto
  - Quantidade em estoque (ou ilimitado)
  - Informações adicionais

## Tecnologias utilizadas

- Angular 17
- NG-ZORRO (Ant Design para Angular)
- Angular Reactive Forms

## Estrutura do projeto

- `src/app/produto-form`: Componente de formulário para cadastro de produtos

## Personalização

Você pode personalizar o tema do Ant Design editando o arquivo `src/theme.less`.