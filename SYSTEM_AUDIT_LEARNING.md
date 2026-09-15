# SYSTEM_AUDIT_LEARNING.md — Manual de Reaprendizado do OMS

> Documento gerado em auditoria READ-ONLY. Nenhum arquivo de código foi alterado. Todas as afirmações abaixo são baseadas em evidência direta do código lido em `Order-Management-System-OMS-/`.

---

## 1. Visão Geral

O **OMS (Order Management System)** é uma API REST em **ASP.NET Core 8 / C#**, organizada em **Clean Architecture** com 4 camadas (`Domain`, `Application`, `Infrastructure`, `API`) mais um projeto compartilhado de contratos (`Shared/OrderManagement.Communication`). Ele gerencia 4 entidades de negócio — `User`, `Customer`, `Product`, `Order` (com `OrderItem`) — com autenticação via JWT, persistência via EF Core, cache em memória, validação com FluentValidation, mapeamento com AutoMapper, testes unitários com xUnit/Moq/FluentAssertions, containerização com Docker, e um pipeline CI/CD via GitHub Actions com SonarCloud e deploy no Azure.

O projeto **não é um CRUD raso**: `Order` tem uma máquina de estados real no domínio (`Pending → Paid → Shipped`, mais `Cancelled`), com regras que impedem edição de pedidos não-pendentes e transições inválidas. Isso é o núcleo mais maduro do sistema.

---

## 2. Inventário Real

```text
OMS
│
├── Order-Management-System-OMS-/          ← repositório git real
│   ├── OrderManagementSystem.sln
│   ├── README.md / README.pt-BR.md
│   ├── docker-compose.yml
│   ├── dockerfile
│   ├── .env.example
│   ├── .gitignore
│   │
│   ├── .github/workflows/
│   │   ├── ci.yml                          ← build + test + SonarCloud
│   │   └── cd.yml                          ← build imagem + deploy Azure
│   │
│   ├── Shared/OrderManagement.Communication/   ← DTOs e Responses (contratos da API)
│   │   ├── Dtos/{Auth,Customer,Order,OrderItem,Product,User}/
│   │   └── Responses/
│   │
│   └── Source/
│       ├── OrderManagement.Domain/         ← entidades, value objects, regras, interfaces
│       ├── OrderManagement.Application/    ← services (casos de uso), validators, cache, DI
│       ├── OrderManagement.Infrastructure/ ← DbContext, repositories, JWT, hashing, migrations
│       ├── OrderManagement.API/            ← controllers, Program.cs, middlewares, Swagger
│       └── OrderManagement.Tests/          ← testes unitários (xUnit)
```

**Responsabilidade de cada projeto (confirmada pelo código, não pelo README):**

- **`OrderManagement.Domain`**: entidades ricas (`Order`, `OrderItem`, `Product`, `Customer`, `User`), value objects (`Email`, `Phone`), enums (`OrderStatus`, `UserRole`), interfaces de repositório e de segurança (`IPasswordHasher`, `IAccessTokenGenerator`). Não depende de mais nenhum projeto — é o centro da Clean Architecture.
- **`OrderManagement.Application`**: um `Service` por entidade (`OrderService`, `UserService`, `CustomerService`, `ProductService`, `AuthService`), interfaces desses services, `Result<T>` (wrapper de resultado), `ICustomMapper`/`CustomMapper` (fachada sobre AutoMapper), `ICacheService`/`CacheService`, validators FluentValidation, exceções de aplicação.
- **`OrderManagement.Infrastructure`**: `OrderManagementDbContext` (EF Core), repositories (implementam as interfaces do Domain), `PasswordHasher` (BCrypt), `AccessTokenGenerator` (JWT), migrations, seed de banco.
- **`OrderManagement.API`**: controllers REST, `Program.cs` (composição raiz), configuração de autenticação JWT, Swagger, `GlobalExceptionHandler`, `MappingProfile` do AutoMapper (fica na API, não na Application — detalhe não óbvio).
- **`Shared/OrderManagement.Communication`**: DTOs de entrada e Responses de saída — é o "contrato" que tanto a API quanto (futuramente) um cliente Angular consumiriam.
- **`OrderManagement.Tests`**: só testes unitários — nenhum projeto de teste de integração existe.

---

## 3. Stack Detectada

```text
Backend
- .NET 8 / C# (net8.0 em todos os .csproj)
- ASP.NET Core 8 (Microsoft.NET.Sdk.Web no OrderManagement.API.csproj)
- Minimal hosting model (Program.cs top-level, sem Startup.cs)

Persistência
- Entity Framework Core 8.0.2
- Pomelo.EntityFrameworkCore.MySql (usado de fato em Program.cs:59 — UseMySql)
- Microsoft.EntityFrameworkCore.SqlServer (referenciado no .csproj mas NÃO usado no código de configuração)
- 1 migration existente: 20260312225632_InitialCreate

Mapeamento
- AutoMapper 12.0.1 + AutoMapper.Extensions.Microsoft.DependencyInjection

Validação
- FluentValidation 12.1.1 (Application) / FluentValidation.AspNetCore 11.3.1 (API)
- Validators manuais complementares dentro dos próprios Services (ex.: OrderService.ValidateProducts)

Segurança
- JWT via Microsoft.AspNetCore.Authentication.JwtBearer 8.0.2 + System.IdentityModel.Tokens.Jwt
- BCrypt.Net-Next 4.0.3 para hashing de senha

Cache
- Microsoft.Extensions.Caching.Memory (IMemoryCache) — cache em memória do processo, não Redis

Logging
- Serilog 4.3.0 + Serilog.AspNetCore + Sinks Console e File (logs/ordermanagementapi-.log)

Documentação de API
- Swashbuckle.AspNetCore 6.6.1 (Swagger/OpenAPI) com suporte a Bearer JWT

Testes
- xUnit 2.9.3, Moq 4.20.72, FluentAssertions 7.0.0, Bogus 35.6.5 (builders), coverlet.msbuild 8.0.0

DevOps
- Docker (multi-stage: SDK 8.0 build → aspnet:8.0 runtime)
- Docker Compose (API + banco relacional)
- GitHub Actions (ci.yml: build/test/Sonar; cd.yml: build imagem + push ACR + deploy Azure App Service)
- SonarCloud (dotnet-sonarscanner no ci.yml)
- Azure Container Registry + Azure App Service (cd.yml)
```

**Onde cada tecnologia aparece de fato**, com caminho de arquivo, está documentado seção a seção abaixo — não é lista de README.

---

## 4. Arquitetura Explicada

O fluxo de dependências real (confirmado pelos `.csproj`, não suposto):

```text
API  →  Application  →  Domain
 ↓            ↓
Infrastructure (referencia Domain e Application)
```

`OrderManagement.API.csproj` referencia `Application`, `Infrastructure` e `Shared/Communication`. `Infrastructure.csproj` referencia `Domain` e `Application` (isso é levemente atípico — normalmente Infrastructure só depende de Domain; aqui ela também depende de Application, provavelmente porque `InfrastructureDependencyInjection.AddTokens` lê `IConfiguration` para montar o `AccessTokenGenerator`, mas o generator em si implementa uma interface do Domain). `Application.csproj` referencia só `Domain` e `Communication`. `Domain.csproj` não referencia nada — está corretamente isolado.

### Domain (`Source/OrderManagement.Domain`)

Contém entidades com **comportamento**, não apenas dados (isso é o ponto mais forte do projeto):

- `Order.cs` — entidade raiz de agregado. Construtor privado protegido para EF Core (`protected Order() {}`), construtor público que valida `customerId`. Métodos de negócio: `AddItem`, `UpdateItem`, `RemoveItem` (só permitidos quando `Status == Pending`, verificado por `ValidateOrderIsPending()`), `MarkAsPaid`, `MarkAsCancelled`, `MarkAsShipped` — cada um valida transições de estado válidas e lança `DomainValidationException` caso contrário. `RecalculateTotal()` é privado e chamado sempre que os itens mudam — o total nunca é calculado "de fora".
- `OrderItem.cs` — entidade filha, recalcula `LineTotal` internamente.
- `Product.cs`, `Customer.cs`, `User.cs` — entidades com setters privados e validação no construtor/métodos `Update*`.
- Value Objects: `Email` (namespace `ValueObjects`) — construtor privado, criado só via `Email.Create(string)`, que usa `System.Net.Mail.MailAddress` para validar formato e normaliza para minúsculas. `Phone` segue padrão semelhante (não lido linha a linha, mas referenciado da mesma forma em `Customer`).
- `Exception/DomainValidationException.cs` — exceção específica de violação de regra de domínio.
- `Interfaces/` — contratos de repositório (`IOrderRepository`, `ICustomerRepository`, etc.) e de segurança (`IPasswordHasher`, `IAccessTokenGenerator`) **definidos no Domain, implementados na Infrastructure** — essa é a Dependency Inversion clássica de Clean Architecture.

### Application (`Source/OrderManagement.Application`)

