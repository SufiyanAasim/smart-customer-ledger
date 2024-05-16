# Lab: SQL CRUD

**Goal**: run the explicit parameterized SQL CRUD scripts directly in MySQL Workbench and
observe them working against real data.

## Prerequisites

MySQL Workbench (or any MySQL client) connected to a `customerledger` database that has had
`database/schema/01_CreateDatabase.sql` through `03_AlterTables.sql` and
`database/seed/DemonstrationSeed.sql` applied.

## Steps

1. Open `database/crud/Customers_CRUD.sql`. Replace the `?` placeholders in the "SELECT by
   primary key" statement with a real `CustomerId` from `DemonstrationSeed.sql` (e.g. the
   row for `DEMO-CUST-001`) and run it.
2. Run the "Search / filter" statement, substituting a partial name (e.g. `'Bilal'`) for the
   `@search` placeholders. Confirm it returns the expected row via `LIKE`.
3. Run the "JOIN example" (customer + branch + account balance) and confirm the balance
   matches what `vw_CustomerAccountSummary` reports for the same customer.
4. Open `database/crud/Invoices_CRUD.sql`. Run the "UPDATE: recalculated totals" statement
   with adjusted `Subtotal`/`TotalAmount` values against a Draft invoice, then run the
   "SELECT by primary key" to confirm the change stuck.
5. Open `database/crud/Payments_CRUD.sql`. Run the transactional INSERT block
   (`START TRANSACTION` ... `COMMIT`) against an Active invoice with a payment amount less
   than its outstanding balance. Confirm the invoice's `PaidAmount`/`OutstandingAmount`
   updated in the same statement block.
6. Attempt the same payment a second time with an amount exceeding the (now smaller)
   outstanding balance directly via raw SQL — note that raw SQL has **no** application-layer
   guard, so this succeeds at the SQL level (unlike going through the app's
   `PaymentService`, which would reject it). This is intentional: it demonstrates why the
   application layer's validation exists in addition to, not instead of, the database
   constraints — see [docs/database/Constraints.md](../database/Constraints.md).

## Expected outcomes

- Every statement uses `?` placeholders — confirm none of the twelve `database/crud/*.sql`
  files contain a string-concatenated value.
- The JOIN and transaction examples in each file demonstrably return/produce correct,
  consistent results.

## Automated coverage

None directly — these are hands-on MySQL Workbench exercises. The equivalent business
logic is covered by `CustomerServiceTests`, `InvoiceServiceTests`, and `PaymentServiceTests`
at the application layer.
