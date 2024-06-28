# Test Strategy

## Levels of testing

| Project | What it tests | External dependency |
|---|---|---|
| `CustomerLedger.UnitTests` | Pure business logic (`InvoiceCalculationService`, `CsvUtilities`, `CurrentUserContext`) | None |
| `CustomerLedger.DatabaseTests` | Schema-level behavior: referential integrity, unique constraints, index usage via `EXPLAIN` | Real MySQL 8.0+ |
| `CustomerLedger.IntegrationTests` | Full service-layer workflows (Customer/Invoice/Payment/Installment/Reconciliation/Backup) and one `WebApplicationFactory` smoke test | Real MySQL 8.0+ |

## Why not EF Core InMemory

The project specification explicitly forbids using the EF Core InMemory provider to claim
relational-database correctness, and this project follows that rule strictly: InMemory does
not enforce foreign keys, unique constraints, CHECK constraints, or `SELECT ... FOR UPDATE`
row locking — all four of which this project specifically needs to prove. Every
database-touching test in this project runs against a real MySQL connection or is skipped.

## The skip-not-fail discipline

`MySqlAvailableFactAttribute` probes the configured MySQL connection at test discovery time
and marks a test `Skip` (not `Fail`) with an explicit reason when no server answers. This
means:

- In an environment without MySQL (e.g. this project's development sandbox), the test run
  reports `Skipped`, never a false `Passed` or a misleading `Failed`.
- In production CI with a real MySQL instance configured via `CUSTOMERLEDGER_TEST_CONNECTION`,
  every one of those same tests actually executes and must pass.

## Test data isolation

Every DB-touching test generates its own uniquely-suffixed data (`Guid.NewGuid()` in
`BranchCode`/`CustomerCode`/etc.) rather than relying on shared seed data, so tests can run
in parallel or repeatedly without colliding.

## Coverage by category

See [Test-Plan.md](Test-Plan.md) for the concrete list, and
[Requirements-Traceability-Matrix.md](Requirements-Traceability-Matrix.md) for which
specification requirement each test maps to.