- Um `*Service` por entidade, implementando uma interface (`IOrderService`, etc.), registrado como `Scoped` em `ApplicationDependencyInjection.cs`.
- `Common/Result.cs` — wrapper `Result<T>` com `Success`, `ErrorMessage`, `Data`, e factory methods `Ok`/`Failure`.
- `Common/CustomMapping/CustomMapper.cs` — uma fachada fina (`ICustomMapper`) em cima do `IMapper` do AutoMapper. Motivo provável: desacoplar os Services do pacote AutoMapper diretamente (para poder trocar de biblioteca de mapeamento sem tocar nos Services) — mas isso não é 100% alcançado, porque o `MappingProfile` real do AutoMapper vive na camada API (acoplamento cruzado incomum).
- `Cache/CacheKeys.cs` e `Cache/CachePolicies.cs` — chaves e políticas de expiração centralizadas para o cache em memória.
- `Exceptions/` — `NotFoundException`, `UnauthorizedException`, `ValidationException` (aplicação) — diferentes de `DomainValidationException` (domínio). Ver seção 9 e 11 para a diferença.
- `Validators/` — um `AbstractValidator<T>` do FluentValidation por DTO de criação/atualização.

### Infrastructure (`Source/OrderManagement.Infrastructure`)

- `Data/OrderManagementDbContext.cs` — `DbSet` para `User`, `Customer`, `Product`, `Order`, `OrderItem`; `OnModelCreating` configura tudo via Fluent API (nenhum uso de Data Annotations nas entidades — as entidades do Domain ficam limpas de EF).
- `Data/Seed/DatabaseSeeder.cs` — cria um usuário Admin (`admin@admin.com` / senha `123456`, hasheada com BCrypt) se não existir nenhum usuário. Chamado em `Program.cs:87` a cada start.
- `Repositories/` — implementações simples, uma classe por entidade, todas usando `_dbContext.Set.AddAsync/Update/Remove/SaveChangesAsync`. `OrderRepository.GetAllAsync/GetOrderByIdAsync` usam `.AsNoTracking().Include(o => o.Items)`.
- `Security/Cryptography/PasswordHasher.cs` — BCrypt.
- `Security/Token/AccessTokenGenerator.cs` — gera JWT com claims `NameIdentifier` (user id) e `Role`.
- `Migrations/` — uma única migration (`InitialCreate`), gerada em 2026-03-12.

### API (`Source/OrderManagement.API`)

- `Controllers/` — 5 controllers: `AuthController`, `UserController`, `CustomerController`, `ProductController`, `OrderController`. Todos `[ApiController]`, a maioria `[Authorize]` na classe com overrides `[Authorize(Roles = "Admin")]` ou `[AllowAnonymous]` por action.
- `Program.cs` — composição raiz: Serilog, AutoMapper, `ICustomMapper`, FluentValidation, `ProblemDetails` + `GlobalExceptionHandler`, `AddApplication()`, `AddInfrastructure()`, `AddJwtAuthentication()`, `AddSwaggerWithJwt()`, `AddDbContext<OrderManagementDbContext>` com MySQL, pipeline HTTP (`UseSwagger`, `UseHttpsRedirection`, `UseExceptionHandler`, `UseAuthentication`, `UseAuthorization`, `MapControllers`), migração automática (`context.Database.Migrate()`) e seed no boot.
- `Configurations/Authentication/AuthenticationConfiguration.cs` — configura `JwtBearer`.
- `Configurations/Swagger/SwaggerConfiguration.cs` — Swagger com botão "Authorize" (Bearer).
- `Configurations/Mapping/MappingProfile.cs` — todos os `CreateMap` do AutoMapper.
- `Handlers/GlobalExceptionHandler.cs` — implementa `IExceptionHandler` (padrão novo do .NET 8, não filtro de exceção clássico).

---

## 5. Fluxo Completo de uma Request — Criar Pedido (`POST /api/orders`)

```text
HTTP REQUEST
      ↓
OrderController.CreateOrder()                Source/OrderManagement.API/Controllers/OrderController.cs:25
      ↓
[Authorize(Roles = "Admin")] valida JWT + role
      ↓
Model binding → CreateOrderDto                Shared/OrderManagement.Communication/Dtos/Order/CreateOrderDto.cs
      ↓
FluentValidation automático (AddFluentValidationAutoValidation)
   → CreateOrderDtoValidator                  Source/OrderManagement.Application/Validators/Order/CreateOrderDtoValidator.cs
   → valida CustomerId não vazio e Items não vazio
   → se inválido: 400 automático (ainda antes do controller rodar a lógica)
      ↓
_orderService.Create(requestDto)              Source/OrderManagement.Application/Services/OrderService.cs:42
      ↓
1. Busca customer via ICustomerRepository.GetCustomerById — se null, throw ValidationException
2. Para cada item do DTO, busca produto via IProductRepository.GetProductById — se algum não existir, throw ValidationException
3. new Order(orderDto.CustomerId)              ← construtor de domínio valida CustomerId != Guid.Empty
4. Para cada item: order.AddItem(productId, quantity, unitPrice)
      ↓ (dentro do Domain)
   Order.AddItem() valida Status == Pending, quantity >= 1, unitPrice > 0
   Cria new OrderItem(...) (valida de novo dentro do próprio OrderItem)
   RecalculateTotal() soma LineTotal de todos os itens
      ↓
5. await _repository.AddAsync(order)           Source/OrderManagement.Infrastructure/Repositories/OrderRepository.cs:17
      ↓
   _dbContext.Orders.AddAsync(order); await _dbContext.SaveChangesAsync();
      ↓ (EF Core traduz para INSERT em Orders + OrderItems, via cascade configurado em OnModelCreating)
   MySQL (Pomelo.EntityFrameworkCore.MySql)
      ↓
6. await _cacheService.RemoveAsync(CacheKeys.Orders.GetAllOrders)   ← invalida cache de listagem
      ↓
7. _mapper.Map<Order, OrderResponse>(order)     Source/OrderManagement.API/Configurations/Mapping/MappingProfile.cs:48
      ↓
8. Result<OrderResponse>.Ok(orderResponse)
      ↓
De volta no Controller:
   if (!result.Success) return BadRequest(...)   ← nunca acontece de fato aqui (ver seção 19, achado sobre Result vs exceptions)
   return CreatedAtAction(nameof(GetOrderById), new { id = result.Data.Id }, result.Data)
      ↓
HTTP 201 Created, header Location apontando para GET /api/orders/{id}, corpo = OrderResponse
```

**Observação didática importante**: repare que existem **dois mecanismos de erro convivendo**: o `Result<T>` (que teria um `Failure`) e exceções lançadas diretamente (`throw new ValidationException(...)`). Na prática, **toda falha de validação de negócio no `OrderService` é feita via `throw`, nunca via `Result.Failure`** — o `Result<T>.Failure` existe na classe mas não é chamado em nenhum dos Services lidos. Isso significa que o `if (!result.Success)` nos controllers é código morto para esses casos: a exceção já teria sido capturada pelo `GlobalExceptionHandler` antes de retornar ao controller. Ver seção 19 (Qualidade) e 20 (Dívida Técnica).

---

## 6. Como a Autenticação Funciona

```text
Login (POST /api/auth/login)
 ↓
AuthController.Login(LoginDto)                Source/OrderManagement.API/Controllers/AuthController.cs
 ↓
_authService.Login(login)                      Source/OrderManagement.Application/Services/AuthService.cs:32
 ↓
1. _userRepository.GetUserByEmailAsync(login.Email) — se null → throw UnauthorizedException("Invalid credentials.")
2. _passwordHasher.Verify(login.Password, user.PasswordHash)   ← BCrypt.Verify
   se inválido → throw UnauthorizedException("Invalid credentials.")
3. _accessToken.Generate(user)                 Source/OrderManagement.Infrastructure/Security/Token/AccessTokenGenerator.cs:21
      claims: ClaimTypes.NameIdentifier = user.Id, ClaimTypes.Role = user.Role
      SigningCredentials: HmacSha256 com chave simétrica de Settings:Jwt:SigningKey
      Expires: DateTime.UtcNow.AddMinutes(Settings:Jwt:ExpirationTimeMinutes)
4. Result<AuthResponse>.Ok(new AuthResponse { AccessToken = token })
 ↓
200 OK { accessToken: "..." }
```

**Uso do token em requests protegidas:**

```text
Header: Authorization: Bearer {token}
 ↓
Middleware app.UseAuthentication()             Program.cs:76 — antes de UseAuthorization
 ↓
JwtBearer valida assinatura (mesma SigningKey), valida Lifetime (ClockSkew = TimeSpan.Zero, ou seja, sem tolerância)
   ValidateIssuer = false, ValidateAudience = false   ← não valida issuer/audience (ver seção 19: aceitável para projeto de portfólio, mas seria endurecido em produção real)
 ↓
app.UseAuthorization()                          Program.cs:77
 ↓
[Authorize] na classe do controller exige qualquer usuário autenticado
[Authorize(Roles = "Admin")] em actions específicas (Create/Update/Delete de Order, Customer, Product) exige role Admin
[AllowAnonymous] em GETs de leitura pública (GetOrderById, GetAllOrders, GetCustomerById, GetAllCustomers, GetProducById, GetAllProducts)
```

