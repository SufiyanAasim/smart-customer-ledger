# Administrator Manual

Everything in [User-Manual.md](User-Manual.md) applies to Administrators too, plus the
following Administrator-only capabilities.

## Managing branches

Admin → Branches → **+ New Branch**. Branches are never physically deleted once they have
any customers/invoices/payments — use **Deactivate** instead.

## Managing users

Admin → Users → **+ New User**. Assign exactly one role (Administrator, Branch Manager, or
Staff) and, for non-Administrators, exactly one branch. Deactivating a user (rather than
deleting) preserves their historical attribution on invoices/payments/audit log entries they
created.

There is no self-registration screen anywhere in the application — this is the only place
new accounts are created (besides the one-time `SeedAdmin` configuration at first run).

## Reviewing the audit log

Admin → Audit Logs. Filter by table name. Mark entries **Reviewed** (with an optional note)
or **Archive** them once no longer relevant. Audit rows are never edited or deleted — only
their review metadata changes.

## Backup and restore

Admin → Backup History:

- **Run Backup Now**: choose Full / SchemaOnly / DataOnly. Requires `mysqldump` on the
  server's PATH (see [Configuration-Guide.md](Configuration-Guide.md) for
  `BackupSettings:MysqldumpPath` if it isn't).
- **Restore**: only available for a Completed backup. Requires typing the literal word
  `RESTORE` to confirm — this overwrites the entire current database.

## Reconciliation

Admin → Reconciliation: choose a branch, **Run Reconciliation**. Every customer account in
that branch is recalculated from its actual invoices and payments; any drift is corrected
and reported (before → after for TotalBilled/TotalPaid/CurrentBalance).

## Approving installment plans

Any Administrator or Branch Manager can approve/cancel a plan from its detail page while it
is `PendingApproval`.

## Cross-branch visibility

As an Administrator, list screens (Customers, Invoices, Payments, Interactions) show all
branches by default; use each screen's branch filter to narrow to one.
