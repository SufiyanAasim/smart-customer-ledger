# Triggers

Source: `database/triggers/CreateTriggers.sql`, verified by
`database/verification/VerifyTriggers.sql`.

| Trigger | Table / Timing | Purpose |
|---|---|---|
| trg_InvoiceItems_BeforeInsert | InvoiceItems BEFORE INSERT | Rejects Quantity ≤ 0, UnitPrice < 0, or a negative resulting line total |
| trg_InvoiceItems_BeforeUpdate | InvoiceItems BEFORE UPDATE | Same validation on update |
| trg_Payments_BeforeInsert | Payments BEFORE INSERT | Rejects a payment against a Cancelled invoice |
| trg_Payments_AfterInsert_Audit | Payments AFTER INSERT | Writes an `AuditLogs` row independent of the application's own audit call |
| trg_Payments_AfterUpdate_Audit | Payments AFTER UPDATE | Writes an audit row when `PaymentStatus` changes (e.g. a reversal) |
| trg_Customers_BeforeDelete | Customers BEFORE DELETE | Rejects deleting a customer with a non-zero account balance |
| trg_Invoices_BeforeDelete | Invoices BEFORE DELETE | Rejects deleting an invoice with completed payments |
| trg_Payments_BeforeDelete | Payments BEFORE DELETE | Rejects deleting **any** payment, unconditionally |

## Design philosophy: why these eight and not more

The project specification is explicit that triggers should cover **database-level
integrity and audit protection**, not every business rule — complex multi-step workflows
(recalculating invoice totals, syncing a customer account, generating an installment
schedule) stay in `CustomerLedger.Application`/`Infrastructure` services, where they can be
unit-tested and where the C# code is the one place that logic lives.

The eight triggers above exist specifically to catch the case the application layer
**cannot** prevent by construction: a direct SQL statement issued outside the app (a DBA
running an ad hoc `UPDATE`, a bug in a future code path, a misconfigured import script).
They are a second, independent line of defense — not a duplicate calculation engine. None
of them recalculate `Invoice.TotalAmount` or `CustomerAccount.CurrentBalance`; that
arithmetic has exactly one owner (`InvoiceCalculationService` / the transactional service
methods), per the spec's "avoid duplicate financial calculation logic" guidance.

## Trigger safety

- No trigger in this project calls another trigger-bearing table in a way that could
  recurse.
- Every trigger uses `SIGNAL SQLSTATE '45000'` for validation failures, giving a
  business-readable error message instead of a raw constraint-violation error number.
- Performance: all eight triggers are `BEFORE`/`AFTER` row-level triggers on the
  already-indexed `Payments`/`Invoices`/`InvoiceItems`/`Customers` tables — none perform an
  unindexed scan.

## What a trigger cannot do

**A trigger does not fire merely because time passes.** Transitioning an installment
schedule row from `Pending` to `Overdue` when its due date arrives requires an actual
INSERT/UPDATE/DELETE event to hang a trigger off of — since none occurs on its own, this
project uses `OverdueInstallmentBackgroundService` (an hourly `BackgroundService`) instead.
See [Events.md](Events.md).
