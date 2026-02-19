<p align="center">
  <h1 align="center">🚀 Boilerplate Customizer</h1>
  <p align="center">
    Um gerador de boilerplate full-stack pronto para produção com backend .NET 9 e frontend React 19.
    <br />
    <a href="./README.md"><strong>🇺🇸 Read in English »</strong></a>
  </p>
</p>

---

## 📋 Sumário

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Stack Tecnológica](#-stack-tecnológica)
- [Gerador de Projetos (Executável)](#-gerador-de-projetos-executável)
- [Funcionalidades do Backend](#-funcionalidades-do-backend)
- [Funcionalidades do Frontend](#-funcionalidades-do-frontend)
- [Como Começar](#-como-começar)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Configuração](#-configuração)
- [Migrations do Banco de Dados](#-migrations-do-banco-de-dados)
- [Endpoints da API](#-endpoints-da-api)

---

## 🎯 Visão Geral

Este é um **gerador de projetos boilerplate** projetado para acelerar a criação de aplicações full-stack. Em vez de começar do zero, você executa o **BoilerplateCustomizer**, responde algumas perguntas e recebe um projeto totalmente estruturado com autenticação, notificações em tempo real, jobs em background, serviço de email e muito mais — tudo pronto para customizar.

O gerador cria:
- Um **backend .NET 9** seguindo Clean Architecture e padrões RESTful
- Um **frontend React 19** com Material UI, rotas, contexto de autenticação e i18n — pronto para customização
- Suporte opcional a **multi-tenancy** com isolamento de tenants, impersonação e sistema de convites

---

## 🏗 Arquitetura

O backend segue **Clean Architecture** com separação clara de responsabilidades em 6 projetos:

```
┌──────────────────────────────────────────────────────┐
│                    Apresentação                       │
│                  (SeuProjeto.Api)                     │
│         Controllers · Middleware · ApiResponse        │
├──────────────────────────────────────────────────────┤
│                     Aplicação                         │
│              (SeuProjeto.Application)                 │
│    Services · DTOs · Interfaces · Utils · Jobs        │
├──────────────────────────────────────────────────────┤
│                      Domínio                          │
│                (SeuProjeto.Domain)                    │
│       Entities · Interfaces · Constants · Enums       │
├──────────────────────────────────────────────────────┤
│                   Infraestrutura                      │
│              (SeuProjeto.Infra.Data)                  │
│   DbContext · Identity · Repositories · Migrations    │
├──────────────────────────────────────────────────────┤
│                        IoC                            │
│               (SeuProjeto.Infra.IoC)                  │
│        Injeção de Dependência · Configuração          │
├──────────────────────────────────────────────────────┤
│                    Job Server                         │
│              (SeuProjeto.JobServer)                   │
│           Hangfire Wrappers · Triggers                │
└──────────────────────────────────────────────────────┘
```

---

## 🛠 Stack Tecnológica

### Backend
| Tecnologia | Propósito |
|---|---|
| **.NET 9** | Runtime e framework |
| **ASP.NET Core Web API** | API RESTful com controllers |
| **Entity Framework Core** | ORM com migrations Code-First |
| **SQL Server** | Banco de dados |
| **ASP.NET Core Identity** | Autenticação e gerenciamento de usuários |
| **JWT (JSON Web Tokens)** | Autenticação stateless com refresh tokens |
| **SignalR** | Comunicação em tempo real via WebSocket |
| **Hangfire** | Processamento de jobs em background com dashboard |
| **SMTP** | Serviço de email transacional |

### Frontend
| Tecnologia | Propósito |
|---|---|
| **React 19** | Framework de UI |
| **TypeScript** | Segurança de tipos |
| **Vite** | Build tool e servidor de desenvolvimento |
| **Material UI (MUI) 7** | Biblioteca de componentes |
| **React Router 7** | Roteamento client-side |
| **Axios** | Cliente HTTP |
| **SignalR Client** | Notificações em tempo real |
| **i18next** | Internacionalização (EN/PT-BR) |
| **notistack** | Notificações toast |

---

## ⚡ Gerador de Projetos (Executável)

O **BoilerplateCustomizer** é um `.exe` autocontido que monta todo o seu projeto de forma interativa.

### O que ele faz:
1. **Pede o nome do projeto** — renomeia todos os namespaces, arquivos e referências
2. **Pede quantas entidades criar** — gera stack CRUD completo (Entity, Repository, Service, Controller, DTOs) a partir de templates
3. **Pergunta sobre multi-tenancy** — remove opcionalmente todo código relacionado a tenants para projetos mais simples
4. **Pede o caminho de saída** — permite escolher onde criar o projeto (ou usa o padrão)
5. **Gera uma chave secreta JWT** automaticamente
6. **Cria uma pasta mãe** contendo os projetos backend e frontend

### Como executar:
```bash
cd Executavel/publish
./BoilerplateCustomizer.exe
```

### Estrutura gerada:
```
SeuProjeto/                     # Pasta mãe
├── SeuProjeto/                 # Backend (solução .NET)
│   ├── SeuProjeto.Api/
│   ├── SeuProjeto.Application/
│   ├── SeuProjeto.Domain/
│   ├── SeuProjeto.Infra.Data/
│   ├── SeuProjeto.Infra.IoC/
│   └── SeuProjeto.JobServer/
└── react-seuprojeto/           # Frontend (aplicação React)
    ├── src/
    ├── package.json
    └── ...
```

---

## 🔧 Funcionalidades do Backend

### Design RESTful
Todos os controllers seguem convenções REST com resposta padronizada:
```json
{
  "isSuccess": true,
  "message": "Operação concluída",
  "data": { ... }
}
```

### Autenticação e Autorização
- **Autenticação baseada em JWT** com rotação de access + refresh token
- **Autorização por roles**: `AdminGlobal`, `GlobalManager`, `TenantAdmin`, `User`, `Guest`
- **Verificação de email** com tokens de confirmação
- **Esqueci/Redefinir senha** com recuperação via email
- **Forçar troca de senha** no primeiro login

### Notificações em Tempo Real (SignalR)
- **Hub WebSocket** em `/hubs/systemNotification`
- Push de notificações para usuários específicos ou broadcast para todos
- Autenticação JWT nas conexões WebSocket
- Eventos: `UpdateNotifications` para feed de notificações ao vivo

### Jobs em Background (Hangfire)
- **Projeto JobServer dedicado** com padrão Wrappers e Triggers
- **Processamento baseado em filas** com filas nomeadas por entidade
- **Retry automático** com tentativas configuráveis (padrão: 3)
- Dashboard do Hangfire disponível em `/hangfire`
- Arquitetura:
  - `IJobScheduler` → enfileira jobs via Hangfire
  - `Wrapper` → implementa o scheduler, chama `BackgroundJob.Enqueue`
  - `Trigger` → executa a lógica real do job
  - `IJobExecutor` → interface de lógica de negócio

### Serviço de Email
- Email via SMTP com provedor configurável
- Templates integrados para: redefinição de senha, verificação de email, convites de tenant
- Interface `IEmailService` facilmente extensível

### Middleware de Tratamento de Exceções
Handler global que mapeia exceções .NET para status HTTP corretos:
- `ArgumentException` → 400
- `UnauthorizedAccessException` → 401
- `KeyNotFoundException` → 404
- `InvalidOperationException` → 409
- Não tratado → 500

### Padrão Repository Genérico
- `IRepository<T>` com operações CRUD + soft delete
- `IUnitOfWork` para gerenciamento de transações
- Consultas baseadas em LINQ com `Expression<Func<T, object>>` includes

### Sistema de Templates de Entidades
Cada entidade gerada pelo customizador recebe:
- Entidade no Domínio (herda `EntityBase` com `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`)
- Registro no Repository
- Service + Interface
- Controller com endpoints CRUD completos
- DTOs (Create, Update, Response)
- Hangfire Job Scheduler + Executor
- Registro no DI

### Multi-Tenancy (Opcional)
Quando habilitado:
- Isolamento de tenant com `TenantId` nas entidades
- Sistema de convites de tenant com tokens por email
- Impersonação de contas de tenant pelo admin
- Configurações e ajustes específicos por tenant
- Role `TenantAdmin` para gerenciamento de tenants

---

## 🎨 Funcionalidades do Frontend

O frontend React foi projetado como um **ponto de partida mínimo e funcional** — ele fornece a infraestrutura essencial para que você possa focar em construir suas funcionalidades de negócio.

### O que está incluso:
- **Fluxo de autenticação** — Login, Registro, Esqueci/Redefinir Senha, Verificação de Email
- **Rotas protegidas** com controle de acesso baseado em roles
- **Layout dashboard** com Sidebar + Header responsivos
- **Centro de notificações em tempo real** via integração SignalR
- **Página de gerenciamento de usuários** com DataGrid (CRUD)
- **Notificações do sistema** com funcionalidade criar/ler/limpar
- **Tema Dark/Light** com toggle
- **Internacionalização** (Inglês + Português) com seletor de idioma
- **Cliente API** (Axios) com interceptor JWT e refresh automático de token
- **Notificações toast** (notistack)
- **Componente DataGrid reutilizável** construído sobre MUI X DataGrid

### O que é intencionalmente simples:
O frontend é um **boilerplate** — ele foi feito para ser customizado. A UI é limpa e funcional, mas deliberadamente não é excessivamente estilizada. Você deve:
- Adicionar suas próprias páginas e componentes de negócio
- Customizar o tema e branding em `src/theme/`
- Estender a navegação da sidebar em `src/components/Layout/Sidebar.tsx`
- Adicionar novas chamadas de API em `src/services/`

---

## 🚀 Como Começar

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB ou instância completa)

### 1. Gere seu projeto
```bash
cd Executavel/publish
./BoilerplateCustomizer.exe
```
Siga as instruções interativas.

### 2. Configure o backend
```bash
cd SeuProjeto/SeuProjeto/SeuProjeto.Api
```
Copie e edite o arquivo de configuração:
```bash
cp appsettings.Example.json appsettings.json
```
Atualize o `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SeuBancoDeDados;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JWT": {
    "SecretKey": "gerado-automaticamente-pelo-customizer",
    "Issuer": "SeuProjetoAPI",
    "Audience": "SeuProjetoClient",
    "ExpiresInMinutes": 60
  },
  "SMTP": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "seu-email@example.com",
    "Password": "sua-senha-smtp"
  },
  "Frontend": {
    "Url": "http://localhost:3000"
  }
}
```

### 3. Execute as migrations do banco de dados
**Windows:**
```bash
create-migrations-windows.bat
apply-migrations-windows.bat
```
**Linux/macOS:**
```bash
chmod +x create-migrations-linux.sh apply-migrations-linux.sh
./create-migrations-linux.sh
./apply-migrations-linux.sh
```

### 4. Execute o backend
```bash
dotnet run --project SeuProjeto.Api
```
O backend irá iniciar em `https://localhost:7001` (ou porta configurada).

### 5. Execute o frontend
```bash
cd SeuProjeto/react-seuprojeto
npm install
npm run dev
```
O frontend irá iniciar em `http://localhost:3000`.

---

## 📁 Estrutura do Projeto

### Backend
```
SeuProjeto/
├── SeuProjeto.Api/
│   ├── Controllers/           # Controllers da API REST
│   ├── ApiResponse/           # Wrapper de resposta padronizado
│   ├── Middleware/             # Tratamento de exceções, impersonação
│   ├── Program.cs             # Startup e configuração do pipeline
│   └── appsettings.json       # Configuração
├── SeuProjeto.Application/
│   ├── Services/              # Lógica de negócio
│   │   ├── Auth/              # Serviço de autenticação
│   │   ├── Email/             # Serviço de envio de email
│   │   ├── SignalR/           # Hub de notificações em tempo real
│   │   ├── SystemNotifications/
│   │   └── Users/
│   ├── Dtos/                  # Objetos de transferência de dados
│   ├── Interfaces/            # Contratos de serviços
│   ├── JobExecutors/          # Executores de jobs do Hangfire
│   ├── JobScheduler/          # Agendadores de jobs do Hangfire
│   ├── Common/                # Eventos e constantes compartilhados
│   └── Utils/                 # Utilitários estáticos
├── SeuProjeto.Domain/
│   ├── Entities/              # Entidades do domínio
│   ├── Interfaces/            # Contratos de repositórios e serviços
│   ├── Constants/             # Roles, etc.
│   └── Enums/
├── SeuProjeto.Infra.Data/
│   ├── Context/               # DbContext, seeding, factory
│   ├── Identity/              # Implementação do ASP.NET Identity
│   ├── Repositories/          # Repositório genérico
│   └── Migrations/
├── SeuProjeto.Infra.IoC/
│   └── DependencyInjection.cs # Todos os registros de DI
└── SeuProjeto.JobServer/
    ├── Wrappers/              # Wrappers de jobs do Hangfire
    └── Triggers/              # Triggers com configuração de retry
```

### Frontend
```
react-seuprojeto/
├── src/
│   ├── components/
│   │   ├── Layout/            # Sidebar, Header, DashboardLayout
│   │   ├── common/            # DataGrid, LanguageSwitcher
│   │   ├── notifications/     # CreateNotificationModal
│   │   └── users/             # EditUserModal
│   ├── contexts/              # AuthContext, tema, utilitários
│   ├── hooks/                 # Hooks React customizados
│   ├── pages/                 # Páginas das rotas
│   ├── services/              # Cliente API (Axios)
│   ├── types/                 # Interfaces TypeScript
│   ├── utils/                 # Constantes, helpers de autenticação
│   ├── theme/                 # Configuração do tema MUI
│   └── i18n.ts                # Configuração de internacionalização
├── public/                    # Assets estáticos e traduções
└── package.json
```

---

## ⚙ Configuração

### Variáveis de Ambiente (Frontend)
```env
VITE_API_BASE_URL=https://localhost:7001
VITE_SIGNALR_URL=https://localhost:7001/hubs/systemNotification
```

### appsettings.json (Backend)
| Seção | Descrição |
|---|---|
| `ConnectionStrings.DefaultConnection` | String de conexão do SQL Server |
| `JWT.SecretKey` | Chave secreta de 64 caracteres gerada automaticamente |
| `JWT.Issuer` / `JWT.Audience` | Parâmetros de validação do token |
| `JWT.ExpiresInMinutes` | Tempo de vida do access token |
| `SMTP.*` | Configuração do provedor de email |
| `Frontend.Url` | Origem CORS para a aplicação React |

---

## 🗄 Migrations do Banco de Dados

O projeto inclui scripts auxiliares para migrations do Entity Framework Core:

| Script | Descrição |
|---|---|
| `create-migrations-windows.bat` | Cria uma nova migration |
| `apply-migrations-windows.bat` | Aplica migrations pendentes |
| `create-migrations-linux.sh` | Cria uma nova migration (Linux/macOS) |
| `apply-migrations-linux.sh` | Aplica migrations pendentes (Linux/macOS) |

Comandos manuais:
```bash
# Criar migration
dotnet ef migrations add NomeDaMigration --project SeuProjeto.Infra.Data --startup-project SeuProjeto.Api

# Aplicar migration
dotnet ef database update --project SeuProjeto.Infra.Data --startup-project SeuProjeto.Api
```

---

## 📡 Endpoints da API

### Auth (`/api/Auth`)
| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/Login` | Autenticar usuário |
| POST | `/Register` | Registrar novo usuário |
| GET | `/Logout` | Deslogar usuário atual |
| POST | `/RefreshToken` | Renovar tokens JWT |
| POST | `/ForgotPassword` | Solicitar email de redefinição de senha |
| POST | `/ResetPassword` | Redefinir senha com token |
| POST | `/ChangePassword` | Alterar senha atual |
| GET | `/confirm-email` | Confirmar endereço de email |

### Usuários (`/api/User`)
| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/` | Listar todos os usuários |
| DELETE | `/{id}` | Excluir usuário |
| PATCH | `/{id}` | Atualizar usuário |

### Notificações do Sistema (`/api/SystemNotification`)
| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/` | Obter todas as notificações |
| POST | `/` | Criar notificação (Admin) |
| PATCH | `/MarkAsRead/{id}` | Marcar como lida |
| POST | `/ClearAllMessages` | Limpar todas as mensagens |

### Tenant (`/api/Tenant`) — *Apenas com multi-tenancy*
| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/` | Listar tenants |
| POST | `/` | Criar tenant |
| POST | `/Invite` | Convidar usuário para tenant |
| POST | `/Impersonate` | Impersonar tenant |

---

## 📄 Licença

Este projeto é open source e está disponível para uso como ponto de partida para suas aplicações.

---

<p align="center">
  Feito com ❤️ para economizar tempo de desenvolvedores.
</p>
