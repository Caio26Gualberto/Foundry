# React Boilerplate Frontend

Template frontend React com Material-UI para aplicações multi-tenant com autenticação baseada em roles.

## 🚀 Características

- **React 19** com TypeScript
- **Material-UI (MUI)** para componentes e tema
- **React Router** para navegação
- **Sistema de Autenticação** completo
- **Multi-tenancy** com seleção de tenant para AdminGlobal
- **Design Responsivo** e minimalista
- **Proteção de Rotas** baseada em roles
- **Context API** para gerenciamento de estado

## 📋 Pré-requisitos

- Node.js 18+
- npm ou yarn
- Backend API rodando (padrão: https://localhost:7001/api)

## 🛠️ Instalação

1. Clone o repositório
2. Instale as dependências:
```bash
npm install
```

3. Configure as variáveis de ambiente:
```bash
cp .env.example .env
```

4. Edite o arquivo `.env` com suas configurações:
```env
VITE_API_BASE_URL=https://localhost:7001/api
VITE_NODE_ENV=development
```

## 🎯 Como usar

### Desenvolvimento
```bash
npm run dev
```

### Build para produção
```bash
npm run build
```

### Preview da build
```bash
npm run preview
```

## 🏗️ Estrutura do Projeto

```
src/
├── components/          # Componentes reutilizáveis
│   ├── Layout/         # Componentes de layout (Header, Sidebar, etc.)
│   ├── AppRouter.tsx   # Configuração de rotas
│   └── ProtectedRoute.tsx # Proteção de rotas
├── contexts/           # Contexts do React
│   └── AuthContext.tsx # Context de autenticação
├── pages/              # Páginas da aplicação
│   ├── Login.tsx       # Página de login
│   ├── TenantSelection.tsx # Seleção de tenant (AdminGlobal)
│   └── Dashboard.tsx   # Dashboard principal
├── services/           # Serviços e APIs
│   └── api.ts         # Cliente da API
├── theme/              # Tema Material-UI
├── types/              # Definições de tipos TypeScript
└── utils/              # Utilitários e constantes
```

## 👥 Sistema de Roles

### Tipos de Usuário

- **AdminGlobal**: Acesso total, pode personificar qualquer tenant
- **GlobalManager**: Acesso global limitado, pode personificar tenants
- **TenantAdmin**: Administrador de um tenant específico
- **User**: Usuário comum de um tenant

### Fluxo de Autenticação

1. **Login**: Usuário faz login com email/senha
2. **Verificação de Role**: Sistema verifica as roles do usuário
3. **Seleção de Tenant** (se aplicável):
   - AdminGlobal/GlobalManager sem tenant → Página de seleção
   - Usuários com tenant → Redirecionamento direto para dashboard
4. **Dashboard**: Acesso às funcionalidades baseado nas roles

## 🎨 Personalização

### Tema
Edite `src/theme/index.ts` para personalizar cores, tipografia e componentes.

### Layout
Modifique os componentes em `src/components/Layout/` para ajustar o layout.

### Rotas
Adicione novas rotas em `src/components/AppRouter.tsx`.

## 🔧 API Integration

O template espera que a API backend tenha os seguintes endpoints:

- `POST /auth/login` - Login do usuário
- `GET /auth/me` - Dados do usuário atual
- `GET /tenants` - Lista de tenants (AdminGlobal)
- `POST /auth/impersonate-tenant` - Personificar tenant

### Formato de Resposta Esperado

```typescript
// Login Response
{
  user: {
    id: string;
    email: string;
    userName: string;
    tenantId?: string;
    roles: string[];
  };
  token: string;
}
```

## 📱 Responsividade

O template é totalmente responsivo:
- **Desktop**: Menu lateral fixo
- **Mobile**: Menu lateral colapsável
- **Tablet**: Layout adaptativo

## 🔒 Segurança

- Tokens JWT armazenados no localStorage
- Validação automática de token
- Proteção de rotas baseada em autenticação
- Logout automático em caso de token inválido

## 🚀 Deploy

1. Build do projeto:
```bash
npm run build
```

2. Os arquivos estarão na pasta `dist/`
3. Configure seu servidor web para servir os arquivos estáticos
4. Configure as variáveis de ambiente de produção

## 📄 Licença

Este é um template de boilerplate para uso interno.
