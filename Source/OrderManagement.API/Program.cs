using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.API.Configurations.Authentication;
using OrderManagement.API.Configurations.Swagger;
using OrderManagement.API.Handlers;
using OrderManagement.Application;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Infrastructure;
using OrderManagement.Infrastructure.Data;
using OrderManagement.Infrastructure.Data.Seed;
using Serilog;


// WebApplication.CreateBuilder monta o "host" da aplicação: lê appsettings.json,
// appsettings.{Environment}.json, variáveis de ambiente e argumentos de linha de
// comando (nessa ordem de prioridade crescente) e já prepara builder.Services
// (o container de Dependency Injection) e builder.Configuration (acesso a config).
var builder = WebApplication.CreateBuilder(args);

// Cache em memória do processo (usado pelo ICacheService/CacheService da Application).
builder.Services.AddMemoryCache();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configure Serilog
// Serilog é configurado de forma "estática" aqui (Log.Logger) porque precisa
// existir mesmo antes do container de DI estar pronto (ex.: para logar erros
// de inicialização). WriteTo.Console + WriteTo.File gravam nos dois destinos
// ao mesmo tempo; RollingInterval.Day cria um arquivo de log novo por dia.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ordermanagementapi-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add Custom Mapper service
builder.Services.AddScoped<ICustomMapper, CustomMapper>();

builder.Services.AddControllers();

// Add Validation DTO services
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

//Add Exception Handling Middleware
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---------------------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing)
// ---------------------------------------------------------------------------
// Por padrão, o navegador BLOQUEIA qualquer chamada JavaScript feita de uma
// origem (protocolo+host+porta) diferente da origem da API, a menos que a API
// devolva os headers de CORS autorizando explicitamente aquela origem. Sem
// isso, um Angular rodando em http://localhost:4200 nunca conseguiria falar
// com esta API em http://localhost:5187 — a chamada seria bloqueada pelo
// próprio navegador antes mesmo de chegar ao controller.
//
// A lista de origens permitidas vem de configuração (appsettings/env vars),
// não fica hardcoded, para que produção (domínio do Angular publicado) e
// desenvolvimento (localhost:4200) possam usar valores diferentes sem
// recompilar o código.
const string AngularCorsPolicy = "AngularClient";

// Cors:AllowedOrigins é um array no appsettings, ex.:
// "Cors": { "AllowedOrigins": [ "http://localhost:4200" ] }
// Se a chave não existir, usamos http://localhost:4200 como padrão sensato
// para desenvolvimento local do Angular (ng serve usa essa porta por padrão).
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy
            // WithOrigins: só essas origens específicas podem chamar a API
            // (nunca use AllowAnyOrigin() junto com AllowCredentials() — o
            // próprio navegador rejeita essa combinação por segurança).
            .WithOrigins(allowedOrigins)
            // Permite qualquer header customizado no request (ex.: Authorization: Bearer ...)
            .AllowAnyHeader()
            // Permite qualquer verbo HTTP (GET, POST, PUT, DELETE, etc.)
            .AllowAnyMethod();
    });
});

// Application & Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Swagger com JWT
builder.Services.AddSwaggerWithJwt();

// ---------------------------------------------------------------------------
// Banco de dados: SQL Server via Entity Framework Core.
// ---------------------------------------------------------------------------
// A string de conexão vem de ConnectionStrings:DefaultConnection
// (appsettings.{Environment}.json, variável de ambiente
// ConnectionStrings__DefaultConnection, ou User Secrets em dev).
// UseSqlServer configura o provider do EF Core para gerar T-SQL e usar o
// driver Microsoft.Data.SqlClient por baixo — troca direta do que antes
// era Pomelo.EntityFrameworkCore.MySql (UseMySql), agora alinhado com o
// SQL Server que o docker-compose.yml já sobe (mcr.microsoft.com/azure-sql-edge).
builder.Services.AddDbContext<OrderManagementDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// A ordem dos middlewares importa. CORS precisa rodar ANTES de
// UseAuthentication/UseAuthorization: o navegador manda um preflight
// (OPTIONS) sem token nenhum, então a política de CORS tem que responder
// a esse preflight antes de qualquer verificação de autenticação acontecer.
app.UseCors(AngularCorsPolicy);

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
   var context = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();

   context.Database.Migrate();

   DatabaseSeeder.Seed(context);
}

await app.RunAsync();