**O que NÃO existe** (confirmado pela ausência no código, não suposição):

- **Não há endpoint de registro público** (`/register`). O README (seção "User Endpoints") descreve `POST /api/users/register`, mas o `UserController` real está em `[Authorize(Roles = "Admin")]` na classe inteira e o endpoint de criação é `POST /api/users`, não `/register` — **o README está desatualizado em relação ao código real**. Ou seja: hoje, só um Admin autenticado pode criar outro usuário. O único usuário inicial vem do `DatabaseSeeder` (`admin@admin.com` / `123456`).
- **Não há refresh token.**
- **Não há revogação/blacklist de token** — um token válido continua válido até expirar, mesmo se a senha do usuário mudar depois.
- **Roles**: só duas — `Admin` e `Operator` (`Source/OrderManagement.Domain/Enums/UserRole.cs`). `Operator` é definido mas nenhum endpoint referencia `[Authorize(Roles = "Operator")]` — o papel existe no domínio mas não é usado em nenhuma regra de autorização hoje.

---

## 7. Entity Framework / Banco de Dados

```text
Application
     ↓
IOrderRepository (Domain/Interfaces)
     ↓
OrderRepository (Infrastructure/Repositories) — implementação concreta
     ↓
OrderManagementDbContext (Infrastructure/Data)
     ↓
Pomelo.EntityFrameworkCore.MySql → MySQL
```

**Não há Unit of Work explícito** — cada método de repositório chama `SaveChangesAsync()` diretamente. Isso funciona bem porque cada operação de negócio (`Create`, `Update`, `Delete` de uma entidade) só toca uma raiz de agregado por vez neste projeto — não há cenário real onde múltiplos repositories precisem ser salvos atomicamente na mesma transação, então a ausência de UoW não é um problema prático aqui, mas seria uma limitação se o sistema crescesse.

**Relacionamentos configurados em `OnModelCreating` (`OrderManagementDbContext.cs`):**
- `User.Email` e `Customer.Email`/`Phone` são **Owned Types** (`OwnsOne`) — os Value Objects viram colunas na própria tabela (`Email` como coluna simples), não tabelas separadas.
- `Order` 1—N `OrderItem`, com `DeleteBehavior.Cascade` (apagar um Order apaga seus itens).
- `Order` N—1 `Customer`, com `DeleteBehavior.Cascade` (apagar um Customer apaga seus Orders — pode ser um risco de perda de dados não intencional, ver seção 19).
- `OrderItem` N—1 `Product`, com `DeleteBehavior.Restrict` (não deixa apagar um Product referenciado em algum item).
- `Price`/`TotalAmount`/`UnitPrice` mapeados como `decimal(18,2)`.

**⚠️ Achado importante — inconsistência de banco de dados:**

Há uma **divergência real entre configuração declarada e configuração usada**:
- `Program.cs:59` configura EF Core explicitamente para **MySQL** (`options.UseMySql(...)`).
- `Infrastructure.csproj` referencia **tanto** `Pomelo.EntityFrameworkCore.MySql` **quanto** `Microsoft.EntityFrameworkCore.SqlServer` — mas só o pacote MySQL é usado no código.
- `docker-compose.yml` sobe um container `mcr.microsoft.com/azure-sql-edge`, que é **SQL Server**, não MySQL.
- `.env.example` tem `DB_CONNECTION_STRING=Server=sql,1433;User ID=sa;Password=...;TrustServerCertificate=true` — essa é sintaxe de **connection string do SQL Server**, incompatível com o driver Pomelo/MySQL configurado em `Program.cs`.

**Isso significa que, hoje, `docker-compose up` provavelmente NÃO funciona**: a API tentaria abrir uma connection string de SQL Server usando o driver MySQL. Isso é uma dívida técnica real, não uma opção de design — provavelmente o projeto migrou de SQL Server para MySQL em algum momento e o Docker/`.env.example` ficaram para trás. Está marcado como **BLOQUEIA** na seção 20.

---

## 8. Dependency Injection — Exemplos Reais

**Exemplo 1 — Repository:**

```text
interface IOrderRepository                    Source/OrderManagement.Domain/Interfaces/IOrderRepository.cs
   ↓
implementação OrderRepository                 Source/OrderManagement.Infrastructure/Repositories/OrderRepository.cs
   ↓
registro: services.AddScoped<IOrderRepository, OrderRepository>()
          Source/OrderManagement.Infrastructure/InfrastructureDependencyInjection.cs:19
   ↓
injeção: OrderService recebe IOrderRepository no construtor
          Source/OrderManagement.Application/Services/OrderService.cs:19,27
   ↓
uso: await _repository.AddAsync(order)
```

`OrderService` **nunca** vê `OrderRepository` (a classe concreta) nem sabe que existe EF Core por trás. Isso é o que permite trocar MySQL por outro banco (ou mockar o repositório em teste) sem tocar no `OrderService`. É exatamente o padrão que os testes unitários exploram: `OrderServiceTest` usa `Mock<IOrderRepository>`, nunca toca em banco de verdade.

**Exemplo 2 — Token JWT com dependência de configuração (mais interessante que o padrão simples):**

```text
interface IAccessTokenGenerator                Source/OrderManagement.Domain/Security/Token/IAccessTokenGenerator.cs
   ↓
implementação AccessTokenGenerator              Source/OrderManagement.Infrastructure/Security/Token/AccessTokenGenerator.cs
   ↓ (construtor precisa de expirationTimeInMinutes e signingKey, que vêm de appsettings — não são injetáveis diretamente)
registro via factory lambda:
   services.AddScoped<IAccessTokenGenerator>(provider =>
       new AccessTokenGenerator(expirationTimeInMinutes, signingKey));
   InfrastructureDependencyInjection.cs:34
```

Aqui o DI container não consegue resolver `AccessTokenGenerator` automaticamente porque o construtor recebe `int` e `string` "soltos" (não são serviços registrados) — então o registro usa uma **factory lambda** que lê `IConfiguration` uma única vez (fora do escopo por request) e captura os valores no closure. É um padrão intermediário de DI que vale entender bem, porque aparece toda vez que uma dependência precisa de valores de configuração primitivos, não de outro serviço.

Isso explica por que `AddTokens` está em `InfrastructureDependencyInjection`, não em `AuthenticationConfiguration` — motivo: `AccessTokenGenerator` fica na Infrastructure (implementa uma interface do Domain), então seu registro de DI faz mais sentido perto de onde a classe existe.

---

## 9. Validação — Entrada vs Regra de Negócio

Existem **duas camadas de validação distintas e propositalmente separadas** neste projeto:

**1. Validação de entrada (FluentValidation, Application layer)** — valida a **forma** do request antes de qualquer lógica rodar. Exemplo: `CreateOrderDtoValidator` garante que `CustomerId` não é vazio e que `Items` não é uma lista vazia. Isso roda automaticamente via `AddFluentValidationAutoValidation()` (`Program.cs:39`), **antes** do controller/service serem chamados — um request malformado nunca chega a tocar em `OrderService`.

**2. Regra de negócio (Domain, dentro das próprias entidades)** — valida **invariantes do domínio**, coisas que dependem do estado atual do objeto. Exemplo: `Order.AddItem()` não valida só `quantity >= 1` (que também poderia ser FluentValidation), mas valida `Status == Pending` — isso é uma regra que **não pode** ser expressa num validator de DTO porque depende do estado atual da entidade no banco, não do formato do request.

Há também uma **terceira camada intermediária**, dentro dos próprios `*Service` (ex.: `OrderService.ValidateProducts`, que verifica se cada produto referenciado no DTO realmente existe no banco) — isso não é nem validação de forma (FluentValidation) nem invariante pura de domínio (não cabe dentro de `Order`, porque `Order` não tem acesso a repositórios) — é **validação de aplicação** que depende de consultar outro agregado.

**Diferença prática que vale internalizar:** se a regra pode ser checada só olhando o formato do DTO → FluentValidation. Se a regra depende do estado interno do agregado → método da entidade lança `DomainValidationException`. Se a regra depende de consultar outro agregado/repositório → fica no Service e lança `ValidationException` (da Application, não a do Domain).

---

## 10. Mapeamento

```text
Request (DTO)                     Domain Entity                    Response
CreateOrderDto          →         Order (via ConstructUsing)   →    OrderResponse
CreateOrderItemDto      →         OrderItem (via ConstructUsing)
```

Tudo centralizado em **um único** `MappingProfile` (`Source/OrderManagement.API/Configurations/Mapping/MappingProfile.cs`), registrado via `builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())` (`Program.cs:30`).

