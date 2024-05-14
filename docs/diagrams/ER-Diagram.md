# ER Diagram

Generated from the actual EF Core Fluent API configuration
(`src/CustomerLedger.Infrastructure/Data/Configurations/`) and the InitialCreate migration —
not drawn freehand. ASP.NET Core Identity tables (AspNetRoles, AspNetUserRoles, etc.) are
omitted for readability except AspNetUsers, which every business table's audit/ownership
columns reference.

```mermaid
erDiagram
    Branch ||--o{ Customer : "has"
    Branch ||--o{ Invoice : "has"
    Branch ||--o{ Payment : "has"
    Branch ||--o{ CustomerInteraction : "has"
    Branch ||--o{ ApplicationUser : "employs"

    Customer ||--|| CustomerAccount : "has exactly one"
    Customer ||--o{ Invoice : "is billed"
    Customer ||--o{ CustomerInteraction : "has"

    Invoice ||--o{ InvoiceItem : "contains"
    Invoice ||--o{ Payment : "receives"
    Invoice ||--o| InstallmentPlan : "may have"

    InstallmentPlan ||--o{ InstallmentSchedule : "generates"

    Payment |o--o| Payment : "reverses"

    ApplicationUser ||--o{ Invoice : "creates"
    ApplicationUser ||--o{ Payment : "receives"
    ApplicationUser ||--o{ CustomerInteraction : "records"
    ApplicationUser ||--o{ InstallmentPlan : "approves"
    ApplicationUser ||--o{ BackupHistory : "creates"

    Branch {
        int BranchId PK
        string BranchCode UK
        string Name
        bool IsActive
    }
    Customer {
        int CustomerId PK
        int BranchId FK
        string CustomerCode UK
        string FullName
        string Status
        bool IsDeleted
    }
    CustomerAccount {
        int CustomerAccountId PK
        int CustomerId FK "UK"
        decimal CreditLimit
        decimal CurrentBalance
        decimal TotalBilled
        decimal TotalPaid
        uint ConcurrencyVersion
    }
    Invoice {
        bigint InvoiceId PK
        int CustomerId FK
        int BranchId FK
        string InvoiceNumber UK
        decimal TotalAmount
        decimal PaidAmount
        decimal OutstandingAmount
        string PaymentStatus
        string InvoiceStatus
        uint ConcurrencyVersion
    }
    InvoiceItem {
        bigint InvoiceItemId PK
        bigint InvoiceId FK
        decimal Quantity
        decimal UnitPrice
        decimal LineTotal
    }
    Payment {
        bigint PaymentId PK
        bigint InvoiceId FK
        int CustomerId FK
        int BranchId FK
        string PaymentNumber UK
        decimal Amount
        string PaymentStatus
        bigint ReversedPaymentId FK
    }
    InstallmentPlan {
        bigint InstallmentPlanId PK
        bigint InvoiceId FK "UK"
        int NumberOfInstallments
        string Status
    }
    InstallmentSchedule {
        bigint InstallmentScheduleId PK
        bigint InstallmentPlanId FK
        int InstallmentNumber
        decimal AmountDue
        decimal AmountPaid
        string Status
    }
    CustomerInteraction {
        bigint CustomerInteractionId PK
        int CustomerId FK
        int BranchId FK
        string InteractionType
        string Status
    }
    AuditLog {
        bigint AuditLogId PK
        string TableName
        string RecordId
        string ActionType
        string ReviewStatus
    }
    BackupHistory {
        bigint BackupHistoryId PK
        string BackupType
        string Status
    }
    ApplicationUser {
        string Id PK
        int BranchId FK
        string EmployeeCode UK
        string FullName
    }
```

`AuditLog` intentionally has no foreign keys to `Branch`/`ApplicationUser` — audit rows must
survive even if the referenced branch or user is later changed, per
[docs/database/Delete-Behaviors.md](../database/Delete-Behaviors.md).
