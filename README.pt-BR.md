# 🧾 Order Management System (OMS)

<a href="./README.md">🇺🇸 English</a> → 🇧🇷 Português

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

# 🚀 Visão Geral

O **Order Management System (OMS)** é uma aplicação **full-stack** composta por uma **API REST em ASP.NET Core 8** (Clean Architecture) e um **frontend em Angular 22 + Angular Material**, projetada para simular um fluxo real de gestão de pedidos usado em empresas modernas.

O projeto demonstra diversas **boas práticas de engenharia de software**, incluindo:

- Clean Architecture (Domain / Application / Infrastructure / API)
- Modelo de domínio rico, com uma máquina de estados real para o pedido (`Pending → Paid → Shipped` / `Cancelled`)
- Autenticação JWT com autorização baseada em papéis (roles)
- Testes unitários automatizados
- Pipeline de CI/CD com análise estática via SonarCloud
- Backend containerizado com Docker e deploy automatizado no Azure
- Uma interface administrativa completa em Angular Material consumindo a API de ponta a ponta

> Para uma explicação técnica completa, linha a linha, de todo o sistema (arquitetura, fluxo de request, EF Core, autenticação, testes, Docker, CI/CD, limitações conhecidas), veja [`SYSTEM_AUDIT_LEARNING.md`](./SYSTEM_AUDIT_LEARNING.md).

---

# ⚡ Funcionalidades

- Autenticação JWT com papéis `Admin` / `Operator`
- Gerenciamento de usuários (somente Admin)
- Gerenciamento de clientes
- Gerenciamento de produtos
- Gerenciamento de pedidos com fluxo de status real e leituras em cache
- Interface administrativa em Angular (Material) com guards, interceptors e formulários reativos
- Testes unitários automatizados
- Monitoramento de qualidade de código (SonarCloud)
- Pipeline CI/CD
- Deploy em nuvem

---

# 🏗 Arquitetura

O backend segue o padrão **Clean Architecture**; o frontend é uma aplicação Angular separada que consome a API via HTTP.

```bash
Order-Management-System-OMS-
│
├── Source
│   ├── OrderManagement.API              # Controllers, Program.cs, config de JWT/CORS/Swagger
│   ├── OrderManagement.Application      # Services (casos de uso), validators, cache, DI
│   ├── OrderManagement.Domain           # Entidades, value objects, regras de negócio
│   ├── OrderManagement.Infrastructure   # DbContext do EF Core, repositories, JWT, hashing
│   └── OrderManagement.Tests            # Testes unitários (xUnit)
│
├── Shared
│   └── OrderManagement.Communication    # DTOs e Responses (contrato da API)
│
└── frontend                             # Interface administrativa em Angular 22 + Angular Material
```

### Domain
Entidades e regras de negócio centrais (ex.: `Order` impõe suas próprias transições de estado e não pode ser modificado depois de deixar de estar `Pending`).

### Application
Services de aplicação (casos de uso), validators do FluentValidation, cache, mapeamento via AutoMapper.

### Infrastructure
`DbContext` do EF Core, repositories, geração de JWT, hashing de senha (BCrypt), migrations.

### API
Endpoints REST, autenticação JWT, CORS, Swagger, tratamento global de exceções (`ProblemDetails`).

### Frontend (`frontend/`)
Aplicação Angular 22 standalone com Angular Material, interceptors HTTP cientes de JWT, guards de rota (autenticação + papel) e telas de CRUD para Pedidos, Clientes, Produtos e Usuários.

---

# 🧰 Tecnologias Utilizadas

### Backend

- ASP.NET Core 8 / C#
- Entity Framework Core 8
- API REST
- Autenticação JWT + hashing de senha com BCrypt
- FluentValidation, AutoMapper, Serilog, cache em memória

### Banco de Dados

- **SQL Server** (via `Microsoft.EntityFrameworkCore.SqlServer`)
- EF Core Migrations

