# Lab: Triggers

**Goal**: create the eight triggers and prove each one actually fires.

## Steps

1. Run `database/triggers/CreateTriggers.sql`.
2. Run `database/verification/VerifyTriggers.sql` section 1 (trigger inventory) and confirm
   all eight rows appear.
3. Run section 2 (invoice item with `Quantity = 0`) and confirm it fails with a
   `SIGNAL SQLSTATE '45000'` error mentioning "quantity must be greater than zero" —
   `trg_InvoiceItems_BeforeInsert` firing.
4. Insert a valid payment against an Active invoice, then run section 4's `SELECT * FROM
   AuditLogs WHERE ActionType = 'TriggerAuditInsert'` and confirm a new row appears
   automatically — `trg_Payments_AfterInsert_Audit` firing without any application code
   having written it.
5. Attempt `DELETE FROM Payments WHERE PaymentId = <any existing id>` and confirm it fails
   with `trg_Payments_BeforeDelete`'s message ("Payments can never be physically deleted").
6. Attempt `DELETE FROM Customers WHERE CustomerId = <a customer with a non-zero balance>`
   and confirm `trg_Customers_BeforeDelete` rejects it; then try a customer whose
   `CustomerAccounts.CurrentBalance = 0` and confirm the delete is **not** blocked by this
   trigger (it may still be blocked by the FK if invoices reference the customer — that is
   the FK's job, not this trigger's).
7. Run `database/triggers/DropTriggers.sql`, re-run `CreateTriggers.sql`, and repeat step 3
   to confirm the recreated trigger still works identically.

## Expected outcomes

- Every `SIGNAL` message is business-readable, not a raw MySQL error number.
- The audit trigger produces a row with `BranchId` populated correctly from `NEW.BranchId`.
- Drop/recreate is clean and idempotent.

## Automated coverage

These triggers only fire against a live MySQL server — no equivalent unit test exists
(EF Core InMemory would not run them, which is exactly why this project avoids relying on
InMemory for anything relational). Verify manually per the steps above.
