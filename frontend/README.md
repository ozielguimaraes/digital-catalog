# Digital Catalog Frontend

Aplicação Angular com Material Design para gerenciamento de catálogo digital, implementando autenticação e autorização com suporte a temas light e dark.

## 🚀 Características

- **Angular 18** com TypeScript
- **Angular Material** para componentes UI
- **Autenticação e Autorização** completa
- **Temas Light/Dark** com persistência
- **Layout Responsivo** com sidebar e header
- **Clean Code** seguindo princípios KISS e DRY
- **Interceptors HTTP** para autenticação automática
- **Guards de Rota** para proteção de páginas
- **Gerenciamento de Estado** reativo

## 📋 Pré-requisitos

- Node.js (versão 18 ou superior)
- npm ou yarn
- Angular CLI (instalar globalmente: `npm install -g @angular/cli`)

## 🛠️ Instalação

1. Clone o repositório:
```bash
git clone <repository-url>
cd digital-catalog/frontend
```

2. Instale as dependências:
```bash
npm install
# ou
yarn install
```

3. Configure a API:
   - Edite o arquivo `src/environments/environment.ts`
   - Atualize a URL da API conforme necessário

4. Execute a aplicação:
```bash
ng serve
```

5. Acesse `http://localhost:4200`

## 🏗️ Estrutura do Projeto

```
src/
├── app/
│   ├── core/                    # Serviços e funcionalidades core
│   │   ├── guards/             # Guards de rota
│   │   ├── interceptors/       # Interceptors HTTP
│   │   ├── models/             # Modelos de dados
│   │   └── services/           # Serviços principais
│   ├── features/               # Módulos de funcionalidades
│   │   ├── auth/               # Autenticação
│   │   └── dashboard/          # Dashboard
│   ├── layouts/                # Layouts da aplicação
│   │   └── main-layout/        # Layout principal
│   └── shared/                 # Componentes compartilhados
├── assets/                     # Recursos estáticos
└── environments/               # Configurações de ambiente
```

## 🔐 Autenticação

A aplicação implementa um sistema completo de autenticação:

- **Login** com email e senha
- **Registro** de novos usuários
- **Logout** com limpeza de dados
- **Refresh Token** automático
- **Proteção de rotas** com guards
- **Interceptors HTTP** para adicionar tokens

### Endpoints da API

- `POST /auth/login` - Login
- `POST /auth/register` - Registro
- `POST /auth/logout` - Logout
- `POST /auth/refresh` - Refresh token
- `GET /auth/me` - Dados do usuário atual

## 🎨 Temas

A aplicação suporta temas light e dark:

- **Tema Light**: Padrão com cores claras
- **Tema Dark**: Tema escuro para melhor experiência noturna
- **Persistência**: Preferência salva no localStorage
- **Detecção automática**: Detecta preferência do sistema

### Como alternar tema

- Clique no ícone de sol/lua no header
- A preferência será salva automaticamente

## 📱 Responsividade

A aplicação é totalmente responsiva:

- **Desktop**: Layout completo com sidebar
- **Tablet**: Sidebar colapsível
- **Mobile**: Menu hambúrguer e layout adaptado

## 🧪 Desenvolvimento

### Scripts disponíveis

```bash
# Desenvolvimento
ng serve

# Build de produção
ng build

# Testes
ng test

# Linting
ng lint

# Geração de componentes
ng generate component nome-do-componente

# Geração de serviços
ng generate service nome-do-servico
```

### Estrutura de Componentes

Cada componente segue a estrutura:
- `component.ts` - Lógica do componente
- `component.html` - Template
- `component.scss` - Estilos
- `component.spec.ts` - Testes (quando aplicável)

## 🔧 Configuração

### Variáveis de Ambiente

```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://catalogo-api.sanyz.com.br/api',
  appName: 'Digital Catalog',
  version: '1.0.0'
};
```

### Personalização de Temas

Os temas podem ser personalizados em `src/styles.scss`:

```scss
// Tema personalizado
$custom-primary: mat-palette($mat-indigo);
$custom-accent: mat-palette($mat-pink, A200, A100, A400);
$custom-theme: mat-light-theme((
  color: (
    primary: $custom-primary,
    accent: $custom-accent,
  )
));
```

## 📚 Tecnologias Utilizadas

- **Angular 18**
- **Angular Material 18**
- **TypeScript**
- **SCSS**
- **RxJS**
- **Angular CDK**

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 🆘 Suporte

Para suporte, entre em contato através de:
- Email: suporte@exemplo.com
- Issues do GitHub

---

Desenvolvido com ❤️ seguindo princípios de Clean Code, KISS e DRY.
