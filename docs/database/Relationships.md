# Relationships

| Parent | Child | FK Column | Cardinality | Delete Behavior |
|---|---|---|---|---|
| Branches | AspNetUsers | BranchId | 1:N (nullable — Admins have none) | Restrict |
| Branches | Customers | BranchId | 1:N | Restrict |
| Branches | Invoices | BranchId | 1:N | Restrict |
| Branches | Payments | BranchId | 1:N | Restrict |
| Branches | CustomerInteractions | BranchId | 1:N | Restrict |
| Customers | CustomerAccounts | CustomerId | 1:1 | Restrict |
| Customers | Invoices | CustomerId | 1:N | Restrict |
| Customers | Payments | CustomerId | 1:N | Restrict |
| Customers | CustomerInteractions | CustomerId | 1:N | Restrict |
| Invoices | InvoiceItems | InvoiceId | 1:N | **Cascade** |
| Invoices | Payments | InvoiceId | 1:N | Restrict |
| Invoices | InstallmentPlans | InvoiceId | 1:1 | Restrict |
| InstallmentPlans | InstallmentSchedules | InstallmentPlanId | 1:N | **Cascade** |
| Payments | Payments | ReversedPaymentId (self) | 1:1 (nullable) | Restrict |
| AspNetUsers | Invoices | CreatedByUserId | 1:N | Restrict |
| AspNetUsers | Payments | ReceivedByUserId | 1:N | Restrict |
| AspNetUsers | InstallmentPlans | ApprovedByUserId | 1:N (nullable) | Restrict |
| AspNetUsers | CustomerInteractions | RecordedByUserId | 1:N | Restrict |
| AspNetUsers | BackupHistories | CreatedByUserId | 1:N | Restrict |

`AuditLogs.UserId`/`AuditLogs.BranchId` are plain columns with **no** foreign key — see
[Delete-Behaviors.md](Delete-Behaviors.md) for why.

## Why only two Cascades

Every other relationship is `Restrict` because the parent side represents financial or
identity data that must never silently disappear when a related row is deleted (see spec
section 8: "Restrict Branch/Customer/Invoice/Payment deletion when referenced"). The two
exceptions:

- **Invoice → InvoiceItems**: cascading here only ever removes draft line items together
  with their own (never-finalized) parent invoice — `InvoiceService` only allows deleting an
  Invoice header at all via `EF`'s cascade path when nothing downstream (Payments,
  InstallmentPlan) references it, and in practice invoices are never hard-deleted by any
  service method; the cascade exists for schema completeness and test/seed cleanup.
- **InstallmentPlan → InstallmentSchedules**: schedule rows have no independent existence
  or business meaning outside their plan.
