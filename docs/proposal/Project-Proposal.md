# Project Proposal — CustomerLedger

## Project Title

CustomerLedger: A Multi-Branch Customer Billing, Credit, Payment, Installment, and Customer
Interaction Management System Using ASP.NET Core MVC and MySQL

## Introduction

Small and medium retail businesses — electronics shops, furniture stores, mobile/computer
retailers, repair workshops, and wholesale distributors — routinely extend credit and
installment plans to customers across one or more branches. Most such businesses run this
on paper ledgers or disconnected spreadsheets, with no enforced referential integrity, no
audit trail, and no way to answer "how much does this customer owe, across every branch,
right now?" with confidence. CustomerLedger is a database-driven web application that
digitizes this exact workflow, built explicitly to demonstrate sound DBMS engineering
(normalization, constraints, indexes, views, triggers, transactions) rather than a
superficial CRUD skin over a database.

## Background

The idea generalizes a pattern common across many small-business domains: a customer
relationship that spans multiple purchases, partial payments over time, and follow-up
communication, all of which must stay financially consistent even under concurrent access
from multiple staff members at multiple branches.

## Problem Statement

Businesses that sell on credit or in installments across multiple branches lack affordable
software that simultaneously: (1) enforces referential and financial integrity at the
database level, not just in application code; (2) isolates data by branch while still
allowing organization-wide oversight; (3) keeps a full audit trail of financial actions;
and (4) supports the full billing lifecycle — invoice, partial/full payment, installment
plan, payment reversal, and account reconciliation — without manual spreadsheet
reconciliation.

## Proposed Solution

CustomerLedger, an ASP.NET Core MVC application backed by MySQL 8.0, modeling:

- Branches, staff (with role-based access), and customers
- Customer financial accounts (credit limit, running balance)
- Invoices and invoice line items with calculated totals
- Payments (full, partial), payment reversal, and installment plans with generated schedules
- Customer interactions (calls, complaints, follow-ups)
- An append-only audit log and administrator-triggered database backups

Financial workflows run inside database transactions with row-level locking to guarantee
correctness under concurrent access — see
[docs/diagrams/Payment-Transaction-Flow.md](../diagrams/Payment-Transaction-Flow.md) and
`database/transactions/ACID-Demonstrations.sql`.

## Objectives

1. Design a normalized MySQL schema (11 business tables + ASP.NET Core Identity) with
   correct primary/foreign keys, constraints, and indexes.
2. Implement role-based, branch-isolated CRUD for every entity through ASP.NET Core MVC.
3. Demonstrate at least six meaningful SQL views and a set of database-level triggers for
   integrity and audit protection.
4. Implement transactional financial workflows with documented ACID guarantees.
5. Provide backup/restore, data export/import, and seed data workflows.
6. Produce complete academic documentation and a real, executable automated test suite.

## Scope

**In scope**: the modules and workflows listed above, delivered incrementally as six
releases (Index → Balance → Snapshot → Chronicle → Replica → Shard), each documented in
`docs/releases/`.

**Out of scope**: payment gateway integration, SMS/email notification delivery, a native
mobile client, and multi-currency support.

## Stakeholders

- **Business owner / Administrator** — organization-wide oversight, user/branch management.
- **Branch Manager** — branch-level oversight, installment approval, staff activity review.
- **Cashier / Staff** — day-to-day customer registration, invoicing, and payment collection.
- **Customers** — indirect stakeholders; their financial history and privacy (masked CNIC,
  no public self-service portal) are protected by the system's design.
- **Course instructor / evaluator** — assesses the project against the DBMS course rubric.

## Functional Requirements

See section 6 (domain entities) and section 9 (CRUD requirements) of the authoritative
project specification supplied for this course, and their concrete implementation recorded
in `docs/releases/v1.0.0-Index.md` through `v3.0.0-Snapshot.md`.

## Non-Functional Requirements

- **Security**: parameterized SQL everywhere, ASP.NET Core Identity for credentials, CSRF
  protection, server-side branch/role authorization.
- **Data integrity**: foreign keys with `RESTRICT` on financial data, CHECK constraints,
  triggers for cross-table rules, application-layer validation as a second line of defense.
- **Performance**: indexes matched to actual query patterns, verified with `EXPLAIN`.
- **Maintainability**: a modular-monolith architecture (Domain/Application/
  Infrastructure/Web) with a single source of truth for financial calculations.
- **Auditability**: an append-only `AuditLogs` table populated by both application code and
  database triggers.

## Methodology

Incremental, release-gated development. Each release (Index, Balance, Snapshot, Chronicle,
Replica, Shard) is implemented, built, and tested before the next begins; every release ends
with a release document reporting what was actually built and verified versus what still
needs manual verification in an environment with a live MySQL server.

## Technology Stack

C#, ASP.NET Core MVC (.NET 8 LTS), Entity Framework Core 8 with Pomelo's MySQL provider,
MySQL 8.0, ASP.NET Core Identity, Bootstrap 5, and xUnit. See
[README.md](../../README.md#technology-stack) for the full stack table.

## Database Methodology

Entity Framework Core Fluent API is the single source of truth for the schema (via
migrations); parallel hand-written SQL scripts under `database/` mirror that schema for
MySQL Workbench walkthroughs, explicit parameterized CRUD demonstrations, and manual
verification — see `docs/database/Database-Dictionary.md` for which source governs which
object.

## Expected Outcomes

A working, multi-branch billing and credit management system with a real, normalized
database design; six SQL reporting views; database-level triggers; documented and tested
ACID transactional workflows; backup/restore and import/export tooling; and a complete
academic documentation package suitable for course submission and viva defense.

## Project Schedule

| Release | Codename | Focus |
|---|---|---|
| v1.0.0 | Index | Schema, CRUD, auth, six views, safe triggers |
| v2.0.0 | Balance | Transactional workflows, ACID demonstrations, reconciliation |
| v3.0.0 | Snapshot | Backup/restore, import/export, seeders |
| v4.0.0 | Chronicle | Documentation (this document is part of it) |
| v5.0.0 | Replica | Read/write separation |
| v6.0.0 | Shard | Logical sharding |

## Risks

- **MySQL client tools unavailable in the demonstration environment** — mitigated by
  documenting exact manual verification commands in every release document.
- **Scope creep across six releases** — mitigated by a strict "implement only the requested
  release" discipline, recorded release-by-release.
- **Concurrency bugs in financial code** — mitigated by an automated test
  (`ConcurrentPaymentTests`) that races two independent connections against the same invoice.

## Limitations

No payment gateway integration, no customer-facing portal, no multi-currency support, and
(until Replica/Shard ship) no horizontal scaling story — all documented explicitly rather
than silently omitted.