Pontos didáticos:
- Mapeamento de Value Object para primitivo é feito explicitamente: `CreateMap<Email, string>().ConvertUsing(src => src.Value)` e o inverso `CreateMap<string, Email>().ConvertUsing(src => Email.Create(src))` — isso garante que, ao **criar** um Email a partir de string vinda de fora, a validação de `Email.Create()` roda mesmo dentro do AutoMapper.
- `CreateMap<Order, OrderResponse>()` usa `.ForMember` para converter o enum `Status` para string e mapear a coleção `Items`.
- `CreateMap<CreateOrderDto, Order>().ConstructUsing(src => new Order(src.CustomerId))` — como `Order` não tem construtor vazio público (só `protected Order() {}` para EF), o AutoMapper é instruído a **não** tentar instanciar via reflection, e sim chamar o construtor de domínio real. Isso preserva a validação do construtor mesmo vindo do AutoMapper.
- **Mas note**: no fluxo real de `OrderService.Create`, o mapeamento de DTO→Order **não é usado** — o código chama `new Order(...)` e `order.AddItem(...)` manualmente linha a linha (`OrderService.cs:67-73`), não usa `_mapper.Map<CreateOrderDto, Order>()`. Ou seja, os `CreateMap<CreateOrderDto, Order>` e `CreateMap<CreateOrderItemDto, OrderItem>` existem no profile mas parecem não ser usados no fluxo de criação de pedido — possível código morto (ou usado em outro lugar não lido nesta auditoria).

`ICustomMapper`/`CustomMapper` é a única forma como os Services tocam o AutoMapper — nenhum Service injeta `IMapper` diretamente.

---

## 11. Tratamento de Erros

```text
erro acontece (throw em qualquer camada)
      ↓
propaga sem catch intermediário através de Service → Controller
      ↓
ASP.NET Core intercepta via app.UseExceptionHandler()          Program.cs:74
      ↓
GlobalExceptionHandler.TryHandleAsync()                        Source/OrderManagement.API/Handlers/GlobalExceptionHandler.cs
      ↓
switch por tipo de exceção:
   ValidationException          → 400
   NotFoundException            → 404
   UnauthorizedException        → 401
   DomainValidationException    → 400
   qualquer outra               → 500
      ↓
monta um ProblemDetails (RFC 7807) e escreve como JSON na resposta
```

Exemplo real (fluxo de `OrderService.GetOrderById` quando o pedido não existe):

```text
throw new ValidationException($"Order with id: {id} was not found.")   ← Application/Services/OrderService.cs:162
      ↓ (não há try/catch no controller nem no service acima)
GlobalExceptionHandler mapeia ValidationException → 400 Bad Request
      ↓
resposta: { "status": 400, "title": "An error occured.", "detail": "Order with id: <guid> was not found." }
```

**Observação que vale estudar com atenção**: notar que "pedido não encontrado" retorna **400**, não **404**, porque `OrderService` lança `ValidationException` em vez de `NotFoundException` para esse caso (`NotFoundException` existe e é usada em `UserService`, mas `OrderService`/`ProductService`/`CustomerService` usam `ValidationException` até para "não encontrado"). Isso é uma inconsistência semântica real entre os Services — HTTP-wise, "não encontrado" deveria ser 404, e aqui só `UserController` recebe esse código corretamente (via `NotFoundException`) enquanto Order/Product/Customer não-encontrado retorna 400. Ver seção 19/20.

Também vale notar: o `GlobalExceptionHandler` é `internal sealed class` implementando `IExceptionHandler` — este é o mecanismo **novo** do .NET 8 (substituiu o antigo `app.UseExceptionHandler("/error")` com controller de erro dedicado, ou o `ExceptionFilterAttribute` do MVC clássico).

---

## 12. Testes

**Projeto único de testes**: `Source/OrderManagement.Tests` — **só testes unitários**. Não existe projeto de teste de integração (nenhum uso de `WebApplicationFactory`, nenhum Testcontainers, nenhum banco em memória para testes end-to-end).

Estrutura:
```text
OrderManagement.Tests/
├── Domain/
│   ├── Builders/       ← um Builder por entidade (test data builders, não Bogus direto)
│   ├── Entities/       ← testes puros de regra de negócio (ex.: OrderTest.cs, 47 testes)
│   └── ValueObjects/   ← EmailTest, PhoneTest
└── Application/
    ├── Builders/       ← Builders para DTOs
    ├── ExtensionMethods/  ← LoggerMockExtensions (helper para verificar chamadas de log com Moq)
    └── Services/       ← um teste por Service, mockando repositórios/cache/mapper
```

**1 teste unitário explicado linha a linha** — `OrderTest.AddItem_ShouldThrowException_WhenOrderIsPaid` (`Domain/Entities/OrderTest.cs:250`):

```csharp
var order = new OrderBuilder().BuildPaidOrder();       // Builder monta um Order já no estado "Paid"
Action act = () => order.AddItem(Guid.NewGuid(), 1, 10.00m);  // encapsula a chamada numa Action (não executa ainda)
act.Should().Throw<DomainValidationException>()         // FluentAssertions: executa a Action e espera exceção
    .WithMessage("*non-pending*");                       // valida que a mensagem contém "non-pending" (wildcard)
order.Status.Should().Be(OrderStatus.Paid);              // garante que o estado do objeto não mudou após a falha
```

Isso testa uma invariante pura de domínio, sem nenhum mock — é um teste 100% síncrono e isolado, típico de "testar a entidade como se fosse uma unidade matemática".

**1 teste de "integração de camada" explicado (na verdade é um teste unitário de Service com mocks — não é integração real com banco)** — `OrderServiceTest.CreateOrder_ShouldBeSuccessful` (`Application/Services/OrderServiceTest.cs:52`):

```csharp
var customer = new CustomerBuilder().Build();             // monta um Customer válido em memória
var product = new ProductBuilder().Build();
var itemDto = new CreateOrderItemDtoBuilder().WithProductId(productId).Build();
var orderDto = new CreateOrderDtoBuilder().WithCustomerId(customer.Id).WithItems(...).Build();

_customerRepositoryMock.Setup(r => r.GetCustomerById(customer.Id)).ReturnsAsync(customer);  // simula "customer existe"
_productRepositoryMock.Setup(r => r.GetProductById(productId)).ReturnsAsync(product);        // simula "produto existe"
_orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);  // simula persistência sem banco real
_mapperMock.Setup(...).Returns(...);                       // simula o mapeamento sem AutoMapper real configurado

var result = await _orderService.Create(orderDto);         // executa o método real do Service (não mockado)

result.Success.Should().BeTrue();
_orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);  // garante que Persist foi chamado exatamente 1x
_loggerMock.VerifyLog($"Order {result.Data.Id} created successfully.");       // helper customizado que verifica log via Moq
```

**O que isso realmente testa**: a lógica de orquestração dentro de `OrderService.Create` (buscar customer, buscar produtos, criar Order, persistir, invalidar cache, mapear resposta) — sem nunca tocar em MySQL de verdade, EF Core, ou HTTP. **Não é** um teste de integração de verdade (não sobe a API, não bate em banco) — é um teste unitário de uma classe de orquestração com dependências mockadas. O nome "integração" no seu pedido original não se aplica literalmente a este projeto: **não há testes de integração aqui**, apenas unitários de Domain e de Application.

**Bibliotecas de mock**: `Moq` para simular interfaces; `Bogus` está referenciado no `.csproj` mas não aparece usado diretamente nos testes lidos (os `Builders` custom parecem ter substituído o uso direto de Bogus, ou Bogus é usado em builders não lidos nesta auditoria).

**O que está bem testado**: `Order` (47+ casos, cobrindo toda a máquina de estados) e `OrderService` (17 testes, cobrindo create/update/delete/get com todos os caminhos de erro).

**O que NÃO está testado** (evidência: nenhum arquivo de teste para eles foi encontrado): `AuthController`/`AuthService` tem teste (`AuthServiceTest.cs` existe), mas **nenhum controller é testado diretamente** (nenhum teste de `OrderController`, `UserController`, etc.) e **nenhuma repository é testada** (nem com banco em memória do EF Core `UseInMemoryDatabase`, nem com Testcontainers).

---

## 13. Docker

```text
Dockerfile (multi-stage)
 ↓
Stage 1 "build-env": mcr.microsoft.com/dotnet/sdk:8.0
   COPY Shared/, Source/, .sln
   dotnet restore
   dotnet publish -c Release -o /app/out
 ↓
Stage 2 "runtime": mcr.microsoft.com/dotnet/aspnet:8.0 (imagem menor, sem SDK completo)
   COPY --from=build-env /app/out .
   EXPOSE 8080
   ENTRYPOINT ["dotnet", "OrderManagement.API.dll"]
```

Padrão multi-stage clássico: a imagem final não carrega o SDK completo (~800MB+), só o runtime ASP.NET (~200MB), reduzindo superfície de ataque e tamanho de imagem.

**Docker Compose:**

