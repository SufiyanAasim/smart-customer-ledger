# CRUD Queries

Every core table has an explicit, parameterized SQL CRUD script under `database/crud/` —
this document is an index and summary, not a duplicate of the SQL itself.

| File | Table | Notable statements beyond basic CRUD |
|---|---|---|
| `Branches_CRUD.sql` | Branches | Deactivate/reactivate instead of DELETE; branch+customer-count JOIN |
| `ApplicationUsers_CRUD.sql` | AspNetUsers | Role JOIN; never writes PasswordHash directly |
| `Customers_CRUD.sql` | Customers | Transaction example: customer + account creation together |
| `CustomerAccounts_CRUD.sql` | CustomerAccounts | Optimistic-concurrency UPDATE (`WHERE ConcurrencyVersion = ?`) |
| `Invoices_CRUD.sql` | Invoices | Activate/Cancel state transitions; cancel guarded by `NOT EXISTS (completed payments)` |
| `InvoiceItems_CRUD.sql` | InvoiceItems | INSERT/UPDATE/DELETE all guarded by `JOIN Invoices ... InvoiceStatus='Draft'` |
| `Payments_CRUD.sql` | Payments | Full transactional posting (`FOR UPDATE`) and reversal, both inline |
| `InstallmentPlans_CRUD.sql` | InstallmentPlans | Approve/Cancel state transitions |
| `InstallmentSchedules_CRUD.sql` | InstallmentSchedules | Overdue filter; payment-processing UPDATE |
| `CustomerInteractions_CRUD.sql` | CustomerInteractions | Upcoming follow-up worklist query |
| `AuditLogs_CRUD.sql` | AuditLogs | No UPDATE of the audit payload — only ReviewStatus/AdminNote |
| `BackupHistory_CRUD.sql` | BackupHistories | No DELETE statement at all |

## Parameterization

Every statement in every file uses `?` placeholders. In application code, these map to
`MySqlCommand.Parameters` (MySqlConnector) or EF Core LINQ (which parameterizes
automatically) — never string concatenation. See
[docs/labs/Parameterized-Queries-Lab.md](../labs/Parameterized-Queries-Lab.md) for a
hands-on comparison against the vulnerable alternative.

## Calculation formulas used throughout

```
LineTotal      = (Quantity × UnitPrice) − DiscountAmount + TaxAmount
Invoice.TotalAmount        = Subtotal − DiscountAmount + TaxAmount
Invoice.OutstandingAmount  = TotalAmount − PaidAmount
CustomerAccount.CurrentBalance = TotalBilled − TotalPaid
```

Defined once in `InvoiceCalculationService` (C#) and mirrored in the SQL scripts' comments
— never recomputed differently in two places.
