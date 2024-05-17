# Lab: ACID Transactions

**Goal**: observe all four ACID properties directly, both via SQL and via the automated
test suite.

## Part 1 — SQL (two MySQL Workbench tabs)

Follow `database/transactions/ACID-Demonstrations.sql` section by section:

1. **Atomicity**: run `PaymentTransaction.sql`'s full block, confirm all three tables
   (Payments, Invoices, CustomerAccounts) changed together. Then run
   `PaymentRollbackDemo.sql`, deliberately issuing `ROLLBACK` instead of `COMMIT`, and
   confirm the "before" and "after rollback" snapshots are identical.
2. **Consistency**: run the CHECK-constraint negative test (negative `Quantity`) and confirm
   MySQL rejects it (error 3819) rather than storing an inconsistent row.
3. **Isolation**: open two Workbench tabs. In Tab A, `START TRANSACTION; SELECT * FROM
   Invoices WHERE InvoiceId = <id> FOR UPDATE;` and leave it open. In Tab B, run the same
   statement and observe it **block** until Tab A commits or rolls back. This is the exact
   mechanism `PaymentService.RecordPaymentAsync` relies on to prevent two concurrent
   payments from jointly overpaying the same invoice.
4. **Durability**: after committing a payment, restart the MySQL server (or simply
   reconnect in a new session) and confirm the payment row is still present.

## Part 2 — Automated (C#)

```bash
export CUSTOMERLEDGER_TEST_CONNECTION="Server=localhost;Port=3306;Database=customerledger_test;Uid=root;Pwd=<password>;"
dotnet test tests/CustomerLedger.IntegrationTests --filter "FullyQualifiedName~ConcurrentPaymentTests"
```

`ConcurrentPaymentTests.TwoConcurrentPayments_ThatWouldJointlyOverpay_OnlyOneSucceeds` opens
two independent `ApplicationDbContext` instances (two independent connections — exactly
like two browser tabs) and races two payments of 700 against an invoice with an outstanding
balance of 1000. Expect exactly one to succeed and the invoice's final `PaidAmount` to be
`700`, never `1400`.

## Expected outcomes

- Rollback leaves zero trace (Part 1, step 1).
- The CHECK constraint actually rejects invalid data on your MySQL version (verify — see
  the caveat in [docs/database/Constraints.md](../database/Constraints.md) about MySQL
  8.0.16+).
- Tab B visibly blocks in step 3 until Tab A finishes.
- The automated test in Part 2 passes with `PaidAmount == 700`.
