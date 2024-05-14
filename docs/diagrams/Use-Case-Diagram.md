# Use-Case Diagram

```mermaid
graph LR
    Admin([Administrator])
    Manager([Branch Manager])
    Staff([Cashier / Staff])

    subgraph Org-wide
        UC1[Manage Branches]
        UC2[Manage Users]
        UC3[Review Audit Logs]
        UC4[Run Backup / Restore]
        UC5[View Org-wide Reports]
    end

    subgraph Branch-scoped
        UC6[Register Customer]
        UC7[Create / Activate Invoice]
        UC8[Record Payment]
        UC9[Reverse Payment]
        UC10[Create / Approve Installment Plan]
        UC11[Pay Installment]
        UC12[Log Customer Interaction]
        UC13[Reconcile Accounts]
        UC14[Export / Import Data]
    end

    Admin --> UC1
    Admin --> UC2
    Admin --> UC3
    Admin --> UC4
    Admin --> UC5
    Admin --> UC6
    Admin --> UC7
    Admin --> UC8
    Admin --> UC9
    Admin --> UC10
    Admin --> UC13
    Admin --> UC14

    Manager --> UC6
    Manager --> UC7
    Manager --> UC8
    Manager --> UC9
    Manager --> UC10
    Manager --> UC11
    Manager --> UC12
    Manager --> UC13
    Manager --> UC14

    Staff --> UC6
    Staff --> UC7
    Staff --> UC8
    Staff --> UC10
    Staff --> UC11
    Staff --> UC12
    Staff --> UC14
```

UC9 (Reverse Payment) and UC10's approval step are restricted to Administrator/Branch
Manager in code — see `[Authorize(Roles = Roles.Administrator + "," + Roles.BranchManager)]`
on `PaymentsController.Reverse` and `InstallmentPlansController.Approve`. UC4 (Backup/
Restore) is Administrator-only. Every use case is additionally scoped to the actor's own
branch server-side via `ICurrentUserContext`, regardless of what this diagram's grouping
suggests about role capability alone.
