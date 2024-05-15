# Views

Source: `database/views/CreateViews.sql`. All six required views, verified with
`database/verification/VerifyViews.sql`.

| View | Purpose | Key columns |
|---|---|---|
| `vw_CustomerAccountSummary` | One row per customer: billing/payment totals + balance | CustomerId, TotalInvoices, TotalBilled, TotalPaid, OutstandingBalance |
| `vw_InvoicePaymentStatus` | Every invoice with customer name and payment state | InvoiceNumber, CustomerName, TotalAmount, OutstandingAmount, PaymentStatus |
| `vw_OverdueInstallments` | Pending installment rows past their due date | InstallmentNumber, DueDate, DaysOverdue |
| `vw_BranchRevenueSummary` | Per-branch billing/collection totals | BranchName, TotalBilled, TotalCollected, TotalOutstanding |
| `vw_CustomerInteractionHistory` | Every interaction with customer + staff names | InteractionType, Subject, StaffName |
| `vw_DailyTransactionSummary` | Daily payment totals broken out by method | TransactionDate, CashAmount, BankTransferAmount, CardAmount |

## Design notes

- `vw_BranchRevenueSummary` computes `TotalCustomers` in a derived subquery **before**
  joining to invoices, specifically to avoid join fan-out inflating the count when a branch
  has many invoices per customer — see the comment in `CreateViews.sql`.
- `vw_OverdueInstallments`'s `DaysOverdue` column is computed with `DATEDIFF(UTC_TIMESTAMP(), DueDate)`
  at query time — a view cannot itself be indexed, but the underlying
  `IX_InstallmentSchedules_Status_DueDate` index keeps the `WHERE Status='Pending' AND DueDate < NOW()`
  predicate cheap.
- None of the six views are materialized — MySQL re-evaluates them on every query. This is
  intentional at this project's scale; a materialized/pre-aggregated table is only justified
  if a specific report becomes a demonstrated bottleneck (spec section 11: "Pre-aggregated
  tables only where explicitly justified").

## A note on what a MySQL view is (and isn't)

A normal MySQL view is **not** a physical table and cannot have its own `CREATE INDEX`.
Every view above is only as fast as the indexes on the tables it joins — see
[Indexes.md](Indexes.md) for the index that backs each view's main predicate.
