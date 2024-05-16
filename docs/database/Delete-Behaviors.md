# Delete Behaviors

## Foreign-key ON DELETE actions

See [Relationships.md](Relationships.md) for the full table — summarized rule: **Restrict**
everywhere except Invoice→InvoiceItems and InstallmentPlan→InstallmentSchedules, which
**Cascade**.

## Why AuditLogs has no foreign keys

`AuditLogs.UserId` and `AuditLogs.BranchId` are plain, unconstrained columns rather than
foreign keys to `AspNetUsers`/`Branches`. An audit trail exists precisely to survive changes
to the things it describes — if a user account were later removed, or a branch code
reused, the audit row must still show what happened and who/where it happened, not become
an orphaned FK violation or (worse) block the very deletion it should be recording. This is
a deliberate schema decision, not an oversight.

## Why the application never issues a hard DELETE on financial rows

Every domain-level "delete" workflow described in the project specification is implemented
as a state change, not a `DELETE` statement:

| Entity | "Delete" action | Actual implementation |
|---|---|---|
| Branch | Deactivate | `UPDATE Branches SET IsActive = 0` |
| Customer | Deactivate / soft delete | `UPDATE Customers SET Status='Inactive'` or `IsDeleted=1` |
| Invoice | Cancel | `UPDATE Invoices SET InvoiceStatus='Cancelled'` (only if zero completed payments) |
| InvoiceItem | Delete | Real `DELETE`, but **only** while the parent invoice is Draft |
| Payment | Reverse | `UPDATE` original to `Reversed` + `INSERT` a linked reversal row — never a `DELETE` |
| InstallmentPlan | Cancel | `UPDATE ... SET Status='Cancelled'` |
| CustomerInteraction | Archive/close | `UPDATE ... SET Status='Closed'` |
| AuditLog | Archive | `UPDATE ... SET IsArchived=1` |
| BackupHistory | (none) | Permanent audit trail — never deleted |

The one real `DELETE` in the entire application is `InvoiceService.RemoveItemAsync`, and
it is guarded by `LoadEditableDraftAsync`'s check that the invoice is still `Draft` — once
an invoice is `Active`, its items become immutable, matching the CASCADE relationship's
intent (a cascade delete of items only ever happens alongside deleting an invoice that was
never finalized in the first place).

Database-level `BEFORE DELETE` triggers on `Customers`, `Invoices`, and `Payments`
(see [Triggers.md](Triggers.md)) additionally reject any direct SQL `DELETE` attempt that
would violate these rules, as a second line of defense independent of the application code.
