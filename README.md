<p align="center">
  <h1 align="center">⚒️ Foundry</h1>
  <p align="center">
    A CLI-powered full-stack project scaffolder with .NET 9 backend and React 19 frontend.
    <br />
    <a href="./README.pt-br.md"><strong>🇧🇷 Leia em Português »</strong></a>
  </p>
</p>

---

**Foundry CLI** is an open-source, cross-platform command-line tool focused on automating project scaffolding and boilerplate customization.

It aims to simplify the creation and configuration of project structures, helping developers bootstrap new projects faster and more consistently.

The source code is publicly available for transparency and community contributions. Suggestions, bug reports, and pull requests are always welcome!

If you just want to use the tool, precompiled binaries for **Windows**, **Linux**, and **macOS** are available in the [Releases](https://github.com/Caio26Gualberto/Foundry/releases) section — no build process required.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Foundry CLI](#-foundry-cli)
- [Backend Features](#-backend-features)
- [Frontend Features](#-frontend-features)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [Configuration](#-configuration)
- [Database Migrations](#-database-migrations)
- [API Endpoints](#-api-endpoints)
- [Contributing](#-contributing)

---

## 🎯 Overview

**Foundry** is a CLI tool that scaffolds production-ready full-stack applications. Run the CLI, answer a few questions, and get a fully structured project with authentication, real-time notifications, background jobs, email services, and more — all ready to customize.

The CLI generates:
- A **.NET 9 backend** following Clean Architecture and RESTful API standards
- A **React 19 frontend** with Material UI, routing, auth context, and i18n
- Choice between **multi-tenancy** (with tenant isolation, impersonation, and invitations) or **single-tenancy** templates

Templates are embedded inside the CLI binary — no external folders required.

---

## 🏗 Architecture

The backend follows **Clean Architecture** with clear separation of concerns across 6 projects:

```
┌──────────────────────────────────────────────────────┐
│                    Presentation                       │
│                  (YourProject.Api)                    │
│          Controllers · Middleware · ApiResponse       │
├──────────────────────────────────────────────────────┤
│                    Application                        │
│              (YourProject.Application)                │
│    Services · DTOs · Interfaces · Utils · Jobs        │
├──────────────────────────────────────────────────────┤
│                      Domain                           │
│                (YourProject.Domain)                   │
│       Entities · Interfaces · Constants · Enums       │
├──────────────────────────────────────────────────────┤
│                   Infrastructure                      │
│              (YourProject.Infra.Data)                 │
│   DbContext · Identity · Repositories · Migrations    │
├──────────────────────────────────────────────────────┤
│                        IoC                            │
│               (YourProject.Infra.IoC)                │
│          Dependency Injection · Configuration         │
├──────────────────────────────────────────────────────┤
│                    Job Server                         │
│              (YourProject.JobServer)                  │
│           Hangfire Wrappers · Triggers                │
└──────────────────────────────────────────────────────┘
```

---

## 🛠 Tech Stack

### Backend
| Technology | Purpose |
|---|---|
| **.NET 9** | Runtime & framework |
| **ASP.NET Core Web API** | RESTful API with controllers |
| **Entity Framework Core** | ORM with Code-First migrations |
| **SQL Server** | Database |
| **ASP.NET Core Identity** | Authentication & user management |
| **JWT (JSON Web Tokens)** | Stateless authentication with refresh tokens |
| **SignalR** | Real-time WebSocket communication |
| **Hangfire** | Background job processing with dashboard |
| **SMTP** | Transactional email service |

### Frontend
| Technology | Purpose |
|---|---|
| **React 19** | UI framework |
| **TypeScript** | Type safety |
| **Vite** | Build tool & dev server |
| **Material UI (MUI) 7** | Component library |
| **React Router 7** | Client-side routing |
| **Axios** | HTTP client |
| **SignalR Client** | Real-time notifications |
| **i18next** | Internationalization (EN/PT-BR) |
| **notistack** | Toast notifications |

---

## ⚡ Foundry CLI

**Foundry CLI** is a self-contained `.exe` that scaffolds your entire project interactively. Templates for both multi-tenancy and single-tenancy are embedded inside the binary — just run it from anywhere.

### What it does:
1. **Asks for your project name** — renames all namespaces, files, and references
2. **Asks about multi-tenancy** — selects the appropriate template (multi-tenancy or single-tenancy)
3. **Asks how many entities** — generates full CRUD stack (Entity, Repository, Service, Controller, DTOs) from templates
4. **Asks for output path** — lets you choose where to create the project (or uses current directory)
5. **Generates a secure JWT secret key** automatically
6. **Creates a parent folder** containing both backend and frontend projects

### How to run:
```bash
Foundry.CLI.exe
```

### Output structure:
```
YourProject/                    # Parent folder
├── YourProject/                # Backend (.NET solution)
│   ├── YourProject.Api/
│   ├── YourProject.Application/
│   ├── YourProject.Domain/
│   ├── YourProject.Infra.Data/
│   ├── YourProject.Infra.IoC/
│   └── YourProject.JobServer/
└── react-yourproject/          # Frontend (React app)
    ├── src/
    ├── package.json
    └── ...
```

---

## 🔧 Backend Features

### RESTful API Design
All controllers follow REST conventions with consistent response wrapping:
```json
{
  "isSuccess": true,
  "message": "Operation completed",
  "data": { ... }
}
```

### Authentication & Authorization
- **JWT-based authentication** with access + refresh token rotation
- **Role-based authorization**: `AdminGlobal`, `GlobalManager`, `TenantAdmin`, `User`, `Guest`
- **Email verification** flow with confirmation tokens
- **Forgot/Reset password** flow with email-based token recovery
- **Force password change** on first login support

### Real-Time Notifications (SignalR)
- **WebSocket hub** at `/hubs/systemNotification`
- Push notifications to specific users or broadcast to all
- JWT authentication on WebSocket connections
- Events: `UpdateNotifications` for live notification feed

### Background Jobs (Hangfire)
- **Dedicated JobServer project** with Wrappers and Triggers pattern
- **Queue-based processing** with named queues per entity
- **Automatic retry** with configurable attempts (default: 3)
- Hangfire Dashboard available at `/hangfire`
- Architecture:
  - `IJobScheduler` → enqueues jobs via Hangfire
  - `Wrapper` → implements scheduler, calls `BackgroundJob.Enqueue`
  - `Trigger` → executes the actual job logic
  - `IJobExecutor` → business logic interface

### Email Service
- SMTP-based email with configurable provider
- Built-in templates for: password reset, email verification, tenant invitations
- Easily extensible `IEmailService` interface

### Exception Handling Middleware
Global exception handler that maps .NET exceptions to proper HTTP status codes:
- `ArgumentException` → 400
- `UnauthorizedAccessException` → 401
- `KeyNotFoundException` → 404
- `InvalidOperationException` → 409
- Unhandled → 500

### Generic Repository Pattern
- `IRepository<T>` with CRUD operations + soft delete
- `IUnitOfWork` for transaction management
- LINQ-based querying with `Expression<Func<T, object>>` includes

### Entity Template System
Each entity generated by the CLI gets:
- Domain Entity (inherits `EntityBase` with `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`)
- Repository registration
- Service + Interface
- Controller with full CRUD endpoints
- DTOs (Create, Update, Response)
- Hangfire Job Scheduler + Executor
- DI registration

### Multi-Tenancy (Optional)
When the multi-tenancy template is selected:
- Tenant isolation with `TenantId` on entities
- Tenant invitation system with email tokens
- Admin impersonation of tenant accounts
- Tenant-specific configurations and settings
- Role `TenantAdmin` for tenant management

---

## 🎨 Frontend Features

The React frontend is designed as a **minimal, functional starting point** — it provides the essential infrastructure so you can focus on building your business features.

### What's included:
- **Authentication flow** — Login, Register, Forgot/Reset Password, Email Verification
- **Protected routes** with role-based access control
- **Dashboard layout** with responsive Sidebar + Header
- **Real-time notification center** via SignalR integration
- **User management page** with DataGrid (CRUD)
- **System notifications** with create/read/clear functionality
- **Dark/Light theme** toggle
- **Internationalization** (English + Portuguese) with language switcher
- **API client** (Axios) with JWT interceptor and automatic token refresh
- **Toast notifications** (notistack)
- **Reusable DataGrid component** built on MUI X DataGrid

### What's intentionally simple:
The frontend is a **scaffold** — it's meant to be customized. The UI is clean and functional but deliberately not over-designed. You should:
- Add your own pages and business components
- Customize the theme and branding in `src/theme/`
- Extend the sidebar navigation in `src/components/Layout/Sidebar.tsx`
- Add new API service calls in `src/services/`

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or full instance)

### 1. Generate your project

**Windows:**
```bash
Foundry.CLI.exe
```

**Linux/macOS:**
```bash
chmod +x Foundry.CLI
./Foundry.CLI
```

> **Note for Linux/macOS users:** The `chmod +x` command grants execute permissions to the binary. By default, downloaded files don't have execution rights for security reasons. This is a one-time step — after setting permissions, you can run the CLI directly.

Follow the interactive prompts.

### 2. Configure the backend
```bash
cd YourProject/YourProject/YourProject.Api
```
Copy and edit the settings file:
```bash
cp appsettings.Example.json appsettings.json
```
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDatabase;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JWT": {
    "SecretKey": "auto-generated-by-foundry",
    "Issuer": "YourProjectAPI",
    "Audience": "YourProjectClient",
    "ExpiresInMinutes": 60
  },
  "SMTP": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "your-email@example.com",
    "Password": "your-smtp-password"
  },
  "Frontend": {
    "Url": "http://localhost:3000"
  }
}
```

### 3. Run database migrations
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

### 4. Run the backend
```bash
dotnet run --project YourProject.Api
```
Backend will start at `https://localhost:7001` (or configured port).

### 5. Run the frontend
```bash
cd YourProject/react-yourproject
npm install
npm run dev
```
Frontend will start at `http://localhost:3000`.

---

## 📁 Project Structure

### Repository
```
Foundry/
├── Foundry.CLI/               # CLI tool source code
│   ├── Program.cs             # CLI entry point & scaffolding logic
│   ├── Templates/
│   │   ├── multi-tenancy/     # Multi-tenant backend + frontend templates
│   │   └── single-tenancy/    # Single-tenant backend + frontend templates
│   └── Foundry.CLI.csproj
├── README.md                  # This file (English)
└── README.pt-br.md            # Portuguese version
```

### Generated Backend
```
YourProject/
├── YourProject.Api/
│   ├── Controllers/           # REST API controllers
│   ├── ApiResponse/           # Standardized response wrapper
│   ├── Middleware/             # Exception handling, impersonation
│   ├── Program.cs             # App startup & pipeline config
│   └── appsettings.json       # Configuration
├── YourProject.Application/
│   ├── Services/              # Business logic
│   │   ├── Auth/              # Authentication service
│   │   ├── Email/             # Email sending service
│   │   ├── SignalR/           # Real-time notification hub
│   │   ├── SystemNotifications/
│   │   └── Users/
│   ├── Dtos/                  # Data transfer objects
│   ├── Interfaces/            # Service contracts
│   ├── JobExecutors/          # Hangfire job executors
│   ├── JobScheduler/          # Hangfire job schedulers
│   ├── Common/                # Shared events & constants
│   └── Utils/                 # Static utilities
├── YourProject.Domain/
│   ├── Entities/              # Domain entities
│   ├── Interfaces/            # Repository & service contracts
│   ├── Constants/             # Roles, etc.
│   └── Enums/
├── YourProject.Infra.Data/
│   ├── Context/               # DbContext, seeding, factory
│   ├── Identity/              # ASP.NET Identity implementation
│   ├── Repositories/          # Generic repository
│   └── Migrations/
├── YourProject.Infra.IoC/
│   └── DependencyInjection.cs # All DI registrations
└── YourProject.JobServer/
    ├── Wrappers/              # Hangfire job wrappers
    └── Triggers/              # Job triggers with retry config
```

### Generated Frontend
```
react-yourproject/
├── src/
│   ├── components/
│   │   ├── Layout/            # Sidebar, Header, DashboardLayout
│   │   ├── common/            # DataGrid, LanguageSwitcher
│   │   ├── notifications/     # CreateNotificationModal
│   │   └── users/             # EditUserModal
│   ├── contexts/              # AuthContext, theme, utils
│   ├── hooks/                 # Custom React hooks
│   ├── pages/                 # Route pages
│   ├── services/              # API client (Axios)
│   ├── types/                 # TypeScript interfaces
│   ├── utils/                 # Constants, auth helpers
│   ├── theme/                 # MUI theme config
│   └── i18n.ts                # Internationalization setup
├── public/                    # Static assets & translations
└── package.json
```

---

## ⚙ Configuration

### Environment Variables (Frontend)
```env
VITE_API_BASE_URL=https://localhost:7001
VITE_SIGNALR_URL=https://localhost:7001/hubs/systemNotification
```

### appsettings.json (Backend)
| Section | Description |
|---|---|
| `ConnectionStrings.DefaultConnection` | SQL Server connection string |
| `JWT.SecretKey` | Auto-generated 64-char secret for token signing |
| `JWT.Issuer` / `JWT.Audience` | Token validation parameters |
| `JWT.ExpiresInMinutes` | Access token lifetime |
| `SMTP.*` | Email provider configuration |
| `Frontend.Url` | CORS origin for the React app |

---

## 🗄 Database Migrations

The generated project includes helper scripts for Entity Framework Core migrations:

| Script | Description |
|---|---|
| `create-migrations-windows.bat` | Creates a new migration |
| `apply-migrations-windows.bat` | Applies pending migrations |
| `create-migrations-linux.sh` | Creates a new migration (Linux/macOS) |
| `apply-migrations-linux.sh` | Applies pending migrations (Linux/macOS) |

Manual commands:
```bash
# Create migration
dotnet ef migrations add MigrationName --project YourProject.Infra.Data --startup-project YourProject.Api

# Apply migration
dotnet ef database update --project YourProject.Infra.Data --startup-project YourProject.Api
```

---

## 📡 API Endpoints

### Auth (`/api/Auth`)
| Method | Endpoint | Description |
|---|---|---|
| POST | `/Login` | Authenticate user |
| POST | `/Register` | Register new user |
| GET | `/Logout` | Logout current user |
| POST | `/RefreshToken` | Refresh JWT tokens |
| POST | `/ForgotPassword` | Request password reset email |
| POST | `/ResetPassword` | Reset password with token |
| POST | `/ChangePassword` | Change current password |
| GET | `/confirm-email` | Confirm email address |

### Users (`/api/User`)
| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | List all users |
| DELETE | `/{id}` | Delete user |
| PATCH | `/{id}` | Update user |

### System Notifications (`/api/SystemNotification`)
| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | Get all notifications |
| POST | `/` | Create notification (Admin) |
| PATCH | `/MarkAsRead/{id}` | Mark as read |
| POST | `/ClearAllMessages` | Clear all messages |

### Tenant (`/api/Tenant`) — *Multi-tenancy template only*
| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | List tenants |
| POST | `/` | Create tenant |
| POST | `/Invite` | Invite user to tenant |
| POST | `/Impersonate` | Impersonate tenant |

---

## 🤝 Contributing

Foundry is open source and contributions are welcome! Whether it's a bug fix, a new feature, documentation improvement, or just a suggestion — feel free to open an issue or submit a pull request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

If you have ideas, feedback, or tips, don't hesitate to share them through [Issues](https://github.com/Caio26Gualberto/Foundry/issues).

---

## 📄 License

This project is open source and available for use as a starting point for your applications.

---

<p align="center">
  Built with 🔨 by Foundry.
</p>
