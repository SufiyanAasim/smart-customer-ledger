# Requirements Traceability Matrix

Maps each major project-specification requirement to the code that implements it and the
test(s) that verify it.

| Requirement | Implementation | Test(s) |
|---|---|---|
| Branch-level data isolation | `ICurrentUserContext.CanAccessBranch`, checked in every service | TC-08, TC-09, TC-18, SEC-03 |
| Role-based authorization | `[Authorize(Roles=...)]` on controllers/actions | SEC-04, SEC-05 |
| Customer registration + linked account | `CustomerService.CreateAsync` + `CustomerAccountService.CreateForCustomerAsync` | TC-06 |
| Unique customer/invoice/payment/branch codes | UNIQUE constraints (see [Constraints.md](../database/Constraints.md)) | TC-07, TC-31 |
| Invoice line-item calculation | `InvoiceCalculationService` | TC-01, TC-02, TC-10 |
| Invoice lifecycle (Draft→Active→Cancelled) | `InvoiceService` | TC-10, TC-11, TC-12 |
| Full/partial payment with balance sync | `PaymentService.RecordPaymentAsync` | TC-13, TC-14, TC-15, TC-16, TC-17 |
| Payment reversal (never deleted) | `PaymentService.ReverseAsync` | TC-19, TC-20, TC-21 |
| Installment plan + schedule generation | `InstallmentPlanService.CreateAsync` | Installment-Flow.md; manual UAT |
| Installment payment processing | `InstallmentScheduleService.PayInstallmentAsync` | TC-22, TC-23 |
| Overdue installment transition | `OverdueInstallmentBackgroundService` | Manual — see [Events.md](../database/Events.md) |
| Account reconciliation | `ReconciliationService` | TC-24, TC-25 |
| ACID transactional integrity | `PaymentService` (`FOR UPDATE`), `database/transactions/*.sql` | TC-26, ACID-Transaction-Lab.md |
| Referential integrity (FK) | EF Fluent config + `03_AlterTables.sql` | TC-29, TC-30 |
| Six required SQL views | `database/views/CreateViews.sql` | Views-Lab.md |
| Database triggers | `database/triggers/CreateTriggers.sql` | Triggers-Lab.md |
| Explicit parameterized SQL CRUD | `database/crud/*.sql` | SEC-01, SQL-CRUD-Lab.md |
| No SQL injection | Parameterized queries throughout | SEC-01, SEC-02 |
| Backup execution with real outcome | `MySqlBackupService` | TC-27 |
| Restore with confirmation | `MySqlRestoreService` + typed confirmation | SEC-12 |
| CSV/JSON export, formula-injection safe | `ExportService`, `CsvUtilities` | TC-05, SEC-08 |
| Validated CSV import | `ImportService` | Import-Export-Lab.md |
| Seed data (dev/demo/large-volume) | `database/seed/*.sql` | Seed-Data.md |
| No hardcoded credentials/branch/user ids | `SeedAdmin:*` config, `ICurrentUserContext` | SEC-10; verified by inspection |
| Anti-forgery / no mass assignment | `[ValidateAntiForgeryToken]`, ViewModels | SEC-06, SEC-09 |
| Automated test suite exists and runs honestly | `MySqlAvailableFactAttribute` skip discipline | Test-Strategy.md |
