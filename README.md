# 🧾 Order Management System (OMS)

🇺🇸 English → <a href="./README.pt-BR.md">🇧🇷 Português</a>

<p align="center">
<img src="https://skillicons.dev/icons?i=dotnet,cs,angular,materialui,github,azure,git" />
</p>

<p align="center">

<img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/C%23-Backend-239120?logo=csharp&logoColor=white"/>
<img src="https://img.shields.io/badge/API-REST-00ADD8?logo=swagger&logoColor=white"/>
<img src="https://img.shields.io/badge/SQL_Server-Database-CC2927?logo=microsoftsqlserver&logoColor=white"/>
<img src="https://img.shields.io/badge/Angular-Frontend-DD0031?logo=angular&logoColor=white"/>
<img src="https://img.shields.io/badge/Cloud-Azure-0078D4?logo=microsoftazure&logoColor=white"/>

</p>

<p align="center">

<img src="https://github.com/GuicesarS/Order-Management-System-OMS-/actions/workflows/ci.yml/badge.svg"/>

<a href="https://sonarcloud.io/project/overview?id=GuicesarS_Order-Management-System-OMS-">
<img src="https://sonarcloud.io/api/project_badges/measure?project=GuicesarS_Order-Management-System-OMS-&metric=coverage"/>
</a>

<a href="https://sonarcloud.io/project/overview?id=GuicesarS_Order-Management-System-OMS-">
<img src="https://sonarcloud.io/api/project_badges/measure?project=GuicesarS_Order-Management-System-OMS-&metric=sqale_rating"/>
</a>

<a href="https://sonarcloud.io/project/overview?id=GuicesarS_Order-Management-System-OMS-">
<img src="https://sonarcloud.io/api/project_badges/measure?project=GuicesarS_Order-Management-System-OMS-&metric=security_rating"/>
</a>

</p>

---

# 🚀 Overview

The **Order Management System (OMS)** is a **full-stack application** built with an **ASP.NET Core 8 REST API** (Clean Architecture) and an **Angular 22 + Angular Material** frontend, designed to simulate a real-world order management workflow used in modern companies.

The project demonstrates several **software engineering best practices**, including:

- Clean Architecture (Domain / Application / Infrastructure / API)
- Rich domain model with a real order state machine (`Pending → Paid → Shipped` / `Cancelled`)
- JWT authentication with role-based authorization
- Automated unit testing
- CI/CD pipeline with SonarCloud static analysis
- Dockerized backend, automated cloud deployment on Azure
- A full Angular Material admin UI consuming the API end-to-end

> For a deep, line-by-line technical walkthrough of the whole system (architecture, request flow, EF Core, auth, tests, Docker, CI/CD, known limitations), see [`SYSTEM_AUDIT_LEARNING.md`](./SYSTEM_AUDIT_LEARNING.md).

---

# ⚡ Features

- JWT authentication with `Admin` / `Operator` roles
- User management (Admin-only)
- Customer management
- Product management
- Order management with a real status workflow and cached reads
- Angular admin UI (Material) with guards, interceptors and reactive forms
- Automated unit testing
- Code quality monitoring (SonarCloud)
- CI/CD pipeline
- Cloud deployment

---

# 🏗 Architecture

The backend follows the **Clean Architecture** pattern; the frontend is a separate Angular application consuming the API over HTTP.

```bash
Order-Management-System-OMS-
│
├── Source
│   ├── OrderManagement.API              # Controllers, Program.cs, JWT/CORS/Swagger config
│   ├── OrderManagement.Application      # Services (use cases), validators, cache, DI
│   ├── OrderManagement.Domain           # Entities, value objects, business rules
│   ├── OrderManagement.Infrastructure   # EF Core DbContext, repositories, JWT, hashing
│   └── OrderManagement.Tests            # xUnit unit tests
│
├── Shared
│   └── OrderManagement.Communication    # DTOs and Responses (API contract)
│
└── frontend                             # Angular 22 + Angular Material admin UI
```

### Domain
Core business entities and rules (e.g. `Order` enforces its own state transitions and can't be modified once it's no longer `Pending`).

### Application
Application services (use cases), FluentValidation validators, caching, AutoMapper-backed mapping.

### Infrastructure
EF Core `DbContext`, repositories, JWT generation, password hashing (BCrypt), migrations.

### API
REST endpoints, JWT authentication, CORS, Swagger, global exception handling (`ProblemDetails`).

### Frontend (`frontend/`)
Angular 22 standalone app with Angular Material, JWT-aware HTTP interceptors, route guards (auth + role), and CRUD screens for Orders, Customers, Products and Users.

---

# 🧰 Tech Stack

### Backend

- ASP.NET Core 8 / C#
- Entity Framework Core 8
- REST API
- JWT Authentication + BCrypt password hashing
- FluentValidation, AutoMapper, Serilog, in-memory caching

### Database

- **SQL Server** (via `Microsoft.EntityFrameworkCore.SqlServer`)
- EF Core Migrations

### Frontend

- Angular 22 (standalone components, signals, new control-flow syntax)
- Angular Material + Angular CDK (Material Icons for the whole UI)
- Reactive Forms, functional guards/interceptors

### Testing

- xUnit, Moq, FluentAssertions, Coverlet

### DevOps

- Docker (multi-stage) + Docker Compose (API + SQL Server)
- GitHub Actions (CI/CD)
- SonarCloud (static analysis + coverage)
- Azure Container Registry + Azure App Service

---

# 📚 API Documentation

The API includes interactive documentation via **Swagger**.

After running the backend locally:

```
http://localhost:5187/swagger
```

---

# 🔐 Authentication

The API uses **JWT (JSON Web Token)** authentication with two roles: `Admin` and `Operator`.