```text
api (build local via dockerfile)
 ↕ depends_on
dbcontext (mcr.microsoft.com/azure-sql-edge, ou seja SQL Server para ARM/Linux)
```

- Porta mapeada: `7212:8080` (host:container).
- Variáveis injetadas: `ConnectionStrings__DefaultConnection`, `Settings__Jwt__SigningKey`, `Settings__Jwt__ExpirationTimeMinutes`, `ASPNETCORE_ENVIRONMENT=Development` — todas vindas de `${VAR}` do `.env` (não commitado, só `.env.example`).
- Volume nomeado `ordermanagement-api-azure-edge` persiste os dados do SQL Server entre reinícios.
- Rede bridge dedicada `ordermanagement-networks`, com alias `sql` para o banco (é por isso que a connection string usa `Server=sql,1433`).

**⚠️ Repetindo o achado da seção 7**: como configurado, este `docker-compose.yml` sobe SQL Server, mas `Program.cs` está hardcoded para MySQL (`UseMySql`). **Rodar `docker-compose up` hoje provavelmente falha ao conectar no banco.** Não fiz nenhuma alteração — só reporto o que o código mostra.

**Comandos para rodar localmente** (informativo, não executado nesta auditoria):

```bash
# Sem Docker (requer MySQL local rodando e configurado em appsettings/env vars):
dotnet restore
dotnet run --project Source/OrderManagement.API

# Com Docker Compose (hoje inconsistente — ver achado acima; corrigir DB antes de tentar):
cp .env.example .env   # preencher com valores reais
docker-compose up --build
```

---

## 14. CI/CD

**`ci.yml` — dispara em**: push para `main`/`develop`, PR para `main`, e manualmente (`workflow_dispatch`).

```text
git push (main/develop) ou PR para main
   ↓
GitHub Actions runner (ubuntu-latest)
   ↓
Checkout (fetch-depth: 0 — histórico completo, necessário para o SonarCloud analisar blame/novidade de código)
   ↓
Setup .NET 8.0.x + Setup JDK 17 (zulu — o scanner do SonarCloud roda em JVM)
   ↓
Cache de pacotes/scanner do SonarCloud (acelera runs subsequentes)
   ↓
dotnet tool install --global dotnet-sonarscanner
   ↓
dotnet restore
   ↓
dotnet sonarscanner begin (usa secrets: SONAR_PROJECT_KEY, SONAR_ORGANIZATION, SONAR_TOKEN, GITHUB_TOKEN)
   aponta sonar.cs.opencover.reportsPaths para **/TestResults/**/coverage.opencover.xml
   ↓
dotnet build --configuration Release --no-restore
   ↓
dotnet test --configuration Release --no-build
   com /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/
   ↓
dotnet sonarscanner end (envia os resultados pro SonarCloud)
```

Secrets necessários: `SONAR_PROJECT_KEY`, `SONAR_ORGANIZATION`, `SONAR_TOKEN`, `GITHUB_TOKEN` (este último é automático do GitHub Actions).

**`cd.yml` — dispara em**: `workflow_run` do CI Pipeline concluído com sucesso, só na branch `main` (deploy é condicionado ao CI ter passado, não a um push direto — padrão correto de "só publica o que já foi validado").

```text
CI Pipeline (main) termina com sucesso
   ↓
cd.yml dispara automaticamente
   ↓
Checkout
   ↓
Substitui appsettings.json inteiro pelo conteúdo do secret APPSETTINGS_PRODUCTION
   (echo '${{ secrets.APPSETTINGS_PRODUCTION }}' > ./Source/OrderManagement.API/appsettings.json)
   ↓
Azure Login (secret AZURE_CREDENTIALS — service principal)
   ↓
az acr login --name apioms
   ↓
docker/build-push-action: builda a imagem (usa o mesmo Dockerfile) e push para apioms.azurecr.io/ordermanagement:latest
   ↓
azure/webapps-deploy: publica a imagem no Azure App Service (secret AZURE_WEBAPP_NAME)
```

Secrets adicionais necessários: `APPSETTINGS_PRODUCTION` (JSON completo, incluindo presumivelmente connection string e JWT signing key de produção — **nunca commitado**, correto), `AZURE_CREDENTIALS`, `AZURE_WEBAPP_NAME`.

**Nenhum secret foi exposto nesta auditoria** — os valores reais desses secrets não existem no repositório, só os nomes das variáveis usadas pelo workflow, que é informação pública e segura de reportar.

---

## 15. Deploy

```text
Código (branch main)
 ↓
CI Pipeline valida (build + test + Sonar)
 ↓
CD Pipeline builda imagem Docker com appsettings de produção injetado via secret
 ↓
Push para Azure Container Registry (apioms.azurecr.io)
 ↓
Deploy da imagem para Azure App Service (nome do app vem do secret AZURE_WEBAPP_NAME)
 ↓
Aplicação em produção roda com o Program.cs de sempre: ao subir, chama context.Database.Migrate() e DatabaseSeeder.Seed()
   → ou seja, migrations rodam automaticamente em produção a cada deploy/restart, sem passo manual
```

Isso depende inteiramente de infraestrutura Azure já existente e configurada fora do repositório (Container Registry `apioms`, um App Service, um banco de dados de produção acessível a partir do App Service) — nada disso é visível ou auditável a partir do código-fonte.

---

## 16. Configuração / Environments

O que existe versionado:
- `Source/OrderManagement.API/appsettings.Development.json` — só `Logging` (nível `Information`/`Warning`). **Não contém** `ConnectionStrings` nem `Settings:Jwt` — ou seja, essas chaves precisam vir de outro lugar em desenvolvimento: variáveis de ambiente, User Secrets do .NET (não versionados por padrão), ou um `appsettings.json` base que **não está no repositório** (nem sequer aparece no `.gitignore` de forma explícita — mas note que `appsettings.json` de produção é **gerado dinamicamente pelo `cd.yml`**, o que sugere que o `appsettings.json` base local também não é versionado por design, para não vazar segredos).
- `.env.example` — placeholder para Docker Compose: `SA_PASSWORD`, `DB_CONNECTION_STRING`, `JWT_SIGNING_KEY`, `JWT_EXPIRATION_MINUTES`. `.env` real está no `.gitignore` (linha 7).
- `Properties/launchSettings.json` — perfis `http` (porta 5187) e `https` (porta 7212/5187) para `dotnet run`.

**Para rodar localmente hoje, você precisaria**, no mínimo, configurar (via `dotnet user-secrets` ou variáveis de ambiente, já que não há `appsettings.json` base commitado):
```
ConnectionStrings:DefaultConnection   → string de conexão MySQL válida (não SQL Server, apesar do que .env.example sugere)
Settings:Jwt:SigningKey               → qualquer string secreta
Settings:Jwt:ExpirationTimeMinutes    → inteiro, ex. 60
```

**CORS**: não há nenhuma menção a `AddCors`/`UseCors` em `Program.cs` nem em nenhum arquivo de configuração lido. **CORS não está configurado.** Isso é crítico para a próxima etapa (Angular) — ver seção 21.

**Swagger**: habilitado sempre (não há `if (app.Environment.IsDevelopment())` envolvendo `UseSwagger()`), disponível em `/swagger` inclusive quando a API roda "Production" — normalmente Swagger é desabilitado em produção; aqui ele fica exposto sempre, o que é uma escolha (comum em projetos de portfólio para facilitar demonstração, mas seria revisado num ambiente corporativo real).

---

## 17. Funcionalidades Existentes

| Área | Funcionalidade | Endpoint | Auth | Testes | Status |
|---|---|---|---|---|---|
| Auth | Login | `POST /api/auth/login` | Não (`[AllowAnonymous]` implícito, sem `[Authorize]` na classe) | Sim (`AuthServiceTest.cs`) | Implementado |
| Auth | Registro público de usuário | — | — | — | **Não existe** (README descreve, código não tem) |
| User | Criar usuário | `POST /api/users` | Sim, role Admin | Não (nenhum `UserServiceTest`/`UserControllerTest` encontrado) | Implementado |
| User | Atualizar usuário | `PUT /api/users/{id}` | Sim, role Admin | Não | Implementado |
| User | Buscar por id | `GET /api/users/{id}` | Sim, role Admin | Não | Implementado |
| User | Listar usuários | `GET /api/users` | Sim, role Admin | Não | Implementado |
| User | Deletar usuário | `DELETE /api/users/{id}` | Sim, role Admin | Não | Implementado |
| Customer | Criar cliente | `POST /api/customer` | Sim, role Admin | Não | Implementado (com bug de `CreatedAtAction`, ver seção 19) |
| Customer | Atualizar cliente | `PUT /api/customer/{id}` | Sim, role Admin | Não | Implementado |
| Customer | Buscar por id | `GET /api/customer/{id}` | Não (`[AllowAnonymous]`) | Não | Implementado |
| Customer | Listar clientes | `GET /api/customer` | Não (`[AllowAnonymous]`) | Não | Implementado |
| Customer | Deletar cliente | `DELETE /api/customer/{id}` | Sim, role Admin | Não | Implementado |
| Product | CRUD completo | `/api/product` (5 verbos) | Create/Update/Delete = Admin; Get/List = anônimo | Não | Implementado |
| Order | Criar pedido | `POST /api/order` | Sim, role Admin | Sim | Implementado |
| Order | Atualizar pedido (itens + status) | `PUT /api/order/{id}` | Sim, role Admin | Sim | Implementado |
| Order | Buscar por id | `GET /api/order/{id}` | Não (`[AllowAnonymous]`) | Sim | Implementado |
| Order | Listar pedidos | `GET /api/order` | Não (`[AllowAnonymous]`) | Sim | Implementado |
| Order | Deletar pedido | `DELETE /api/order/{id}` | Sim, role Admin | Sim | Implementado (retorna `Ok`, não `NoContent` — inconsistente com os outros deletes, ver seção 19) |
| Order | Máquina de estados (Pending→Paid→Shipped/Cancelled) | via `PUT` com `Status` no body | Sim, role Admin | Sim (extensa) | Implementado |

