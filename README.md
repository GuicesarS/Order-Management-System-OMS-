# 🧾 Order Management System (OMS)

🇺🇸 English → <a href="./README.pt-BR.md">🇧🇷 Português</a>

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

# 🚀 Overview

The **Order Management System (OMS)** is a **professional REST API built with ASP.NET Core**, designed to simulate a real-world backend environment used in modern companies.

The project demonstrates several **software engineering best practices**, including:

- Clean Architecture  
- Automated Testing  
- CI/CD Pipeline  
- Continuous Code Quality Analysis with SonarCloud  
- Automated Cloud Deployment using Azure  

The goal is to showcase skills in **scalable backend development, software architecture, and modern DevOps practices**.

---

# ⚡ Features

- JWT Authentication  
- User Management  
- Order Management  
- RESTful API  
- Automated Testing  
- Code Quality Monitoring  
- CI/CD Pipeline  
- Cloud Deployment  

---

# 🏗 Architecture

The project follows the **Clean Architecture** pattern.

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
Core business entities and rules.

### Application
Application services and use cases.

### Infrastructure
Data access and external integrations.

### API
REST endpoints and application configuration.

---

# 🧰 Tech Stack

### Backend

- ASP.NET Core 8  
- C#  
- Entity Framework Core  
- REST API  
- JWT Authentication  

### Database

- MySQL  
- Entity Framework Migrations  

### DevOps

- GitHub Actions  
- CI/CD Pipeline  
- Azure App Service  
- Azure Database  

### Code Quality

- SonarCloud  
- Test Coverage  
- Static Code Analysis  

---

# 📚 API Documentation

The API includes interactive documentation using **Swagger**.

After running the project locally:


https://localhost:5001/swagger


---

# 🔐 Authentication

The API uses **JWT (JSON Web Token)** authentication.

Flow:

1. Register user  
2. Login  
3. Receive JWT Token  
4. Use token in protected endpoints  

Header:


Authorization: Bearer {token}


---

# 👤 User Endpoints

| Method | Endpoint | Description |
|------|------|------|
| POST | `/api/users/register` | Register new user |
| POST | `/api/users/login` | Authenticate user |
| GET | `/api/users` | List users |
| GET | `/api/users/{id}` | Get user by ID |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Delete user |

---

# 📦 Order Endpoints

| Method | Endpoint | Description |
|------|------|------|
| POST | `/api/orders` | Create order |
| GET | `/api/orders` | List orders |
| GET | `/api/orders/{id}` | Get order by ID |
| PUT | `/api/orders/{id}` | Update order |
| DELETE | `/api/orders/{id}` | Delete order |

---

# 🧪 Testing

The project includes automated tests using:

- xUnit  
- FluentAssertions  
- Coverlet  

Test coverage is monitored continuously by **SonarCloud**.

---

# ⚙️ CI/CD Pipeline

The project uses **GitHub Actions**.

Pipeline:


Commit
→
Build
→
Run Tests
→
SonarCloud Analysis
→
Deploy to Azure


---

# ☁️ Cloud Deployment

The application is deployed using **Azure App Service**.

Infrastructure:

- Azure App Service  
- Azure Database  
- GitHub Actions  

---

# 💻 Running Locally

Clone the repository:

```bash
git clone https://github.com/GuicesarS/Order-Management-System-OMS-.git
```
## Restore dependencies:
```bash
dotnet restore
```
## Run the application:
```bash
dotnet run --project Source/OrderManagement.API
```
## Access Swagger:
https://localhost:5001/swagger

---

# 👨‍💻 Author

**Guilherme César Soares**

Backend Developer — C# / .NET  

GitHub  
https://github.com/GuicesarS

SonarCloud  
https://sonarcloud.io/project/overview?id=GuicesarS_Order-Management-System-OMS-
