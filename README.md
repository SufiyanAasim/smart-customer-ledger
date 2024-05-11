<div align="center">

# CustomerLedger

**CustomerLedger: A Multi-Branch Customer Billing, Credit, Payment, Installment, and Customer Interaction Management System Using ASP.NET Core MVC and MySQL**

[![.NET](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-3.0.0%20Snapshot-10b981?style=flat)](docs/releases/v3.0.0-Snapshot.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-10b981?style=flat)](LICENSE)
[![Database](https://img.shields.io/badge/database-MySQL%208.0-1e293b?style=flat&logo=mysql&logoColor=white)]()

A university Database Management Systems project: multi-branch customer billing, credit tracking, invoicing, payments, installment plans, and customer interaction history — built to demonstrate real relational-database engineering, not just a CRUD skin.

</div>

---

## Project Summary

Electronics shops, furniture stores, repair workshops, and other small/medium retailers routinely sell on credit and in installments across multiple branches, but rarely have software that ties billing, payments, credit limits, and customer follow-ups together with real transactional integrity. CustomerLedger models that workflow end-to-end: register a customer → bill them → take a full/partial/installment payment → keep the customer's account balance, invoice status, and audit trail consistent, all inside a properly normalized MySQL schema with explicit constraints, indexes, views, and triggers — not just an ORM-generated afterthought.

## Main Features

- Multi-branch structure with branch-level data isolation enforced in the service layer
- Role-based access control: Administrator, Branch Manager, Cashier/Staff
- Customer registration with a linked one-to-one financial account (credit limit, balances)
- Draft → Active → Cancelled invoice lifecycle with line items and recalculated totals,
  transactionally synced to the customer's account balance on activation/cancellation
- Payment recording against active invoices with outstanding-balance enforcement, row-locked
  against concurrent overpayment, plus full payment reversal (linked row, never deleted)
- Installment plan creation with automatic schedule generation (remainder-safe splitting)
  and per-installment payment processing
- Account reconciliation: recalculates and corrects a customer's totals from source rows
- Real backup/restore execution (`mysqldump`/`mysql`) with actual outcomes recorded
- CSV/JSON export (customers, invoices, payments, account statements) and validated,
  preview-before-write CSV customer import
- Customer interaction / complaint / follow-up logging
- Administrator audit-log review
- Six required SQL reporting views, safe initial triggers, ACID-transaction SQL
  demonstrations, and 12 explicit parameterized SQL CRUD scripts
- Search, filtering, sorting, and pagination on every list screen

## Technology Stack

| Layer | Technology |
|---|---|
| Backend | C#, ASP.NET Core MVC (.NET 8 LTS) |
| ORM | Entity Framework Core 8 + Pomelo.EntityFrameworkCore.MySql |
| Database | MySQL 8.0 |
| Direct SQL demos | MySqlConnector (parameterized) |
| Auth | ASP.NET Core Identity, claims-based branch context |
| Frontend | Razor Views, Bootstrap 5 (Charcoal + Emerald theme), vanilla JS |
| Testing | xUnit — unit tests (no DB) + database/integration tests against real MySQL |

## Architecture Overview

Modular monolith, four projects under `src/`:

```
CustomerLedger.Domain          entities, enums, constants — no external dependencies
CustomerLedger.Application     service interfaces, DTOs, pure business-rule logic
CustomerLedger.Infrastructure  EF Core DbContext/config, service implementations, Identity
CustomerLedger.Web             MVC controllers, Razor views, Program.cs composition root
```

Controllers stay thin; every branch-isolation and business-rule check happens in the
Infrastructure service layer via `ICurrentUserContext`, never trusting a client-supplied
branch id. See [docs/database/Database-Dictionary.md](docs/database/Database-Dictionary.md)
(added in v4.0.0 — Chronicle) for full schema documentation, and
[docs/releases/v1.0.0-Index.md](docs/releases/v1.0.0-Index.md) for this release's exact scope.

## Current Version

**v3.0.0 — Snapshot** (this release)

## Release Roadmap

| Version | Codename | Focus |
|---|---|---|
| v1.0.0 | Index | Foundation: schema, CRUD, auth, six views, safe triggers |
| v2.0.0 | Balance | Full transactional workflows, ACID demonstrations, reconciliation |
| v3.0.0 | Snapshot | Backup/restore, import/export, seeders, migration procedures |
| v4.0.0 | Chronicle | Full academic documentation, diagrams, labs, viva prep |
| v5.0.0 | Replica | Read/write separation, replica-aware reporting |
| v6.0.0 | Shard | Logical sharding, cross-shard reporting — **latest release** |
| v7.0.0 | Capital | AI/ML/data-mining-driven analytics (planned, not yet started) |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server 8.0+
- (Optional) `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

## Setup Instructions

### 1. Clone and restore

```bash
git clone <this-repository-url>
cd "Smart Customer Ledger"
dotnet restore
```

### 2. MySQL setup

Create the database and application user (adjust the password):

```bash
mysql -u root -p < database/schema/01_CreateDatabase.sql
```

Edit the generated user's password in that script (or via `ALTER USER`) before running it in
a real environment — the shipped script uses a placeholder.

### 3. Configure connection string and secrets (user secrets — never appsettings.json)

```bash
cd src/CustomerLedger.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=customerledger;Uid=customerledger_app;Pwd=<your-password>;"
dotnet user-secrets set "SeedAdmin:Email" "admin@customerledger.local"
dotnet user-secrets set "SeedAdmin:Password" "<a-strong-password>"
```

See [appsettings.Example.json](src/CustomerLedger.Web/appsettings.Example.json) for every
key these seeders/connections read.

### 4. Apply the EF Core migration

```bash
dotnet ef database update --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
```

### 5. Run

```bash
dotnet run --project src/CustomerLedger.Web
```

The first run seeds the three roles and the Administrator account from `SeedAdmin:*`
(nothing is seeded if those keys are absent). In Development, a demonstration "Main Branch"
is also seeded.

### 6. Run tests

```bash
dotnet test
```

Unit tests (`tests/CustomerLedger.UnitTests`) run with no external dependency. Database and
integration tests (`tests/CustomerLedger.DatabaseTests`, `tests/CustomerLedger.IntegrationTests`)
require a real MySQL instance — point them at one via:

```bash
export CUSTOMERLEDGER_TEST_CONNECTION="Server=localhost;Port=3306;Database=customerledger_test;Uid=root;Pwd=<password>;"
dotnet test
```

Without a reachable MySQL server, these tests report **Skipped** (with the reason printed),
never a false Pass.

## Default Development Roles

| Role | Scope |
|---|---|
| Administrator | Organization-wide — branches, users, all customers/invoices/payments, audit review |
| Branch Manager | Assigned branch — customers, invoices, installment approval, branch reports |
| Cashier / Staff | Assigned branch — customer registration, invoicing, payments, interactions |

There is no public self-registration screen. Accounts are created by an Administrator under
**Admin → Users**, or by the `SeedAdmin` configuration on first run.

## Database Script Structure

```
database/
  schema/       01_CreateDatabase.sql, 02_CreateTables.sql, 03_AlterTables.sql
  constraints/  CreateConstraints.sql
  indexes/      CreateIndexes.sql, VerifyIndexes.sql
  crud/         explicit parameterized CRUD for all 12 core tables
  views/        CreateViews.sql (6 required views), DropViews.sql
  triggers/     CreateTriggers.sql, DropTriggers.sql
  transactions/ InvoiceTransaction, PaymentTransaction, PaymentRollbackDemo,
                PaymentReversal, Reconciliation, ACID-Demonstrations.sql
  seed/         DevelopmentSeed.sql, DemonstrationSeed.sql, LargeDatasetSeed.sql
  verification/ VerifySchema/Constraints/Views/Triggers/SeedData.sql
```

The EF Core migration (`src/CustomerLedger.Infrastructure/Data/Migrations`) is the schema
actually applied by `dotnet ef database update`; the scripts above mirror that schema for
MySQL Workbench walkthroughs and grading review — see the top of each file for which source
governs which object.

## Documentation

- [docs/releases/v1.0.0-Index.md](docs/releases/v1.0.0-Index.md) — this release's full scope
- [CHANGELOG.md](CHANGELOG.md)
- Full database dictionary, ERD, labs, and viva prep ship with **v4.0.0 — Chronicle**

## Security Notes

- Parameterized SQL everywhere — see every file under `database/crud/` and `MySqlCommand`
  usage patterns; no string-concatenated queries.
- ASP.NET Core Identity handles all password hashing; nothing here ever stores or logs a
  plaintext password, password hash, or security stamp.
- Branch isolation is enforced server-side in the service layer via `ICurrentUserContext`,
  not only hidden in the UI.
- Anti-forgery tokens on every state-changing form; ViewModels (not raw entities) bind
  incoming form data to prevent over-posting.
- Secrets belong in user secrets or environment variables — `appsettings.json` ships with an
  empty connection string on purpose.

## Known Limitations (v3.0.0 — Snapshot)

- Backup/restore requires the `mysqldump`/`mysql` client binaries on the server's PATH.
- Import supports Customers only, not Invoices/Payments (those originate from the app's own
  transactional workflows, not bulk upload).
- No scheduled/automatic backups — every run is Administrator-triggered.
- Full academic documentation package (proposal, report, diagrams, labs, viva prep) ships
  with v4.0.0 — Chronicle.
- No read replica or sharding — those ship with v5.0.0 — Replica and v6.0.0 — Shard.
- Manager/Staff do not have dedicated MVC Areas; role- and branch-scoped authorization is
  enforced in the shared controllers/services instead, to avoid duplicating near-identical
  views across three areas.

## License

[MIT](LICENSE)
