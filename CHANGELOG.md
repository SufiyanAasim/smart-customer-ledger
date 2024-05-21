# Changelog

All notable changes to CustomerLedger are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project follows [Semantic Versioning](https://semver.org/).

## [4.0.0] - Chronicle

### Added

- `docs/proposal/Project-Proposal.md` and `docs/report/Final-Project-Report.md`.
- 10 Mermaid diagrams under `docs/diagrams/` (ER, relational schema, use-case, architecture,
  auth flow, invoice/payment/reversal/installment/backup-restore flows).
- 12-file database dictionary under `docs/database/`.
- 9 hands-on labs under `docs/labs/`.
- 9 testing documents under `docs/testing/`, including a requirements traceability matrix.
- 6 manuals under `docs/manuals/` (installation, configuration, user, admin, DB setup,
  troubleshooting).
- Viva Q&A and demonstration script under `docs/viva/`.
- Grading and submission checklists under `docs/grading/`.

Documentation-only release — no application, database, or test code changed.

## [3.0.0] - Snapshot

### Added

- `IBackupService`/`MySqlBackupService`: real `mysqldump` execution with actual outcome
  recorded in `BackupHistory` (password via `MYSQL_PWD` env var, never a CLI argument).
- `IRestoreService`/`MySqlRestoreService`: restores a completed backup via the `mysql` client.
- `CsvUtilities`: shared CSV read/write with formula-injection neutralization.
- `IExportService`: CSV export for Customers/Invoices/Payments, JSON for Customers, and a
  combined CSV account statement per customer.
- `IImportService`: preview-then-confirm validated CSV import for Customers, with
  duplicate/required-field rejection reporting.
- Web UI: backup/restore controls (Admin), export links on Customer/Invoice/Payment lists,
  and a Customers import screen.
- `database/seed/DemonstrationSeed.sql` and `LargeDatasetSeed.sql`.
- New tests: `CsvUtilitiesTests` (10, no DB needed) and `BackupServiceTests` (MySQL-gated,
  confirms a missing `mysqldump` binary is recorded as Failed, never Completed).

## [2.0.0] - Balance

### Added

- Transactional invoice activation/cancellation that syncs `CustomerAccount.TotalBilled` and
  `CurrentBalance`.
- Transactional payment posting with a `SELECT ... FOR UPDATE` row lock on the invoice, and
  full payment reversal (`IPaymentService.ReverseAsync`) via a linked, never-deleted row.
- `IInstallmentScheduleService.PayInstallmentAsync` — pays one schedule row by delegating to
  the payment-posting code path, and auto-completes the parent plan once every row is settled.
- `OverdueInstallmentBackgroundService` — hourly sweep that transitions Pending schedule rows
  past their due date to Overdue.
- `IReconciliationService` — recalculates and corrects a customer account's totals from
  source rows, with an audit trail of what changed.
- Web UI: payment reversal, per-installment "Pay" action, and an Admin reconciliation screen.
- SQL ACID demonstration scripts under `database/transactions/` (invoice activation,
  payment posting, forced rollback, reversal, reconciliation, and a guided ACID walkthrough).
- New tests: payment reversal, reconciliation, installment payment processing, and a
  genuine concurrency test racing two independent database connections against the same
  invoice.

### Changed

- Every service method that mutates `Invoice` or `CustomerAccount` now increments
  `ConcurrencyVersion` explicitly, so optimistic concurrency detection (configured since
  Index but previously inert) actually works.

### Fixed

- `CustomerAccount.TotalBilled`/`TotalPaid`/`CurrentBalance` were never updated by any
  Index-era code path — Balance wires this up at invoice activation/cancellation and
  payment posting/reversal.

## [1.0.0] - Index

### Added

- Solution scaffolding: CustomerLedger.slnx with Domain/Application/Infrastructure/Web
  projects plus UnitTests/IntegrationTests/DatabaseTests.
- Domain layer: 11 business entities, 13 enums, ApplicationUser (extends ASP.NET Core
  Identity), Roles constants.
- EF Core Fluent API configuration for every entity (keys, constraints, decimal precision,
  indexes, delete behaviors, enum-as-string conversions) and the InitialCreate migration
  targeting MySQL via Pomelo.EntityFrameworkCore.MySql.
- ASP.NET Core Identity with claims-based branch context (`ICurrentUserContext`), role
  seeding, and configuration-driven Administrator seeding (no hardcoded credentials).
- Branch, Customer, CustomerAccount, Invoice/InvoiceItem, Payment, InstallmentPlan/Schedule,
  CustomerInteraction, AuditLog, and BackupHistory service implementations with
  server-side branch isolation on every operation.
- Full Web layer: Program.cs wiring, Identity area (login/logout, no public
  self-registration), Admin area (Branches, Users, AuditLogs, BackupHistories), and
  Customer/Invoice/Payment/InstallmentPlan/CustomerInteraction CRUD modules with search,
  filtering, and pagination.
- Charcoal + Emerald Bootstrap theme and an SVG ledger-and-coin logo.
- Explicit parameterized SQL CRUD scripts for all 12 core tables.
- The six required SQL reporting views (`vw_CustomerAccountSummary`,
  `vw_InvoicePaymentStatus`, `vw_OverdueInstallments`, `vw_BranchRevenueSummary`,
  `vw_CustomerInteractionHistory`, `vw_DailyTransactionSummary`).
- Safe initial triggers for invoice-item validation, payment-vs-cancelled-invoice
  rejection, financial audit logging, and physical-deletion prevention.
- Database verification scripts (schema, constraints, views, triggers, seed data) and a
  development seed script.
- xUnit test suite: 10 passing unit tests (no external dependency) plus database/
  integration tests that skip cleanly with a clear reason when no MySQL server is reachable.

### Security

- Every SQL statement in `database/crud/` and every EF Core query uses parameters — no
  string-concatenated SQL anywhere in the codebase.
- Anti-forgery tokens on all state-changing forms; ViewModels (not raw entities) bind
  incoming form data.
- Branch-level authorization enforced server-side in the Infrastructure service layer, not
  only hidden in the UI.
- `appsettings.json` ships with an empty connection string; real values live in user
  secrets or environment variables — see `appsettings.Example.json`.

### Known Limitations

- Full ACID/rollback/concurrency demonstrations ship with v2.0.0 — Balance.
- Backup/restore execution, export/import, and demonstration/large-volume seeders ship
  with v3.0.0 — Snapshot.
- Full academic documentation package ships with v4.0.0 — Chronicle.
- Read replica and sharding ship with v5.0.0 — Replica and v6.0.0 — Shard respectively.