### Frontend

- Angular 22 (standalone components, signals, nova sintaxe de control-flow)
- Angular Material + Angular CDK (Material Icons em toda a interface)
- Reactive Forms, guards/interceptors funcionais

### Testes

- xUnit, Moq, FluentAssertions, Coverlet

### DevOps

- Docker (multi-stage) + Docker Compose (API + SQL Server)
- GitHub Actions (CI/CD)
- SonarCloud (análise estática + cobertura)
- Azure Container Registry + Azure App Service

---

# 📚 Documentação da API

A API possui documentação interativa via **Swagger**.

Após rodar o backend localmente:

```
http://localhost:5187/swagger
```

---

# 🔐 Autenticação

A API utiliza autenticação **JWT (JSON Web Token)** com dois papéis: `Admin` e `Operator`.

Fluxo:

1. Fazer login com `POST /api/Auth/login`
2. Receber um token JWT
3. Enviá-lo em todo request protegido

```
Authorization: Bearer {token}
```

**Não existe endpoint público de autocadastro.** Novos usuários são criados por um `Admin` autenticado via `POST /api/User`. Um usuário Admin de seed é criado automaticamente na primeira execução:

```
email: admin@admin.com
senha: 123456
```

⚠️ Troque esse usuário de seed antes de usar este projeto além de fins de portfólio/desenvolvimento local.

---

# 📡 Endpoints da API

Os segmentos de rota seguem o nome do controller (no singular). Todos os GETs de listagem/detalhe são `[AllowAnonymous]`; toda operação de escrita exige um JWT válido, e a maioria exige o papel `Admin`.

## Auth — `/api/Auth`

| Método | Endpoint | Autenticação |
|---|---|---|
| POST | `/api/Auth/login` | Pública |

## Usuários — `/api/User` (todos os endpoints exigem papel `Admin`)

| Método | Endpoint | Descrição |
|---|---|---|
| POST | `/api/User` | Criar usuário |
| GET | `/api/User` | Listar usuários |
| GET | `/api/User/{id}` | Buscar usuário por id |
| PUT | `/api/User/{id}` | Atualizar usuário |
| DELETE | `/api/User/{id}` | Remover usuário |

## Clientes — `/api/Customer`

| Método | Endpoint | Autenticação |
|---|---|---|
| POST | `/api/Customer` | Admin |
| GET | `/api/Customer` | Pública |
| GET | `/api/Customer/{id}` | Pública |
| PUT | `/api/Customer/{id}` | Admin |
| DELETE | `/api/Customer/{id}` | Admin |

## Produtos — `/api/Product`

| Método | Endpoint | Autenticação |
|---|---|---|
| POST | `/api/Product` | Admin |
| GET | `/api/Product` | Pública |
| GET | `/api/Product/{id}` | Pública |
| PUT | `/api/Product/{id}` | Admin |
| DELETE | `/api/Product/{id}` | Admin |

## Pedidos — `/api/Order`

| Método | Endpoint | Autenticação |
|---|---|---|
| POST | `/api/Order` | Admin |
| GET | `/api/Order` | Pública |
| GET | `/api/Order/{id}` | Pública |
| PUT | `/api/Order/{id}` | Admin |
| DELETE | `/api/Order/{id}` | Admin |

Os pedidos seguem um fluxo de status real imposto pelo domínio: `Pending → Paid → Shipped`, com `Cancelled` alcançável a partir de `Pending`/`Paid`. Itens só podem ser adicionados/editados/removidos enquanto o pedido está `Pending`.

---

# 🧪 Testes Automatizados

O projeto possui **167 testes unitários automatizados**, cobrindo as entidades de domínio e os services de aplicação (a máquina de estados do `Order` é a parte mais testada do código).

Ferramentas:

- xUnit
- FluentAssertions
- Moq
- Coverlet