*(Nota: os nomes de rota exatos usam `[Route("api/[controller]")]`, então o segmento é o nome do controller sem sufixo `Controller`, ex.: `OrderController` → `/api/Order`; roteamento em ASP.NET Core por padrão é case-insensitive, então `/api/order` funciona igual.)*

---

## 18. Patterns e Conceitos Utilizados

### Dependency Injection
**O que é**: prover as dependências de uma classe de fora, em vez dela criá-las.
**Onde usei**: em todo o projeto — `ApplicationDependencyInjection.cs`, `InfrastructureDependencyInjection.cs`, `Program.cs`.
**Por que usei**: desacopla Services de implementações concretas, viabiliza testes com mocks.
**Preciso revisar?** 🔴 FUNDAMENTAL

### Repository Pattern
**O que é**: abstrair acesso a dados atrás de uma interface.
**Onde usei**: `IOrderRepository`/`OrderRepository` e equivalentes.
**Por que usei**: isolar EF Core do resto da aplicação.
**Preciso revisar?** 🔴 FUNDAMENTAL

### Clean Architecture / Dependency Inversion
**O que é**: camadas internas (Domain) não dependem de camadas externas (Infrastructure); é o inverso — interfaces vivem no Domain, implementações na Infrastructure.
**Onde usei**: `IPasswordHasher`/`IAccessTokenGenerator` definidos em `Domain/Security`, implementados em `Infrastructure/Security`.
**Por que usei**: manter o núcleo de negócio livre de detalhes técnicos.
**Preciso revisar?** 🔴 FUNDAMENTAL

### Rich Domain Model (entidades com comportamento)
**O que é**: em vez de entidades "burras" (só getters/setters públicos), as entidades protegem seu próprio estado com métodos que validam invariantes.
**Onde usei**: `Order.AddItem/MarkAsPaid/MarkAsShipped`, setters `private`.
**Por que usei**: impedir que o sistema chegue a um estado inválido (ex.: pedido enviado sem estar pago).
**Preciso revisar?** 🔴 FUNDAMENTAL — é a parte mais sofisticada e valiosa do seu código.

### Value Objects
**O que é**: tipo que representa um valor (não uma identidade), imutável, autoválido.
**Onde usei**: `Email.Create()`, `Phone.Create()`.
**Por que usei**: evitar strings soltas e inválidas circulando pelo sistema.
**Preciso revisar?** 🟡 IMPORTANTE

### JWT Authentication + Role-based Authorization
**O que é**: token assinado que carrega identidade/claims, validado a cada request sem consultar banco.
**Onde usei**: `AuthenticationConfiguration.cs`, `[Authorize(Roles = "Admin")]`.
**Por que usei**: autenticação stateless para API REST.
**Preciso revisar?** 🔴 FUNDAMENTAL

### DTO (Data Transfer Object) + AutoMapper
**O que é**: objetos específicos para entrada/saída da API, diferentes das entidades de domínio.
**Onde usei**: `Shared/OrderManagement.Communication`, `MappingProfile.cs`.
**Por que usei**: não expor a entidade de domínio diretamente na API (desacoplamento de contrato).
**Preciso revisar?** 🟡 IMPORTANTE

### FluentValidation
**O que é**: biblioteca de validação declarativa/fluente para DTOs.
**Onde usei**: `Validators/*.cs`.
**Por que usei**: validar forma de entrada antes de chegar na lógica de negócio.
**Preciso revisar?** 🟡 IMPORTANTE

### async/await + LINQ
**O que é**: programação assíncrona não-bloqueante; consultas declarativas sobre coleções.
**Onde usei**: todos os métodos de repositório e service (`async Task<...>`), `.Where`, `.Sum`, `.FirstOrDefault` no Domain e Repositories.
**Por que usei**: I/O de banco não deve bloquear threads da API.
**Preciso revisar?** 🔴 FUNDAMENTAL

### Middleware / IExceptionHandler (.NET 8)
**O que é**: componente que intercepta o pipeline HTTP; `IExceptionHandler` é o mecanismo moderno de tratamento global de exceção.
**Onde usei**: `GlobalExceptionHandler.cs` + `app.UseExceptionHandler()`.
**Por que usei**: centralizar conversão de exceções em respostas HTTP padronizadas (ProblemDetails).
**Preciso revisar?** 🟡 IMPORTANTE

### Options/Configuration Pattern (parcial)
**O que é**: ler configuração tipada de `appsettings`/env vars via `IConfiguration`.
**Onde usei**: `configuration.GetValue<int>("Settings:Jwt:ExpirationTimeMinutes")`.
**Por que usei**: parametrizar segredos e ajustes por ambiente sem hardcode.
**Preciso revisar?** 🟢 COMPLEMENTAR (não usa o padrão `IOptions<T>` completo do ASP.NET Core, usa leitura direta — funciona, mas é uma versão simplificada do pattern "de livro").

### Caching (IMemoryCache)
**O que é**: guardar resultados de leitura em memória do processo para evitar reconsultar o banco.
**Onde usei**: `CacheService.cs`, usado em `GetAll`/`GetById` de Order/User, invalidado em `Create`/`Update`/`Delete`.
**Por que usei**: reduzir carga no banco em leituras repetidas.
**Preciso revisar?** 🟢 COMPLEMENTAR — funciona, mas é cache local ao processo (não sobrevive a múltiplas instâncias/réplicas; não seria suficiente num cenário com mais de uma instância da API rodando).

### Testing (xUnit + Moq + FluentAssertions + test data builders)
**O que é**: testes automatizados isolando a unidade sob teste com dublês (mocks) das dependências.
**Onde usei**: `OrderManagement.Tests/`.
**Por que usei**: validar regras de domínio e orquestração de services sem infraestrutura real.
**Preciso revisar?** 🔴 FUNDAMENTAL

### CI/CD (GitHub Actions) + Docker + SonarCloud
**O que é**: automação de build/test/análise/deploy a cada push, empacotamento reproduzível em container.
**Onde usei**: `.github/workflows/`, `dockerfile`.
**Por que usei**: portfólio demonstrando prática profissional de entrega contínua.
**Preciso revisar?** 🟡 IMPORTANTE (os arquivos existem e o fluxo é coerente; o ponto fraco não é o pipeline em si, é a inconsistência de banco que provavelmente quebra o ambiente local via Compose — ver seção 20).

---

## 19. Qualidade Atual (Diagnóstico, sem correção)

