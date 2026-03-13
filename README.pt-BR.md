# 🧾 Order Management System (OMS)

<a href="./README.md">🇺🇸 English</a> → 🇧🇷 Português

<p align="center">
<img src="https://skillicons.dev/icons?i=dotnet,cs,github,azure,git" />
</p>

<p align="center">

<img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/C%23-Backend-239120?logo=csharp&logoColor=white"/>
<img src="https://img.shields.io/badge/API-REST-00ADD8?logo=swagger&logoColor=white"/>
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

O **Order Management System (OMS)** é uma **API REST profissional desenvolvida com ASP.NET Core**, projetada para simular um ambiente real de desenvolvimento backend utilizado em empresas modernas.

O projeto demonstra diversas **boas práticas de engenharia de software**, incluindo:

- Clean Architecture  
- Testes automatizados  
- Pipeline de CI/CD  
- Análise contínua de qualidade de código com SonarCloud  
- Deploy automatizado em nuvem utilizando Azure  

O objetivo é demonstrar habilidades em **desenvolvimento backend escalável, arquitetura de software e práticas modernas de DevOps**.

---

# ⚡ Funcionalidades

- Autenticação com JWT  
- Gerenciamento de usuários  
- Gerenciamento de pedidos  
- API RESTful  
- Testes automatizados  
- Monitoramento de qualidade de código  
- Pipeline CI/CD  
- Deploy em nuvem  

---

# 🏗 Arquitetura

O projeto segue o padrão **Clean Architecture**, separando responsabilidades em diferentes camadas da aplicação.

```bash
Source
│
├── OrderManagement.API
├── OrderManagement.Application
├── OrderManagement.Domain
├── OrderManagement.Infrastructure
│
Shared
├── OrderManagement.Communication
```

### Domain
Contém as **entidades principais e regras de negócio do sistema**.

### Application
Responsável pelos **casos de uso e serviços da aplicação**.

### Infrastructure
Implementa o **acesso a dados, repositórios e integrações externas**.

### API
Camada responsável por:

- Controllers
- Endpoints REST
- Autenticação JWT
- Configuração da aplicação

---

# 🧰 Tecnologias Utilizadas

### Backend

- ASP.NET Core 8  
- C#  
- Entity Framework Core  
- RESTful API  
- JWT Authentication  

### Banco de Dados

- MySQL  
- Entity Framework Migrations  

### DevOps

- GitHub Actions  
- CI/CD Pipeline  
- Azure App Service  
- Azure Database  

### Qualidade de Código

- SonarCloud  
- Cobertura de testes  
- Análise estática de código  

---

# 📚 Documentação da API

A API possui documentação interativa utilizando **Swagger**.

Após iniciar a aplicação localmente, acesse:


https://localhost:5001/swagger


A interface do Swagger permite **explorar e testar todos os endpoints da API diretamente pelo navegador**.

---

# 🔐 Autenticação

A API utiliza **JWT (JSON Web Token)** para autenticação.

Fluxo de autenticação:

1. Registrar usuário  
2. Fazer login  
3. Receber token JWT  
4. Utilizar o token nos endpoints protegidos  

Header utilizado:


Authorization: Bearer {token}


---

# 👤 Endpoints de Usuários

| Método | Endpoint | Descrição |
|------|------|------|
| POST | `/api/users/register` | Registrar novo usuário |
| POST | `/api/users/login` | Autenticar usuário |
| GET | `/api/users` | Listar usuários |
| GET | `/api/users/{id}` | Buscar usuário por ID |
| PUT | `/api/users/{id}` | Atualizar usuário |
| DELETE | `/api/users/{id}` | Remover usuário |

---

# 📦 Endpoints de Pedidos

| Método | Endpoint | Descrição |
|------|------|------|
| POST | `/api/orders` | Criar pedido |
| GET | `/api/orders` | Listar pedidos |
| GET | `/api/orders/{id}` | Buscar pedido por ID |
| PUT | `/api/orders/{id}` | Atualizar pedido |
| DELETE | `/api/orders/{id}` | Remover pedido |

---

# 🧪 Testes Automatizados

O projeto possui **testes automatizados**, garantindo maior confiabilidade e qualidade do código.

Ferramentas utilizadas:

- xUnit  
- FluentAssertions  
- Coverlet  

A cobertura de testes é monitorada continuamente pelo **SonarCloud**.

---

# ⚙️ Pipeline CI/CD

O projeto utiliza **GitHub Actions** para automação de integração contínua.

Pipeline:


Commit
→
Build
→
Execução de Testes
→
Análise de Qualidade (SonarCloud)
→
Deploy no Azure


Esse fluxo garante:

- Build automatizado  
- Execução de testes  
- Monitoramento da qualidade do código  
- Deploy contínuo  

---

# ☁️ Deploy em Nuvem

A aplicação é implantada utilizando **Azure App Service**.

Infraestrutura utilizada:

- Azure App Service  
- Azure Database  
- GitHub Actions  

---

# 💻 Executar o Projeto Localmente

Siga os passos abaixo para executar o projeto em sua máquina.

## Clonar o repositório

```bash
git clone https://github.com/GuicesarS/Order-Management-System-OMS-.git
```

## Restaurar dependências
```bash
dotnet restore
```

## Executar a aplicação
```bash
dotnet run --project Source/OrderManagement.API
```

## Acessar documentação da API

Após iniciar a aplicação, acesse:

https://localhost:5001/swagger

---

# 👨‍💻 Autor

**Guilherme César Soares**

Desenvolvedor Backend focado em C#, .NET e APIs REST, interessado em arquitetura de software, boas práticas de engenharia e desenvolvimento de aplicações escaláveis.

GitHub
https://github.com/GuicesarS

SonarCloud
https://sonarcloud.io/project/overview?id=GuicesarS_Order-Management-System-OMS-
