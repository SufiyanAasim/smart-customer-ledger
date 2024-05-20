# Grading Checklist

Maps the authoritative project specification's requirements to where each is satisfied.

## Database design

- [x] Tables, relationships, primary/foreign keys — [Tables-and-Columns.md](../database/Tables-and-Columns.md), [Relationships.md](../database/Relationships.md)
- [x] Constraints (unique, CHECK) — [Constraints.md](../database/Constraints.md)
- [x] Indexes matched to real query patterns — [Indexes.md](../database/Indexes.md)
- [x] At least five (project ships six) meaningful views — [Views.md](../database/Views.md)
- [x] Triggers for business rules, validation, consistency, audit, financial integrity — [Triggers.md](../database/Triggers.md)

## Application

- [x] Frontend CRUD for every business table — see each module's controller/views under `src/CustomerLedger.Web`
- [x] Explicit SQL CRUD scripts for every core table — `database/crud/*.sql`
- [x] Parameterized SQL everywhere, no string concatenation — [Parameterized-Queries-Lab.md](../labs/Parameterized-Queries-Lab.md)
- [x] Real-world business need addressed — [Project-Proposal.md](../proposal/Project-Proposal.md)
- [x] Demonstrates real DBMS concepts, not a superficial CRUD skin — transactions, triggers, views, constraints, all exercised, not just declared

## Transactions and ACID

- [x] Documented and executable ACID demonstrations — `database/transactions/ACID-Demonstrations.sql`, [ACID-Transaction-Lab.md](../labs/ACID-Transaction-Lab.md)
- [x] Automated concurrency test — `ConcurrentPaymentTests`

## Documentation

- [x] Project proposal — [Project-Proposal.md](../proposal/Project-Proposal.md)
- [x] Final project report — [Final-Project-Report.md](../report/Final-Project-Report.md)
- [x] Database documentation — `docs/database/*.md` (12 files)
- [x] Testing evidence — `docs/testing/*.md` (9 files) + [Evidence-Checklist.md](../testing/Evidence-Checklist.md)
- [x] Demonstration instructions — [Demonstration-Script.md](../viva/Demonstration-Script.md)
- [x] Viva preparation — [Viva-Questions-and-Answers.md](../viva/Viva-Questions-and-Answers.md)

## Testing

- [x] Real, executable automated test suite — 51 tests total (20 unit + 5 database + 26 integration) as of Snapshot
- [x] Tests actually run and results honestly reported (not fabricated) — every release document's Tests/Verification sections
- [x] Database-dependent tests skip (not fail) when no MySQL is available, and are documented as needing re-execution against a live server

## Release process

- [x] Release-gated, incremental development — six release documents under `docs/releases/`
- [x] CHANGELOG following Keep a Changelog — `CHANGELOG.md`
- [x] Each release's scope, build result, and test result reported honestly, including what could not be verified in the development sandbox

## Before final submission

- [ ] Re-run the full test suite against a real MySQL 8.0+ instance and update every release
      document's Tests section with actual pass counts (not the sandbox's skip counts)
- [ ] Capture the screenshots/output listed in [Evidence-Checklist.md](../testing/Evidence-Checklist.md)
- [ ] Confirm `mysqldump`/`mysql` client backup/restore actually works end-to-end on the
      grading machine
- [ ] Review [Submission-Checklist.md](Submission-Checklist.md)
