# Transactions

Source: `database/transactions/*.sql`, implemented in C# by
`InvoiceService.ActivateAsync`/`CancelAsync`, `PaymentService.RecordPaymentAsync`/
`ReverseAsync`, and `ReconciliationService.ReconcileCustomerAccountAsync`.

| Script | Mirrors | Demonstrates |
|---|---|---|
| `InvoiceTransaction.sql` | `InvoiceService.ActivateAsync` | Invoice + CustomerAccount updated atomically |
| `PaymentTransaction.sql` | `PaymentService.RecordPaymentAsync` | `FOR UPDATE` row lock + three-table update in one transaction |
| `PaymentRollbackDemo.sql` | — | Forced `ROLLBACK` leaves every table byte-for-byte unchanged |
| `PaymentReversal.sql` | `PaymentService.ReverseAsync` | Linked reversal row, never a `DELETE` |
| `Reconciliation.sql` | `ReconciliationService` | Recalculation from source rows + audit trail |
| `ACID-Demonstrations.sql` | — | Guided tour of all four ACID properties, including a two-session isolation walkthrough |

## Isolation level

MySQL's default `REPEATABLE READ`, combined with explicit `SELECT ... FOR UPDATE` locks on
the specific `Invoices` row being modified. This project does not lower isolation to `READ
UNCOMMITTED` anywhere. See `ACID-Demonstrations.sql`'s Isolation section for the exact
two-session sequence, and `ConcurrentPaymentTests` for the automated version of the same
test using two independent `DbContext`/connections.

## Why locking happens at the application layer, not purely via triggers

A `SELECT ... FOR UPDATE` lock only has meaning within an open transaction spanning
multiple statements (lock → validate → write → commit) — that shape belongs in
`PaymentService`, not in a single-statement trigger. Triggers (see
[Triggers.md](Triggers.md)) handle the orthogonal concern of rejecting invalid data
regardless of which transaction wrote it.
