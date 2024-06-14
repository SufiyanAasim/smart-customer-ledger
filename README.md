<div align="center">

# CustomerLedger

**CustomerLedger: A Multi-Branch Customer Billing, Credit, Payment, Installment, and Customer Interaction Management System Using ASP.NET Core MVC and MySQL**

[![.NET](https://img.shields.io/badge/.NET-8.0%20LTS-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-7.0.0%20Capital-10b981?style=flat)](docs/releases/v7.0.0-Capital.md)
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
- *(v7.0.0 — Capital, bonus)* Customer payment-risk scoring via a from-scratch logistic
  regression model, and RFM customer segmentation — see Admin/Manager → Analytics

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

**v6.0.0 — Shard is the final release of the core, specification-mandated six-release
roadmap.** **v7.0.0 — Capital** is built on top of it as a separately-requested bonus
extension (a real, from-scratch logistic regression risk model plus RFM customer
segmentation — see [docs/releases/v7.0.0-Capital.md](docs/releases/v7.0.0-Capital.md)) and
should be evaluated as an addition, not as satisfying any of the six core releases'
requirements.

## Release Roadmap

| Version | Codename | Focus |
|---|---|---|
| v1.0.0 | Index | Foundation: schema, CRUD, auth, six views, safe triggers |
| v2.0.0 | Balance | Full transactional workflows, ACID demonstrations, reconciliation |
| v3.0.0 | Snapshot | Backup/restore, import/export, seeders, migration procedures |
| v4.0.0 | Chronicle | Full academic documentation, diagrams, labs, viva prep |
| v5.0.0 | Replica | Read/write separation, replica-aware reporting |
| v6.0.0 | Shard | Logical sharding, cross-shard reporting — **final core release** |
| v7.0.0 | Capital | Logistic-regression risk scoring + RFM segmentation — **bonus, shipped** |

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

- [CHANGELOG.md](CHANGELOG.md) and `docs/releases/` (one document per shipped release)
- [docs/proposal/Project-Proposal.md](docs/proposal/Project-Proposal.md) and
  [docs/report/Final-Project-Report.md](docs/report/Final-Project-Report.md)
- [docs/database/Database-Dictionary.md](docs/database/Database-Dictionary.md) — start here
  for schema, relationships, constraints, indexes, views, and triggers
- `docs/diagrams/` — ER diagram, architecture, and every major transaction flow (Mermaid)
- `docs/labs/` — 9 hands-on labs (CRUD, views, triggers, ACID, backup/restore, import/
  export, migrations, parameterized queries)
- `docs/testing/` — test strategy/plan/cases and a requirements traceability matrix
- `docs/manuals/` — installation, configuration, user, administrator, database setup, and
  troubleshooting guides
- [docs/viva/Viva-Questions-and-Answers.md](docs/viva/Viva-Questions-and-Answers.md) and
  [docs/viva/Demonstration-Script.md](docs/viva/Demonstration-Script.md)
- `docs/grading/` — grading and submission checklists

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

## Known Limitations (v7.0.0 — Capital + core v6.0.0 — Shard)

- **Capital's risk-scoring label is a same-day heuristic, not a real historical default
  outcome** — see [docs/releases/v7.0.0-Capital.md](docs/releases/v7.0.0-Capital.md) for
  the full, honest methodology discussion. Treat it as a course-level ML demonstration, not
  a production credit-risk model.
- **Capital's model retrains from scratch per request and is not train/test-evaluated** —
  no accuracy/precision/recall metric is reported anywhere.
- **Plain modulus routing (`branchId % activeShardCount`) reshuffles branches when the
  shard count changes** — a production system would use consistent hashing or an explicit,
  persisted branch-to-shard assignment table instead. See
  `database/sharding/ShardRoutingExamples.sql` for a worked example of this exact problem.
- **No distributed transactions across shards** — every financial transaction stays scoped
  to one branch's shard by design; a workflow needing atomicity across two branches on
  different shards would need a saga/compensating-action pattern not implemented here.
- **The main application's CRUD workflows are not actually sharded** — Shard's
  routing/aggregation layer is demonstrated in isolation (Admin → Shard Status), not
  retrofitted through every controller, which would be a far larger and riskier change.
- **Replica ships in simulated mode by default** — a periodic `mysqldump`/`mysql` batch
  resync, not continuous native MySQL replication (fully documented under
  `database/replication/`, but requires two actual MySQL servers to configure). Do not
  present the simulated mode as production-grade high availability.
- Only `vw_BranchRevenueSummary` is wired through the replica-aware and cross-shard
  reporting paths — the pattern extends straightforwardly to the other five views but that
  isn't done in this release.
- Backup/restore requires the `mysqldump`/`mysql` client binaries on the server's PATH.
- Import supports Customers only, not Invoices/Payments.
- No scheduled/automatic backups — every run is Administrator-triggered.
- The testing evidence checklist's screenshots/command-output items are placeholders that
  need manual capture against a real running instance — see
  [docs/testing/Evidence-Checklist.md](docs/testing/Evidence-Checklist.md).
- Manager/Staff do not have dedicated MVC Areas; role- and branch-scoped authorization is
  enforced in the shared controllers/services instead, to avoid duplicating near-identical
  views across three areas.

## License

[MIT](LICENSE)
