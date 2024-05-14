# Installment Flow

Matches `InstallmentPlanService.CreateAsync` (schedule generation) and
`InstallmentScheduleService.PayInstallmentAsync` exactly.

```mermaid
flowchart TD
    A[Staff creates installment plan\nfor an Active invoice] --> B{Validate:\nplan count > 0,\nStartDate <= EndDate,\nno existing plan,\nDownPayment < Outstanding}
    B -- invalid --> B1[BusinessRuleException]
    B -- valid --> C["TotalInstallmentAmount = Outstanding - DownPayment"]
    C --> D["Generate N schedule rows\n(remainder folded into the last row)"]
    D --> E[Plan status = PendingApproval]
    E --> F{Administrator/\nBranch Manager approves?}
    F -- Cancel --> F1[Plan status = Cancelled]
    F -- Approve --> G[Plan status = Active]
    G --> H[Staff pays one schedule row]
    H --> I["InstallmentScheduleService.PayInstallmentAsync\ndelegates to PaymentService.RecordPaymentAsync"]
    I --> J[Schedule row: AmountPaid += amount]
    J --> K{AmountPaid >= AmountDue?}
    K -- yes --> L[Schedule row Status = Paid, PaidDate set]
    K -- no --> M[Schedule row stays Pending/Overdue]
    L --> N{All schedule rows\nPaid or Cancelled?}
    N -- yes --> O[Plan status = Completed]
    N -- no --> H
```

Overdue transition (Pending → Overdue when `DueDate` passes) is **not** part of this
diagram's event chain — it happens independently, once an hour, via
`OverdueInstallmentBackgroundService`, since no INSERT/UPDATE/DELETE event fires merely
because time passed. See [Triggers.md](../database/Triggers.md) for why this is a
background service rather than a database trigger.
