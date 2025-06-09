# 🗱️ Data Privacy Management System (DPMS)

DPMS is a comprehensive solution designed to help organizations manage data privacy obligations effectively. The system supports data inventory, consent management, risk assessment (e.g. DPIA), incident tracking, and compliance reporting — all in one unified platform.

---

## ✅ Features

* 🔐 **User Management**: Role-based access control (Admin, DPO, Staff, ...)
* 📝 **Consent Management**: Dynamic consent banner and user tracking
* 📄 **Data Inventory**: Register systems, data flows, and processing purposes
* 🧾 **DPIA Workflow**: Risk evaluation and mitigation tracking
* ⚠️ **Incident Reporting**: Log data breach events
---

## 🛠️ Tech Stack

* **Backend**: .NET 8 Web API
* **Frontend**: React (TypeScript)
* **Database**: SQL Server
* **ORM**: Entity Framework Core
* **Authentication**: JWT-based Auth + Feature-based management
* **Storage**: AWS S3 (for document uploads)
* **CI/CD**: GitHub Actions / Azure DevOps ()
---
## 🏦 Architecture

The DPMS follows a clean and modular layered architecture:

- API Layer – Handles incoming HTTP requests and system entry points.
  Includes:
    - Controllers (e.g., ConsentController, DPIAController, ExternalSystemController)
    - Middlewares for cross-cutting concerns (e.g., logging, authentication)

- Service Layer – Contains core business logic and coordinates operations.
  Includes:
    - ConsentService
    - DPIAService
    - ExternalSystemService
    - Other domain-specific services

- Repository Layer – Manages data access using repositories and the Unit of Work pattern.
  Includes:
    - ConsentRepository
    - DPIARepository
    - ExternalSystemRepository

- External Integrations – Connects the system with third-party services.
  Examples:
    - AWS S3 (File storage)
    - SendGrid (Email service)
    - Google AI Studio (LLM features)

- Shared Types – Reusable domain models, utilities, and enums shared across layers.
  Includes:
    - Entities (Consent, DPIA, User, ExternalSystem, etc.)
    - Enums
    - Utils

- Database – The centralized data store backing all persistence operations.
  - DPMS Database

Uses patterns like:

* Repository - Service
* Unit Of Work
* Observer (for notifications)
---

## 🚀 Getting Started

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [SQL Server]
* [Reactjs + Typescript ]

### Clone the repo

```bash
git clone https://github.com/MinhLD2003/dotnet-clean-architecture.git
cd dotnet-clean-architecture
```
---

## ▶️ Running the Application

### Backend (.NET API)

```bash
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend (if applicable)

```bash
cd client
npm install
npm run dev
```

---

## 📘 API Documentation

After running the backend, access Swagger UI:

```
http://localhost:<port>/swagger
```

Includes all endpoints for:

* Authentication
* Users
* Consent
* DPIA
* Risk Assessment
---

## 📁 Project Structure

```
/src
  /DPMS.API           - Entry point (Web API)
  /DPMS.Application   - Use cases, DTOs, services
  /DPMS.Domain        - Entities and core logic
  /DPMS.Infrastructure- EF Core, S3 integration, etc.
  /DPMS.Tests         - Unit and integration tests
```

---

## 🤝 Contributing

1. Fork the project
2. Create your feature branch: `git checkout -b feature/YourFeature`
3. Commit your changes: `git commit -m "Add feature"`
4. Push to the branch: `git push origin feature/YourFeature`
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📬 Contact

* MinhLD2003 - [GitHub Profile](https://github.com/MinhLD2003)
* Email: [your.email@example.com](mailto:your.email@example.com)