A cobertura de testes é monitorada continuamente pelo **SonarCloud**. Atualmente não há testes de integração (sem `WebApplicationFactory`/testes ponta a ponta) — veja [`SYSTEM_AUDIT_LEARNING.md`](./SYSTEM_AUDIT_LEARNING.md) para o detalhamento completo das lacunas de cobertura.

---

# ⚙️ Pipeline CI/CD

O projeto utiliza **GitHub Actions**.

```
Commit
→
Build
→
Execução de Testes (com cobertura)
→
Análise SonarCloud
→
Build e push da imagem Docker (Azure Container Registry)
→
Deploy no Azure App Service
```

---

# ☁️ Deploy em Nuvem

O backend é implantado utilizando **Azure App Service**, com a imagem do container publicada no **Azure Container Registry**.

Infraestrutura:

- Azure App Service
- Azure SQL Database
- Azure Container Registry
- GitHub Actions

---

# 💻 Executar o Projeto Localmente

## 1. Clonar o repositório

```bash
git clone https://github.com/GuicesarS/Order-Management-System-OMS-.git
cd Order-Management-System-OMS-
```

## 2. Backend (API)

### Opção A — Docker Compose (API + SQL Server)

```bash
cp .env.example .env
# edite o .env se quiser credenciais/portas diferentes
docker-compose up --build
```

A API ficará disponível em `http://localhost:7212`.

### Opção B — `dotnet run` contra um SQL Server local

Garanta que uma instância de SQL Server esteja acessível e bata com a connection string em `Source/OrderManagement.API/appsettings.Development.json` (padrão: `localhost,1433`, banco `OrderManagementDb`, usuário `sa`). Depois:

```bash
dotnet restore
dotnet run --project Source/OrderManagement.API
```

A API ficará disponível em `http://localhost:5187` (veja `Source/OrderManagement.API/Properties/launchSettings.json` para as portas exatas). Na inicialização, a API aplica as migrations do EF Core automaticamente e cria o usuário admin de seed descrito acima.

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

A aplicação ficará disponível em `http://localhost:4200`, já apontando para `http://localhost:5187/api` (veja `frontend/src/environments/environment.development.ts`). A política de CORS do backend já libera essa origem por padrão (`Cors:AllowedOrigins` em `appsettings.Development.json`).

Faça login com o admin de seed (`admin@admin.com` / `123456`) para acessar todas as telas, incluindo a seção de Usuários (somente Admin).

---

# 🖥️ Frontend

A pasta `frontend/` contém uma aplicação administrativa standalone em **Angular 22 + Angular Material** que consome a API de ponta a ponta:

- Login via JWT, com o token decodificado no próprio cliente para alimentar os guards de rota (`authGuard`, `roleGuard`) e exibir condicionalmente navegação/ações restritas a Admin.
- Um `HttpInterceptorFn` funcional anexa o header `Authorization: Bearer` em todo request, e um segundo interceptor centraliza o tratamento de erros (`ProblemDetails` → notificações via snackbar, logout automático em `401`).
- Telas de CRUD para **Pedidos**, **Clientes**, **Produtos** e **Usuários**, construídas com `mat-table` (paginação/ordenação no lado do cliente, já que a API não pagina), formulários reativos e diálogos do Material para confirmação de exclusão.
- O formulário de pedido usa um `FormArray` para gerenciar os itens, e a tela de detalhe do pedido expõe ações de transição de status (Marcar como Pago / Enviado / Cancelar) respeitando a mesma máquina de estados imposta pelo domínio no backend.

---

# 👨‍💻 Autor

**Guilherme César Soares**

Desenvolvedor Backend focado em C#, .NET e APIs REST, interessado em arquitetura de software, boas práticas de engenharia e desenvolvimento de aplicações escaláveis.

GitHub
https://github.com/GuicesarS

SonarCloud
https://sonarcloud.io/project/overview?id=GuicesarS_Order-Management-System-OMS-