Flow:

1. Log in with `POST /api/Auth/login`
2. Receive a JWT access token
3. Send it on every protected request

```
Authorization: Bearer {token}
```

There is **no public self-registration endpoint**. New users are created by an authenticated `Admin` via `POST /api/User`. A seed admin user is created automatically on first run:

```
email: admin@admin.com
password: 123456
```

⚠️ Change this seed user before using this project anywhere beyond local development/portfolio purposes.

---

# 📡 API Endpoints

Route segments match the controller name (singular). All list/detail GET endpoints are `[AllowAnonymous]`; every write operation requires a valid JWT, and most require the `Admin` role.

## Auth — `/api/Auth`

| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/Auth/login` | Public |

## Users — `/api/User` (all endpoints require role `Admin`)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/User` | Create user |
| GET | `/api/User` | List users |
| GET | `/api/User/{id}` | Get user by id |
| PUT | `/api/User/{id}` | Update user |
| DELETE | `/api/User/{id}` | Delete user |

## Customers — `/api/Customer`

| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/Customer` | Admin |
| GET | `/api/Customer` | Public |
| GET | `/api/Customer/{id}` | Public |
| PUT | `/api/Customer/{id}` | Admin |
| DELETE | `/api/Customer/{id}` | Admin |

## Products — `/api/Product`

| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/Product` | Admin |
| GET | `/api/Product` | Public |
| GET | `/api/Product/{id}` | Public |
| PUT | `/api/Product/{id}` | Admin |
| DELETE | `/api/Product/{id}` | Admin |

## Orders — `/api/Order`

| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/Order` | Admin |
| GET | `/api/Order` | Public |
| GET | `/api/Order/{id}` | Public |
| PUT | `/api/Order/{id}` | Admin |
| DELETE | `/api/Order/{id}` | Admin |

Orders follow a real status workflow enforced by the domain: `Pending → Paid → Shipped`, with `Cancelled` reachable from `Pending`/`Paid`. Items can only be added/edited/removed while an order is `Pending`.

---

# 🧪 Automated Testing

The project includes **167 automated unit tests** covering the domain entities and application services (Order's state machine is the most thoroughly tested piece of the codebase).

Tooling:

- xUnit
- FluentAssertions
- Moq
- Coverlet

Test coverage is monitored continuously by **SonarCloud**. There are currently no integration tests (no `WebApplicationFactory`/end-to-end tests) — see [`SYSTEM_AUDIT_LEARNING.md`](./SYSTEM_AUDIT_LEARNING.md) for a full breakdown of test coverage gaps.

---

# ⚙️ CI/CD Pipeline

The project uses **GitHub Actions**.

```
Commit
→
Build
→
Run Tests (with coverage)
→
SonarCloud Analysis
→
Docker build + push (Azure Container Registry)
→
Deploy to Azure App Service
```

---

# ☁️ Cloud Deployment

The backend is deployed using **Azure App Service**, with the container image published to **Azure Container Registry**.

Infrastructure:

- Azure App Service
- Azure SQL Database
- Azure Container Registry
- GitHub Actions

---

# 💻 Running Locally

## 1. Clone the repository

```bash
git clone https://github.com/GuicesarS/Order-Management-System-OMS-.git
cd Order-Management-System-OMS-
```

## 2. Backend (API)

### Option A — Docker Compose (API + SQL Server)

```bash
cp .env.example .env
# edit .env if you want different credentials/ports
docker-compose up --build
```

The API will be available at `http://localhost:7212`.

### Option B — `dotnet run` against a local SQL Server

Make sure a SQL Server instance is reachable and matches the connection string in `Source/OrderManagement.API/appsettings.Development.json` (defaults to `localhost,1433`, database `OrderManagementDb`, user `sa`). Then:

```bash
dotnet restore
dotnet run --project Source/OrderManagement.API
```

The API will be available at `http://localhost:5187` (see `Source/OrderManagement.API/Properties/launchSettings.json` for the exact ports). On startup, the API automatically applies EF Core migrations and seeds the admin user described above.

### Swagger

```
http://localhost:5187/swagger
```

## 3. Frontend (Angular)

```bash
cd frontend
npm install
npm start   # ng serve
```

The app will be available at `http://localhost:4200`, already pointed at `http://localhost:5187/api` (see `frontend/src/environments/environment.development.ts`). The backend's CORS policy already allows this origin by default (`Cors:AllowedOrigins` in `appsettings.Development.json`).

Log in with the seed admin (`admin@admin.com` / `123456`) to access every screen, including the Admin-only Users section.

---

# 🖥️ Frontend

The `frontend/` folder contains a standalone **Angular 22 + Angular Material** admin application that consumes the API end-to-end:

- JWT-based login, with the token decoded client-side to drive route guards (`authGuard`, `roleGuard`) and to conditionally show Admin-only navigation/actions.
- A functional `HttpInterceptorFn` attaches the `Authorization: Bearer` header to every request, and a second interceptor centralizes error handling (`ProblemDetails` → snackbar notifications, auto-logout on `401`).
- CRUD screens for **Orders**, **Customers**, **Products** and **Users**, built with `mat-table` (client-side pagination/sorting, since the API doesn't paginate), reactive forms, and Material dialogs for delete confirmation.
- The Order form uses a `FormArray` to manage line items, and the Order detail screen exposes status-transition actions (Mark as Paid / Shipped / Cancel) that respect the same state machine enforced by the backend domain.

---

# 👨‍💻 Author

**Guilherme César Soares**

Backend Developer — C# / .NET

GitHub
https://github.com/GuicesarS

SonarCloud
https://sonarcloud.io/project/overview?id=GuicesarS_Order-Management-System-OMS-
