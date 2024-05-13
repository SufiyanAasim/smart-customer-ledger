# Final Project Report — CustomerLedger

*Covers v1.0.0 — Index through v3.0.0 — Snapshot. Update this report's Implementation,
Testing, and Results sections at Replica/Shard time rather than duplicating a new report
per release.*

## Abstract

CustomerLedger is a multi-branch customer billing, credit, payment, installment, and
customer-interaction management system built with ASP.NET Core MVC and MySQL 8.0. It
demonstrates a properly normalized relational schema, enforced referential and financial
integrity through constraints and triggers, six reporting views, transactional financial
workflows with documented ACID guarantees, and real backup/restore and data import/export
tooling — implemented incrementally across three shipped releases (Index, Balance,
Snapshot) with a real, executable automated test suite at every step.

## Introduction

See [docs/proposal/Project-Proposal.md](../proposal/Project-Proposal.md) for the full
introduction, problem statement, and stakeholder analysis; summarized here for the report's
own completeness.

## Background

Small and medium retailers selling on credit or in installments across branches typically
lack software that enforces financial integrity at the database level. CustomerLedger
targets that gap directly.

## Problem Statement

See [Project-Proposal.md § Problem Statement](../proposal/Project-Proposal.md#problem-statement).

## Objectives

See [Project-Proposal.md § Objectives](../proposal/Project-Proposal.md#objectives).

## Scope

See [Project-Proposal.md § Scope](../proposal/Project-Proposal.md#scope) and the
release-by-release scope boundaries recorded in each `docs/releases/*.md` file.

## Requirements

Functional and non-functional requirements are recorded in
[Project-Proposal.md](../proposal/Project-Proposal.md#functional-requirements) and
implemented per the domain entity list in
[docs/database/Database-Dictionary.md](../database/Database-Dictionary.md).

## Methodology

Release-gated incremental development (see Project Proposal § Methodology). Each release's
actual scope, build result, and test result are recorded honestly in its own release
document — including what could **not** be verified in the development sandbox (no MySQL
server or `mysqldump`/`mysql` client available there) versus what was verified.

## System Analysis

### Actors

Administrator, Branch Manager, Cashier/Staff — see
[docs/diagrams/Use-Case-Diagram.md](../diagrams/Use-Case-Diagram.md).

### Core Workflow

Customer registration → financial account creation → invoice creation → invoice item
calculation → full/partial/installment payment → invoice and account balance update →
audit log entry → reports and statements. See
[docs/diagrams/Invoice-Transaction-Flow.md](../diagrams/Invoice-Transaction-Flow.md) and
[docs/diagrams/Payment-Transaction-Flow.md](../diagrams/Payment-Transaction-Flow.md).

## System Design

### Architecture

A modular monolith with four projects: `CustomerLedger.Domain` (entities, enums, no
external dependency), `CustomerLedger.Application` (service interfaces, DTOs, pure business
logic), `CustomerLedger.Infrastructure` (EF Core, service implementations, Identity), and
`CustomerLedger.Web` (MVC controllers/views, composition root). See
[docs/diagrams/System-Architecture.md](../diagrams/System-Architecture.md).

Controllers stay thin; every branch-isolation and business-rule check is enforced in the
Infrastructure service layer via `ICurrentUserContext`, never trusting a client-supplied
branch id — this is verified by dedicated branch-isolation tests
(`CustomerServiceTests.GetByIdAsync_FromAnotherBranch_ThrowsBranchAccessDeniedException`,
`PaymentServiceTests.RecordPaymentAsync_FromDifferentBranch_ThrowsBranchAccessDeniedException`).

### Authentication and Authorization

ASP.NET Core Identity with three roles (Administrator, Branch Manager, Cashier/Staff).
Branch membership is carried as a claim (`BranchId`) added at sign-in by
`ApplicationClaimsPrincipalFactory`, read by `ICurrentUserContext` without a database
round-trip per request. See
[docs/diagrams/Authentication-Flow.md](../diagrams/Authentication-Flow.md).

## Database Design

See [docs/database/Database-Dictionary.md](../database/Database-Dictionary.md),
[Tables-and-Columns.md](../database/Tables-and-Columns.md),
[Relationships.md](../database/Relationships.md),
[Constraints.md](../database/Constraints.md),
[Indexes.md](../database/Indexes.md),
[Views.md](../database/Views.md), and
[Triggers.md](../database/Triggers.md) for the full schema documentation, cross-checked
against the actual EF Core migration and SQL scripts.

## Implementation

### Index (v1.0.0)

Domain model, EF Core mappings, ASP.NET Core Identity, CRUD for all 11 business entities,
six SQL views, safe initial triggers, 12 explicit parameterized SQL CRUD scripts. See
[docs/releases/v1.0.0-Index.md](../releases/v1.0.0-Index.md).

### Balance (v2.0.0)

Transactional invoice activation/cancellation synced to customer account balances,
row-locked payment posting (`SELECT ... FOR UPDATE`), payment reversal via a linked
never-deleted row, installment payment processing, an hourly overdue-installment sweep, and
account reconciliation. See [docs/releases/v2.0.0-Balance.md](../releases/v2.0.0-Balance.md).

### Snapshot (v3.0.0)

Real `mysqldump`/`mysql`-backed backup and restore, CSV/JSON export, and a
preview-then-confirm validated CSV customer import. See
[docs/releases/v3.0.0-Snapshot.md](../releases/v3.0.0-Snapshot.md).

## Security

- Every SQL statement — hand-written and EF Core-generated — uses parameters; no string
  concatenation of untrusted input anywhere in the codebase (verified by inspection of
  every file under `database/crud/` and every `MySqlCommand`/LINQ query in the codebase).
- ASP.NET Core Identity owns all password hashing/verification; no code path logs or
  serializes a password, password hash, or security stamp.
- Branch-level authorization is enforced server-side, independent of UI state.
- The backup/restore database password is passed to child processes via the `MYSQL_PWD`
  environment variable, never a visible command-line argument.
- CSV export neutralizes formula injection (a leading `=`, `+`, `-`, or `@` gets a
  defusing apostrophe) — see `CsvUtilitiesTests`.

## Testing

See [docs/testing/Test-Strategy.md](../testing/Test-Strategy.md) for the full strategy and
[docs/testing/Test-Plan.md](../testing/Test-Plan.md) for the test plan. As of Snapshot:

```
CustomerLedger.UnitTests:        20 passed, 0 failed, 0 skipped  (no external dependency)
CustomerLedger.DatabaseTests:     0 passed, 0 failed, 5 skipped  (MySQL unreachable here)
CustomerLedger.IntegrationTests:  0 passed, 0 failed, 23 skipped (MySQL unreachable here)
```

The 28 MySQL-gated tests are real, written tests that skip with an explicit reason rather
than reporting a false pass — see `MySqlAvailableFactAttribute`. They must be re-run against
a live MySQL instance before final grading; the exact command is documented in every
release document's Tests section.

## Findings

- Optimistic concurrency (`ConcurrencyVersion`) was configured since Index but never
  actually incremented by any code path until Balance — a real, documented defect fixed
  mid-project (see [v2.0.0-Balance.md § Fixed](../releases/v2.0.0-Balance.md)).
- `CustomerAccount.TotalBilled`/`TotalPaid`/`CurrentBalance` were similarly inert in Index;
  wiring them into invoice activation/cancellation and payment posting/reversal was Balance's
  central piece of work.

## Results

A working multi-branch billing system covering the full customer lifecycle from
registration through invoicing, payment, installment plans, reversal, and reconciliation,
backed by a schema with 11 business tables, six reporting views, database-level triggers,
documented ACID transactional behavior, and a 51-test automated suite (as of Snapshot).

## Limitations

See each release document's "Known Limitations" section — most notably: no read replica or
sharding until v5.0.0/v6.0.0, no payment gateway integration, and MySQL client tools
required on the server's PATH for backup/restore.

## Future Enhancements

Read/write replica separation (v5.0.0 — Replica), logical sharding (v6.0.0 — Shard), and
(beyond the currently planned v6.0.0 as the final numbered release) a possible v7.0.0 —
Capital exploring AI/ML-driven analytics on top of the existing transactional data.

## Conclusion

CustomerLedger demonstrates that a course DBMS project can go well beyond a superficial
CRUD interface: real transactions with row-level locking, real triggers, real reporting
views, and a real (if partially unverifiable in this specific sandbox) automated test
suite, all built incrementally and documented honestly at every step.

## References

- Microsoft, *ASP.NET Core documentation* — https://learn.microsoft.com/aspnet/core
- Microsoft, *Entity Framework Core documentation* — https://learn.microsoft.com/ef/core
- Pomelo Foundation, *Pomelo.EntityFrameworkCore.MySql* — https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql
- Oracle, *MySQL 8.0 Reference Manual* — https://dev.mysql.com/doc/refman/8.0/en/