| Área | Nota | Observação |
|---|---|---|
| Arquitetura geral | 🟢 Bom | Separação de camadas coerente, Dependency Inversion correta, Domain isolado. |
| Modelagem de Domínio | 🟢 Bom | `Order` como agregado com máquina de estados é o ponto mais forte do projeto. |
| API / Controllers | 🟡 Melhorável | Padrão inconsistente: `DeleteOrder` retorna `Ok` em vez de `NoContent`; `CreateCustomer` aponta `CreatedAtAction` para si mesmo em vez de `GetCustomerById` (bug — o header `Location` gerado provavelmente não vai apontar para uma rota GET válida com esse nome de action). |
| Padrão de erro (`Result<T>` vs exceptions) | 🟡 Melhorável | `Result<T>.Failure` existe mas não parece ser usado nos Services lidos — todo erro de negócio usa `throw`. Os `if (!result.Success)` nos controllers são, na prática, inalcançáveis para os casos hoje cobertos, porque a exceção já teria sido capturada antes. Isso não quebra nada, mas é uma camada de abstração que não está sendo aproveitada — ou os dois padrões coexistem por acidente histórico. |
| Semântica de erro HTTP | 🟡 Melhorável | "Não encontrado" retorna 400 (`ValidationException`) em `Order`/`Product`/`Customer`, mas 404 (`NotFoundException`) em `User`. Inconsistente entre services. |
| Segurança | 🟡 Melhorável | JWT correto e BCrypt correto. Mas: sem CORS configurado; sem endpoint de registro público (ok para portfólio, mas README promete o contrário); Swagger exposto sempre, mesmo fora de Development; `ValidateIssuer`/`ValidateAudience` desligados (aceitável para projeto único, não para múltiplos consumidores). |
| Persistência / Banco | 🔴 Problema | Inconsistência real entre `Program.cs` (MySQL) e `docker-compose.yml`/`.env.example` (SQL Server). Isso provavelmente quebra `docker-compose up` hoje. |
| Testes | 🟡 Melhorável | Cobertura forte em Domain e em Application Services testados (Order, Auth), mas nenhum teste para `User`/`Customer`/`Product` Services, nenhum teste de Controller, nenhum teste de Repository/EF Core, e **zero testes de integração**. |
| Docker | 🟡 Melhorável | Dockerfile multi-stage está correto e enxuto; o problema é só o descompasso de banco no Compose (mesmo achado da Persistência). |
| CI/CD | 🟢 Bom | Fluxo CI→CD coerente, gate corrigido (CD só roda se CI passar), uso correto de secrets, SonarCloud integrado com cobertura real via Coverlet. |
| Configuração | 🟡 Melhorável | Falta um `appsettings.json` base documentado/exemplo para rodar sem Docker sem adivinhar as chaves; isso está implícito só no `.env.example` (que também está desatualizado, ver Persistência). |
| Logging | 🟢 Bom | Serilog bem configurado, com contexto estruturado (`_logger.LogInformation("... {OrderId}", id)`), sink de arquivo e console. |
| Tratamento de erros | 🟢 Bom | `IExceptionHandler` centralizado, `ProblemDetails` padronizado — mecanismo moderno e correto, apesar da inconsistência semântica citada acima. |
| Documentação (README) | 🔴 Problema | Descreve `/register` e fluxo de "Register user" que não existem no código atual — está desatualizado em relação ao código real. Isso é exatamente o tipo de coisa que gera a confusão que você descreveu ao pedir esta auditoria. |

---

## 20. Dívida Técnica Antes do Frontend

### BLOQUEIA FRONTEND

1. **CORS não configurado.** Sem `AddCors`/`UseCors`, um Angular rodando em `localhost:4200` (ou qualquer origem diferente da API) terá todas as chamadas bloqueadas pelo navegador. Isso precisa ser resolvido antes de escrever a primeira chamada HTTP do Angular.
2. **Inconsistência MySQL vs SQL Server** (`Program.cs` vs `docker-compose.yml`/`.env.example`). Sem um banco funcionando de forma confiável e reproduzível, não dá para desenvolver o frontend contra uma API que às vezes sobe e às vezes não.

### DEVERIA CORRIGIR (importante, não bloqueia o início do Angular)

3. `CreateCustomer` — `CreatedAtAction(nameof(CreateCustomer), ...)` deveria apontar para `nameof(GetCustomerById)`, do jeito que `OrderController`/`UserController`/`ProductController` já fazem corretamente. Isso afeta o header `Location` da resposta 201, que um cliente Angular bem-feito poderia usar para navegar após criar um cliente.
4. `DeleteOrder` retorna `Ok(result.Data)` (200) enquanto os outros `Delete*` retornam `NoContent()` (204). Padronizar facilita o cliente Angular tratar todos os deletes da mesma forma.
5. Semântica 400 vs 404 inconsistente entre Services ("não encontrado" usando `ValidationException` em vez de `NotFoundException` em Order/Product/Customer). Um interceptor HTTP no Angular que trata 404 de forma diferente de 400 vai se comportar de forma inconsistente dependendo do recurso.
6. Falta endpoint de registro público — decidir se o Angular vai ter tela de "criar conta" ou só login com o usuário seed (`admin@admin.com`/`123456`) e criação de usuários feita por um Admin já logado.
7. README desatualizado — atualizar depois, não é urgente para o Angular funcionar, mas é fonte de confusão futura.

### PODE ESPERAR

8. Ausência de testes de integração — melhoria de qualidade, não bloqueia consumo pela API.
9. Ausência de testes para `User`/`Customer`/`Product` Services — mesma categoria.
10. `Result<T>.Failure` não utilizado / ambiguidade de padrão de erro — refactor de estilo interno, invisível para o consumidor da API.
11. Cache em memória não distribuído — só relevante se/quando houver múltiplas instâncias da API.
12. `ValidateIssuer`/`ValidateAudience` desligados no JWT — relevante só se a API for consumida por múltiplos clientes/domínios distintos no futuro.
13. Falta de Unit of Work explícito — não é um problema hoje, só se cenários multi-agregado surgirem.

---

## 21. O Backend Está Pronto para Angular?

**Classificação: READY WITH SMALL FIXES**

Motivos:
- Contratos (DTOs/Responses) são claros, estáveis e já pensados como uma camada separada (`Shared/OrderManagement.Communication`) — ótimo ponto de partida para gerar `interfaces` TypeScript.
- Autenticação JWT + roles já funciona de ponta a ponta e é o padrão mais comum de se integrar com Angular (interceptor de `Authorization: Bearer`).
- Swagger está disponível e correto (com suporte a Bearer), então dá para explorar/testar a API manualmente antes de escrever o serviço Angular.
- **Mas**: sem CORS configurado, nenhuma chamada do Angular vai funcionar sem antes resolver isso — é o único bloqueador realmente técnico, e é pequeno de corrigir (uma policy no `Program.cs`).
- A instabilidade do banco via Docker Compose (item 2 da seção 20) não impede o Angular de ser desenvolvido, mas impede um ambiente "docker-compose up e pronto" — hoje, rodar a API localmente via `dotnet run` contra um MySQL real (não via Compose) é o caminho mais confiável enquanto isso não for resolvido.
- Não há paginação em nenhum `GetAll` (`Order`, `User`, `Customer`, `Product` retornam listas completas) — para o escopo de portfólio isso é aceitável, mas o Angular não deve assumir que os endpoints de listagem serão paginados.

---

## 22. Plano do Frontend Angular (conceitual, baseado nos endpoints reais)

```text
Angular
│
├── Core
│   ├── api/            ← services HttpClient por recurso, espelhando os controllers reais:
│   │                       AuthService, OrderService, CustomerService, ProductService, UserService
│   ├── auth/            ← guarda o JWT (localStorage), expõe estado de autenticação/role atual
│   ├── guards/          ← AuthGuard (rota exige login) + RoleGuard (rota exige role Admin,
│   │                       espelhando exatamente os [Authorize(Roles="Admin")] do backend)
│   ├── interceptors/    ← anexa "Authorization: Bearer {token}" e trata 401/403 globalmente
│   └── models/          ← interfaces TS geradas a partir dos DTOs/Responses reais do
│                            Shared/OrderManagement.Communication (CreateOrderDto, OrderResponse, etc.)
│
├── Auth
│   └── Login            ← consome POST /api/auth/login
│
├── Layout
│   ├── Sidebar / Header ← navegação entre Orders / Customers / Products / Users (só visível
│   │                       para Admin, já que a maior parte das operações exige essa role)
│
├── Orders
│   ├── List             ← GET /api/order (anônimo, mas normalmente atrás do guard mesmo assim)
│   ├── Details           ← GET /api/order/{id}, mostra Items e Status
│   ├── Create             ← POST /api/order (Admin), formulário reativo com FormArray para Items
│   └── StatusActions      ← PUT /api/order/{id} para MarkAsPaid/Shipped/Cancelled — mapeia
│                             1:1 com a máquina de estados que você já construiu no Domain
│
├── Customers
│   └── List / Create / Edit    ← CRUD simples espelhando CustomerController
│
├── Products
│   └── List / Create / Edit    ← CRUD simples espelhando ProductController
│
└── Users (só Admin)
    └── List / Create / Edit    ← CRUD simples espelhando UserController
```

**Conceitos Angular que você vai praticar, mapeados ao que a API já exige:**
- **Reactive Forms + validation**: o formulário de criar Order precisa de um `FormArray` de items (replica a validação de `CreateOrderDtoValidator` e as regras de domínio de `Order.AddItem`).
- **HttpClient + services por recurso**: 1 service Angular por controller do backend.
- **Interceptors**: anexar token, tratar erros padronizados (o `ProblemDetails` do `GlobalExceptionHandler` já dá um formato consistente de erro pra tratar globalmente).
- **Guards**: `AuthGuard` (rota exige token) e `RoleGuard` (rota exige `Admin`) — mapeiam 1:1 pros `[Authorize]`/`[Authorize(Roles="Admin")]` do backend.
- **Routing**: rotas por recurso + rotas filhas (list/detail/create/edit).
- **Error handling / loading state**: já que a API não pagina, listas grandes precisam de algum indicador de loading enquanto a resposta completa chega.
- **Environment/config**: `environment.ts` com a URL base da API (variando entre `http://localhost:5187` em dev e a URL do Azure App Service em produção).
- **Models/interfaces**: tipagem espelhando 1:1 os DTOs e Responses do `Shared/OrderManagement.Communication`.
- Signals/RxJS: dá para usar tanto `signal()` quanto `Observable` do HttpClient — não é algo que a API impõe, é decisão sua no Angular.

