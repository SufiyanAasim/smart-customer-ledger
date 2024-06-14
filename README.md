# Smart Customer Ledger

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Version](https://img.shields.io/badge/version-v7.0.0%20Capital-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![.NET Core](https://img.shields.io/badge/.NET-8.0-purple)

## Project Overview

**Smart Customer Ledger** is a multi-branch customer billing, credit limit, payment, installment, and customer interaction tracking system built with C# .NET 8 LTS, ASP.NET Core MVC, EF Core 8, and MySQL 8.0.

It enforces strict multi-branch data isolation via `ICurrentUserContext`, implements ACID transaction handling with row-level locking, and features an integrated Machine Learning payment-risk model and RFM customer segmentation engine.

---

## Features

- **Multi-Branch Isolation**: Strict data access boundaries per branch manager or cashier role.
- **ACID Financial Workflows**: Atomic payments, automated installment remainder splitting, and account balance sweeps.
- **Logistic Regression Risk Model**: From-scratch supervised machine learning to predict account default probability.
- **RFM Customer Segmentation**: Recency, Frequency, and Monetary quartile scoring with automated segment categorization.
- **Theme Mode Switcher**: Seamless dark/light theme switching with state persistence.
- **Enlarged High-Res Brand Logo**: Clean visual branding across all views.
- **Standalone Windows Executable**: Zero-dependency single-file `.exe` bundle for offline execution.

---

## Screenshots

- **Executive Dashboard**: Clean dark/light high-contrast analytics dashboard.
- **Credits & System Specs**: Author leadership, technology stack, and release roadmap.

---

## Architecture

Smart Customer Ledger follows a clean 4-tier modular architecture:

```
src/
├── CustomerLedger.Domain/          # Core Domain Entities, Enums, Constants
├── CustomerLedger.Application/     # Services, Interfaces, DTOs, Business Rules
├── CustomerLedger.Infrastructure/  # EF Core DbContext, Migrations, MySQL Repositories
└── CustomerLedger.Web/             # ASP.NET Core MVC Web Layer, Views, Controllers
```

---

## Technology Stack

- **Framework**: C# .NET 8 LTS (ASP.NET Core MVC)
- **ORM & DB**: Entity Framework Core 8, Pomelo MySQL, MySqlConnector
- **Authentication**: ASP.NET Core Identity (Claims & Policy-Based RBAC)
- **Frontend**: Razor Views, Bootstrap 5 (Charcoal + Emerald Theme), Vanilla JS
- **Testing**: xUnit, Moq, FluentAssertions

---

## Requirements

- .NET 8.0 SDK or higher
- MySQL 8.0 (Optional; defaults to In-Memory DB mode in Development)
- Windows x64 / Linux / macOS

---

## Installation

```bash
git clone https://github.com/SufiyanAasim/Smart-Customer-Ledger.git
cd "Smart-Customer-Ledger"
dotnet restore
```

---

## Quick Start

```bash
dotnet run --project src/CustomerLedger.Web
```

Access the application in your browser at `http://localhost:5260`.

**Default Login Credentials:**
- Email: `admin@scl.com`
- Password: `admin@584`

---

## Configuration

Configuration settings are stored in `src/CustomerLedger.Web/appsettings.json` and `appsettings.Development.json`.

---

## Environment Variables

| Variable | Required | Default | Description |
| :--- | :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | No | `Development` | Hosting environment mode |
| `PORT` | No | `5260` | Web server listening port |
| `ConnectionStrings__DefaultConnection` | Yes | `UseInMemory` | MySQL connection string or InMemory fallback |

---

## Running Locally

```bash
dotnet build
dotnet run --project src/CustomerLedger.Web
```

---

## Docker

```bash
docker-compose up --build
```

---

## Cloud Deployment

Deploy the containerized app or execute the self-contained executable on any Windows x64 server environment.

---

## API Documentation

Detailed endpoint schemas and domain request models are documented in [docs/api/API.md](file:///d:/Completed%20Github%20Projects%20%28Fully%20Tested%20&%20Deployed%29/Smart%20Customer%20Ledger/docs/api/API.md).

---

## Project Structure

Refer to section 1 for the comprehensive file and directory tree layout.

---

## Testing

Run unit tests via CLI:

```bash
dotnet test tests/CustomerLedger.UnitTests/CustomerLedger.UnitTests.csproj
```

---

## Performance

Sub-millisecond ledger aggregation query execution times backed by optimized database indexing.

---

## Security

Enforces HTTPS redirection, parameterized SQL script sanitization, and Identity claim policies.

---

## Contributing

Please review [CONTRIBUTING.md](file:///d:/Completed%20Github%20Projects%20%28Fully%20Tested%20&%20Deployed%29/Smart%20Customer%20Ledger/CONTRIBUTING.md) for contribution guidelines.

---

## Roadmap

Planned releases and feature timelines are documented in [ROADMAP.md](file:///d:/Completed%20Github%20Projects%20%28Fully%20Tested%20&%20Deployed%29/Smart%20Customer%20Ledger/ROADMAP.md).

---

## FAQ

- **Q: Does the app require a running MySQL instance to test?**
  - A: No, it automatically falls back to EF Core In-Memory mode in Development.

---

## Troubleshooting

Refer to [docs/troubleshooting/Troubleshooting.md](file:///d:/Completed%20Github%20Projects%20%28Fully%20Tested%20&%20Deployed%29/Smart%20Customer%20Ledger/docs/troubleshooting/Troubleshooting.md) for common resolution steps.

---

## License

Distributed under the [MIT License](file:///d:/Completed%20Github%20Projects%20%28Fully%20Tested%20&%20Deployed%29/Smart%20Customer%20Ledger/LICENSE).

---

## Acknowledgements

- .NET 8 LTS Core Engineering Team
- Bootstrap 5 & FontAwesome Design System

---

## Support

Refer to [SUPPORT.md](file:///d:/Completed%20Github%20Projects%20%28Fully%20Tested%20&%20Deployed%29/Smart%20Customer%20Ledger/SUPPORT.md) for help and inquiry instructions.
