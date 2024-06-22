<div align="center">

<img src="src/CustomerLedger.Web/wwwroot/images/logo.png" alt="Smart Customer Ledger Logo" width="110" />

# Smart Customer Ledger

**A multi-branch customer billing, credit limit, payment, installment, and customer interaction tracking system**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)](docs/development/Development.md)
[![Version](https://img.shields.io/badge/version-7.0.0%20Capital-10b981?style=flat)](docs/releases/v7.0.0-Capital.md)
[![Database](https://img.shields.io/badge/Database-MySQL%208.0-003B57?style=flat&logo=mysql&logoColor=white)](docs/architecture/Architecture.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-22c55e?style=flat)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux-64748b?style=flat)]()
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-0ea5e9?style=flat)](CONTRIBUTING.md)

Manages multi-branch customer accounts, processes atomic payment settlements with remainder splitting, calculates credit default probabilities using logistic regression machine learning, segments accounts via RFM analysis, isolates branch access via `ICurrentUserContext`, and runs as a standalone single-file `.exe` bundle — all with zero external dependencies.

[**Download .exe**](publish/CustomerLedger.Web.exe) · [**Changelog**](CHANGELOG.md) · [**Roadmap**](ROADMAP.md) · [**Report a Bug**](.github/ISSUE_TEMPLATE/bug_report.md)

</div>

---

## ✨ Features

### 🔐 User Portal & Multi-Branch Access Control
- Role-based security (Administrator, Branch Manager, Staff/Cashier) with ASP.NET Core Identity
- Default administrator credentials (`admin@scl.com` / `admin@584`)
- Strict multi-branch data isolation enforced in service layer via `ICurrentUserContext`

### 💳 Ledger Management Engine
- Customer registration with credit limit validation and account balance tracking
- Billable invoice generation, payment allocation, and installment schedule splitting
- Customer interaction logging for support follow-ups and account notes

### ⚡ ACID Financial Settlement Workflows
- Row-level database locking during invoice payments to prevent concurrent balance drift
- Atomic payment reversal and installment remainder redistribution
- Automated account balance reconciliation service

### 🤖 ML Credit Risk & RFM Analytics
- From-scratch supervised Logistic Regression model predicting customer payment default probability
- Recency, Frequency, and Monetary (RFM) quartile scoring with automated customer segment classification
- Dedicated Smart AI Credit & Ledger Assistant mode (`/Analytics?mode=smart`)

### 🎨 Responsive Dark/Light Theme System
- Charcoal and Emerald modern design system with 1-click persistent theme toggle
- Enlarged high-resolution branding logo across all screens
- Unified right header dropdown pill housing theme options, admin links, and red logout button

### 📄 Executive Audit & Backup Utilities
- Database backup/restore engine supporting `mysqldump` and SQL script execution
- CSV/JSON data import/export utilities with formula injection neutralization

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           CustomerLedger.Web                             │
│       ASP.NET Core MVC (Controllers, Razor Views, Identity Auth)        │
└────────┬───────────────┬──────────────┬──────────────────┬───────────────┘
         │               │              │                  │
         ▼               ▼              ▼                  ▼
CustomerService   InvoiceService  PaymentService  CustomerRiskScoringService
(Profile mgmt)   (Billing logic)  (ACID settlement) (Logistic Regression ML)
         │               │              │                  │
         └───────────────┴──────┬───────┴──────────────────┘
                                │
                                ▼
                   CustomerLedger.Infrastructure
                    (EF Core DbContext & MySQL)
```

Full architectural breakdown in [docs/architecture/Architecture.md](file:///d:/Completed%20Github%20Projects%20%28Fully%20Tested%20&%20Deployed%29/Smart%20Customer%20Ledger/docs/architecture/Architecture.md).

---

## 🛠️ Technology Stack

| Component | Framework / Tool | Purpose |
|-----------|------------------|---------|
| **Core Framework** | C# .NET 8 LTS | Web application runtime & MVC pipeline |
| **Database ORM** | EF Core 8 & Pomelo | Relational database mapping & migrations |
| **Database** | MySQL 8.0 / InMemory | Primary data persistence engine |
| **Authentication** | ASP.NET Core Identity | Claims-based authentication & RBAC |
| **Frontend** | Razor Views & Bootstrap 5 | Modern responsive web UI |
| **Testing** | xUnit & Moq | Unit and integration test validation |

---

## 🚀 Getting Started

### Requirements
- Windows OS / Linux / macOS
- .NET 8.0 SDK or higher

### Quick Start

```bash
git clone https://github.com/SufiyanAasim/Smart-Customer-Ledger.git
cd "Smart-Customer-Ledger"
dotnet build
dotnet run --project src/CustomerLedger.Web
```

Access the application in your browser at `http://localhost:5260`.

**Default Login Credentials:**
- Email: `admin@scl.com`
- Password: `admin@584`

---

## 🗂️ Project Structure

```
Smart Customer Ledger/
├── .github/
│   ├── ISSUE_TEMPLATE/            # Bug report, feature, and security templates
│   ├── workflows/                 # CI build and test GitHub Actions
│   ├── CODEOWNERS
│   ├── dependabot.yml
│   └── PULL_REQUEST_TEMPLATE.md
├── docs/
│   ├── architecture/ (Architecture.md)
│   ├── deployment/ (Deployment.md)
│   ├── api/ (API.md)
│   ├── guides/
│   ├── releases/ (v1.0.0-Index.md to v7.0.0-Capital.md)
│   ├── development/
│   └── troubleshooting/
├── publish/
│   └── CustomerLedger.Web.exe    # Standalone single-file executable
├── src/
│   ├── CustomerLedger.Domain/     # Core domain entities & value objects
│   ├── CustomerLedger.Application/# Service interfaces, DTOs & business rules
│   ├── CustomerLedger.Infrastructure/# EF Core DbContext, repositories & ML services
│   └── CustomerLedger.Web/        # Web MVC views, controllers & static assets
├── tests/
│   ├── CustomerLedger.UnitTests/  # Business logic & ML unit tests (32 passing)
│   ├── CustomerLedger.DatabaseTests/
│   └── CustomerLedger.IntegrationTests/
├── Dockerfile
├── docker-compose.yml
├── Makefile
├── README.md
├── CHANGELOG.md
├── CONTRIBUTING.md
├── LICENSE
├── RELEASE.md
├── ROADMAP.md
├── SECURITY.md
└── SUPPORT.md
```

---

## 🧪 Testing

Run the automated xUnit unit test suite via CLI:

```bash
dotnet test tests/CustomerLedger.UnitTests/CustomerLedger.UnitTests.csproj
```

**Test Status:** `32 Passed, 0 Failed, 0 Skipped`.

---

## 📦 Building Standalone Executable (.exe)

Compile the self-contained single-file Windows executable with embedded application icon:

```powershell
dotnet publish src/CustomerLedger.Web/CustomerLedger.Web.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/
```

The output binary is staged at `publish/CustomerLedger.Web.exe`.

---

## 🛡️ Security

Smart Customer Ledger enforces HTTPS redirection, parameterized SQL script sanitization, and strict claims-based role policies (`Administrator`, `BranchManager`, `Staff`). See [SECURITY.md](SECURITY.md) to report a vulnerability.

---

## 🤝 Contributor

<table>
  <tr>
    <td align="center">
      <a href="https://github.com/SufiyanAasim">
        <img src="https://github.com/SufiyanAasim.png" width="80" alt="SufiyanAasim"/><br/>
        <sub><b>Mohammad Sufiyan Aasim</b></sub>
      </a><br/>
      <sub>System Architect & Sole Developer</sub>
    </td>
  </tr>
</table>

See [CONTRIBUTING.md](CONTRIBUTING.md) to get involved.

---

## 📄 License

[MIT License](LICENSE) © 2024 Smart Customer Ledger Contributors.
