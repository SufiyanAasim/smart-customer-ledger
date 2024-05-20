# Viva Questions and Answers

## Project overview

**Q: What problem does this project solve, in one sentence?**
A: It lets a multi-branch retail business sell on credit/installments while enforcing
financial and referential integrity at the database level, not just trusting application
code.

**Q: Why a modular monolith instead of microservices?**
A: The domain doesn't have independently-scalable subdomains that would justify the
operational overhead of microservices for a course project; a modular monolith
(Domain/Application/Infrastructure/Web) gives clean separation of concerns without that cost.

## ERD / Normalization

**Q: Is this schema normalized? To what normal form?**
A: Third normal form (3NF) — every non-key column depends on the whole primary key and
nothing but the key. E.g. `CustomerAccounts` is a separate table from `Customers` rather
than embedding balance columns directly in `Customers`, because the account has its own
lifecycle and independent attributes (CreditLimit, ConcurrencyVersion).

**Q: Why is `CustomerAccount` a separate table instead of columns on `Customer`?**
A: It represents a distinct concept (a financial ledger) with its own concurrency token and
its own one-to-one relationship — keeping it separate also means `CustomerAccount` totals
can be recalculated/reconciled without touching customer profile data at all.

## Keys and constraints

**Q: Why is `AuditLogs.UserId` not a foreign key?**
A: An audit trail must survive even if the referenced user or branch record later changes
or is removed — coupling it with a FK would let deleting/renaming something also silently
lose or block the very audit trail meant to record it. See
[Delete-Behaviors.md](../database/Delete-Behaviors.md).

**Q: Why RESTRICT on almost every FK instead of CASCADE?**
A: Financial and identity data must never disappear as a side effect of deleting something
else. The two exceptions (Invoice→InvoiceItems, InstallmentPlan→InstallmentSchedules) are
child rows with no independent meaning outside their parent, and only fire in practice on a
Draft invoice that was never finalized.

## Indexing

**Q: How did you decide which columns to index?**
A: By working backward from actual query patterns in the service layer (e.g. the invoice
list's `WHERE BranchId=? AND InvoiceStatus=? ORDER BY InvoiceDate`), not by indexing every
column speculatively. See [Indexes.md](../database/Indexes.md) for the query each index
supports.

**Q: Can you index a MySQL view directly?**
A: No — a normal view has no storage of its own. Performance comes entirely from indexes on
the underlying tables the view joins.

## Views

**Q: Why six views specifically, and what's the general shape of each?**
A: Each answers a recurring reporting question that would otherwise require repeating the
same JOIN logic across multiple screens: customer summary, invoice payment status, overdue
installments, branch revenue, interaction history, and daily transactions. See
[Views.md](../database/Views.md).

## Triggers

**Q: Why only eight triggers, not one per business rule?**
A: Triggers are reserved for database-level integrity/audit protection that must hold
regardless of which code path writes to the table — complex multi-step business workflows
stay in C# services where they're testable. See [Triggers.md](../database/Triggers.md)'s
"Design philosophy" section.

**Q: Does a trigger fire when an installment becomes overdue?**
A: No — no INSERT/UPDATE/DELETE occurs merely because time passes, so no trigger can hang
off that. `OverdueInstallmentBackgroundService` (an hourly ASP.NET Core `BackgroundService`)
handles this instead. See [Events.md](../database/Events.md).

## ACID properties

**Q: How is Isolation actually enforced, concretely?**
A: `SELECT ... FOR UPDATE` on the specific invoice row inside `PaymentService`'s
transaction — a second concurrent request touching the same invoice blocks until the first
commits, then re-validates against the now-current balance. Proven by
`ConcurrentPaymentTests`.

**Q: What would happen without that lock?**
A: Two concurrent payments could both read the same `OutstandingAmount`, both pass
validation independently, and both commit — jointly overpaying the invoice. This is
sometimes called a "lost update" or TOCTOU (time-of-check to time-of-use) race.

**Q: Why increment `ConcurrencyVersion` manually instead of using a database rowversion?**
A: MySQL (via Pomelo) doesn't have SQL Server's native `rowversion`/`timestamp` type; a
manually-incremented integer concurrency token, checked by EF Core's optimistic concurrency
feature, achieves the same detection.

## Transactions

**Q: Walk me through what happens, step by step, when a payment is recorded.**
A: See [Payment-Transaction-Flow.md](../diagrams/Payment-Transaction-Flow.md) — lock the
invoice row, validate, insert the payment, update the invoice, update the customer account,
write an audit log entry, commit.

## SQL injection / parameterized queries

**Q: Show me a query in this project and prove it can't be SQL-injected.**
A: Any file in `database/crud/` — every value is a `?` placeholder bound as a real
parameter, never string-concatenated. See
[Parameterized-Queries-Lab.md](../labs/Parameterized-Queries-Lab.md).

## EF Core / Migrations

**Q: How many migrations does this project have, and why so few for six planned releases?**
A: One (`InitialCreate`) as of Chronicle. Balance and Snapshot both added business logic and
SQL scripts without changing the EF Core model — confirmed each time by generating a
migration and observing an empty diff (see [Migration-Lab.md](../labs/Migration-Lab.md)).

## Backup and restore

**Q: What guarantees that a "Completed" backup actually succeeded?**
A: `MySqlBackupService` only sets `Status = Completed` after checking the `mysqldump`
process's exit code **and** confirming the output file exists with a non-zero size —
proven by `BackupServiceTests`, which forces a missing-binary failure and confirms it's
recorded `Failed`, never `Completed`.

## Replication and sharding (concepts, for later releases)

**Q: What's the difference between replication and sharding?**
A: Replication copies the same data to multiple servers for read scaling and availability;
sharding splits **different** data (e.g. by branch) across multiple servers for write
scaling. Replica (v5.0.0) and Shard (v6.0.0) address these respectively — not yet
implemented as of Chronicle.

## Security

**Q: How is branch isolation actually enforced — is it just hiding UI elements?**
A: No — every service method checks `ICurrentUserContext.CanAccessBranch` server-side
before returning or mutating data, independent of anything the UI shows or hides. Proven by
tests that request another branch's record directly (bypassing the UI) and confirm a 403.

## Testing

**Q: Why not use EF Core's InMemory provider for your tests?**
A: It doesn't enforce foreign keys, unique constraints, CHECK constraints, or row locking —
exactly the things this project needs to prove. Using it would let tests pass while the
real MySQL behavior remained unverified.

**Q: What happens to your database tests when there's no MySQL server, like in this
sandbox?**
A: They report `Skipped` with an explicit reason (`MySqlAvailableFactAttribute`), never a
false `Passed`. This is a deliberate honesty discipline, not a workaround.

## Limitations

**Q: What doesn't this project do?**
A: No payment gateway integration, no customer self-service portal, no multi-currency
support, and (until Replica/Shard ship) no horizontal scaling story. See each release
document's "Known Limitations" section for the complete, honest list.
