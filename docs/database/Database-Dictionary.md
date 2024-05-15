# Database Dictionary

Authoritative index into the other `docs/database/*.md` files — start here.

| Concern | Document | Source of truth |
|---|---|---|
| Column-level detail | [Tables-and-Columns.md](Tables-and-Columns.md) | `02_CreateTables.sql` + EF migration |
| Foreign keys | [Relationships.md](Relationships.md) | EF Fluent configs |
| CHECK/UNIQUE constraints | [Constraints.md](Constraints.md) | `03_AlterTables.sql`, `CreateConstraints.sql` |
| Delete behavior per FK | [Delete-Behaviors.md](Delete-Behaviors.md) | EF Fluent configs |
| Indexes and their query patterns | [Indexes.md](Indexes.md) | `CreateIndexes.sql` |
| The six reporting views | [Views.md](Views.md) | `CreateViews.sql` |
| Triggers | [Triggers.md](Triggers.md) | `CreateTriggers.sql` |
| Scheduled mechanisms | [Events.md](Events.md) | `OverdueInstallmentBackgroundService` |
| Explicit SQL CRUD | [CRUD-Queries.md](CRUD-Queries.md) | `database/crud/*.sql` |
| Transactional workflows | [Transactions.md](Transactions.md) | `database/transactions/*.sql` |
| Seed data | [Seed-Data.md](Seed-Data.md) | `database/seed/*.sql` |

## Tables at a Glance

| Table | Purpose | Row lifecycle |
|---|---|---|
| Branches | Physical business locations | Deactivated, never hard-deleted once referenced |
| AspNetUsers | Staff accounts (extends Identity) | Deactivated via `IsActive` |
| Customers | People/businesses billed | Soft-deleted (`IsDeleted`) or `Status = Inactive` |
| CustomerAccounts | One-to-one financial ledger per customer | Never deleted while the customer exists |
| Invoices | Billing headers | Draft → Active → Cancelled; never hard-deleted |
| InvoiceItems | Invoice line items | Cascade-deleted with a Draft invoice only |
| Payments | Payment records | Never deleted — reversed via a linked row |
| InstallmentPlans | Payment-plan headers | PendingApproval → Active → Completed/Cancelled |
| InstallmentSchedules | Per-installment due rows | Cascade-deleted with their plan |
| CustomerInteractions | Calls/complaints/follow-ups | Status-transitioned, never deleted |
| AuditLogs | Append-only audit trail | Archived, never physically deleted |
| BackupHistories | Backup run outcomes | Append-only |

Two engineering decisions worth calling out explicitly:

1. **Enums are stored as strings** (`VARCHAR`), not integers — `Status`, `PaymentStatus`,
   `InvoiceStatus`, etc. This trades a few bytes of storage for a schema that is
   self-describing in MySQL Workbench without needing the C# source to interpret it. See
   `HasConversion<string>()` in every `*Configuration.cs` file.
2. **`ConcurrencyVersion` is an application-maintained integer**, not a database-generated
   rowversion — every service method that mutates `Invoice`/`CustomerAccount` increments it
   explicitly. See `docs/releases/v2.0.0-Balance.md § Fixed` for why this matters.