---

## 23. Mapa de Reaprendizado

```text
🔴 REVISAR PRIMEIRO

C#
├── async/await                      (usado em toda Application/Infrastructure)
├── interfaces + DI                  (base de toda a arquitetura)
├── LINQ                             (Sum, FirstOrDefault, Where usados no Domain/Repositories)
└── encapsulamento (private set)     (base do Rich Domain Model do seu Order)

ASP.NET Core
├── minimal hosting (Program.cs)     (sem Startup.cs — modelo atual)
├── middleware pipeline               (UseAuthentication/UseAuthorization/UseExceptionHandler, ordem importa)
├── model binding + FluentValidation  ([FromBody]/[FromRoute] + AddFluentValidationAutoValidation)
└── JWT (JwtBearer + [Authorize])     (AuthenticationConfiguration.cs + AccessTokenGenerator.cs)

EF Core
├── DbContext + Fluent API (OnModelCreating)
├── Owned Types (Email/Phone como Owned, não tabela separada)
├── relacionamentos + DeleteBehavior (Cascade vs Restrict)
└── migrations (Database.Migrate() automático no boot)

TESTES
├── xUnit + FluentAssertions          (Should().Throw, Should().Be)
├── Moq (Setup/Returns/Verify)
└── test data builders (seus próprios *Builder.cs, não Bogus direto)

ANGULAR (a vir)
├── components / routing
├── services + HttpClient
├── reactive forms + FormArray
├── guards (Auth + Role)
└── interceptors (Bearer token, tratamento de erro)

DEVOPS
├── Docker multi-stage
├── GitHub Actions (jobs, secrets, workflow_run)
└── SonarCloud (coverage via Coverlet)
```

---

## 24. Ordem Recomendada de Estudo

```text
FASE 1 — ENTENDER (concluída com este documento)
   Arquitetura + fluxo de Create Order + Auth.
   Critério de avanço: você consegue explicar de cabeça, sem olhar o código,
   o caminho de POST /api/orders do controller até o banco.

FASE 2 — BACKEND (C# / ASP.NET / EF)
   Reler Order.cs, OrderService.cs, OrderRepository.cs, OrderManagementDbContext.cs
   com atenção total — é o quarteto mais importante do projeto.
   Critério de avanço: você consegue explicar por que AddItem() valida Status == Pending
   e por que isso está no Domain e não no Service.

FASE 3 — SEGURANÇA (JWT)
   AuthenticationConfiguration.cs + AccessTokenGenerator.cs + AuthService.cs.
   Critério de avanço: você sabe descrever o que está dentro de um JWT gerado por este
   projeto e como o middleware o valida a cada request.

FASE 4 — QUALIDADE (Testes)
   Ler OrderTest.cs e OrderServiceTest.cs por completo.
   Critério de avanço: você consegue escrever um teste novo para um cenário de Order
   que ainda não está coberto, sem copiar um existente.

FASE 5 — CORRIGIR O BLOQUEADOR (CORS + banco)
   Só depois de entender bem o backend, resolver os dois itens que BLOQUEIAM o Angular
   (seção 20). Pequeno e cirúrgico — não é uma fase de "melhorar tudo".

FASE 6 — FRONTEND (Angular)
   Seguir o plano da seção 22, começando por Auth (Login) + Orders (List/Details),
   que são as telas que mais exercitam os conceitos centrais do curso.

FASE 7 — DEVOPS (Docker + CI/CD)
   Reler dockerfile, docker-compose.yml, ci.yml, cd.yml já sabendo o que cada
   variável/secret alimenta no backend.

FASE 8 — OMS MINI (reconstrução manual, guiada por mim etapa a etapa quando você pedir)
```

---

## 25. Glossário do Seu Próprio Projeto

- **`Order`** — agregado raiz que representa um pedido; único lugar onde `TotalAmount` pode mudar (via `RecalculateTotal`, privado).
- **`OrderItem`** — entidade filha de `Order`, não existe fora de um pedido (sem repositório próprio).
- **`OrderStatus`** — enum `Pending/Paid/Shipped/Cancelled`; as transições válidas estão codificadas nos métodos `MarkAs*` de `Order`, não numa tabela de transições externa.
- **`DomainValidationException`** — lançada de dentro do Domain quando uma invariante de entidade é violada (ex.: pedido não-pendente sendo editado).
- **`ValidationException`/`NotFoundException`/`UnauthorizedException`** — exceções da camada Application, mapeadas para códigos HTTP pelo `GlobalExceptionHandler`.
- **`Result<T>`** — wrapper de retorno dos Services com `Success`/`Data`/`ErrorMessage`; hoje conivente mas parcialmente não utilizado (ver seção 19).
- **`ICustomMapper`** — fachada da Application sobre o `IMapper` do AutoMapper.
- **`ICacheService`** — abstração sobre `IMemoryCache`, com chaves centralizadas em `CacheKeys`.
- **Owned Type (EF Core)** — forma como `Email`/`Phone` (Value Objects) são persistidos como coluna da própria tabela do dono, não como tabela separada.
- **`IExceptionHandler`** — interface do .NET 8 para tratamento global de exceção (substituto moderno do middleware de exceção clássico).

---

## Resumo para Guilherme

### O que descobri
Uma API .NET 8 em Clean Architecture genuína, com o ponto mais forte sendo o `Order` como agregado de domínio com máquina de estados real (`Pending → Paid → Shipped/Cancelled`). JWT + BCrypt corretos, EF Core bem modelado com Owned Types, testes unitários sólidos para Domain e para os Services de Order/Auth, pipeline CI/CD real (GitHub Actions + SonarCloud + Azure).

### Stack real
.NET 8, ASP.NET Core 8, EF Core (MySQL via Pomelo — apesar do Docker Compose apontar para SQL Server), AutoMapper, FluentValidation, JWT + BCrypt, Serilog, IMemoryCache, xUnit/Moq/FluentAssertions, Docker multi-stage, GitHub Actions, SonarCloud, Azure Container Registry + App Service.

### Arquitetura real
`API → Application → Domain`, com `Infrastructure` implementando interfaces definidas no `Domain` (Dependency Inversion correta). 5 recursos: Auth, User, Customer, Product, Order.

### Principais funcionalidades
CRUD de User/Customer/Product, criação e ciclo de vida completo de Order (com validações de negócio robustas), login JWT com roles Admin/Operator (Operator não usado em nenhuma regra hoje).

### Conceitos que você mais precisa relembrar
Rich Domain Model / invariantes de entidade (seu `Order.cs` é o melhor material de estudo que você tem), Dependency Inversion (`Domain` define interface, `Infrastructure` implementa), o padrão de DI com factory lambda (`AccessTokenGenerator`), e a diferença entre validação de entrada (FluentValidation) e regra de negócio (Domain).

### Problemas encontrados (sem corrigir)
1. **CORS ausente** — bloqueia Angular até ser configurado.
2. **MySQL (código) vs SQL Server (Docker Compose/.env.example)** — inconsistência real, provavelmente quebra `docker-compose up` hoje.
3. README descreve um endpoint `/register` que não existe no código atual.
4. Inconsistências pontuais: `CreateCustomer` aponta `CreatedAtAction` pra si mesmo (bug); `DeleteOrder` retorna `Ok` em vez de `NoContent`; "não encontrado" ora é 400 (Order/Product/Customer) ora é 404 (User).
5. `Result<T>.Failure` parece não utilizado — todo erro de negócio usa `throw`.
6. Cobertura de teste desigual: Order/Auth bem testados; User/Customer/Product Services sem teste; nenhum teste de Controller ou Repository; zero testes de integração.

### Pronto para Angular?
**READY WITH SMALL FIXES.** Precisa resolver CORS (pequeno) e estabilizar o banco local (também pequeno, mas real) antes da primeira chamada HTTP do Angular. O resto (contratos, auth, Swagger) já está pronto.

### Próximos 5 passos sugeridos
1. Decidir e corrigir a inconsistência de banco (MySQL definitivo, ajustar `docker-compose.yml`/`.env.example` para condizer, ou vice-versa).
2. Adicionar `AddCors`/`UseCors` no `Program.cs` liberando a origem do Angular em desenvolvimento.
3. Revisar os pequenos bugs de padrão HTTP (seção 20, itens 3-5) antes de gerar os `models` TypeScript, para não replicar inconsistências no frontend.
4. Criar o projeto Angular seguindo o plano da seção 22, começando por Auth + Orders.
5. Quando quiser, começamos o **OMS Mini** guiado etapa a etapa, conforme você descreveu na seção 24 do seu pedido — eu não crio nada sozinho lá, só oriento.

**Aguardando sua autorização para prosseguir com qualquer uma dessas etapas.**
