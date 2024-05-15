# Tables and Columns

Column-level reference for every business table. ASP.NET Core Identity's own tables
(AspNetRoles, AspNetUserClaims, etc.) are framework-standard and not repeated here — only
AspNetUsers is listed, since it carries CustomerLedger-specific additions. Source of truth:
`src/CustomerLedger.Infrastructure/Data/Configurations/*.cs` and
`database/schema/02_CreateTables.sql`.

## Branches

| Column | Type | Nullable | Notes |
|---|---|---|---|
| BranchId | INT | No | PK, identity |
| BranchCode | VARCHAR(20) | No | Unique |
| Name | VARCHAR(150) | No | |
| Email | VARCHAR(256) | Yes | |
| PhoneNumber | VARCHAR(20) | No | |
| Address | VARCHAR(300) | No | |
| City | VARCHAR(100) | No | |
| IsActive | TINYINT(1) | No | Default true |
| CreatedAtUtc | DATETIME(6) | No | |
| UpdatedAtUtc | DATETIME(6) | Yes | |

## AspNetUsers (CustomerLedger additions only)

| Column | Type | Nullable | Notes |
|---|---|---|---|
| FullName | VARCHAR(150) | No | |
| BranchId | INT | Yes | NULL only for Administrators |
| EmployeeCode | VARCHAR(30) | No | Unique |
| IsActive | TINYINT(1) | No | Default true |
| CreatedAtUtc | DATETIME(6) | No | |
| LastLoginAtUtc | DATETIME(6) | Yes | Set by `AccountController.Login` |

## Customers

| Column | Type | Nullable | Notes |
|---|---|---|---|
| CustomerId | INT | No | PK, identity |
| BranchId | INT | No | FK → Branches |
| CustomerCode | VARCHAR(20) | No | Unique |
| FullName | VARCHAR(150) | No | |
| Email | VARCHAR(256) | Yes | |
| PhoneNumber | VARCHAR(20) | No | |
| CNIC | VARCHAR(20) | Yes | Masked in list views |
| Address | VARCHAR(300) | No | |
| City | VARCHAR(100) | No | |
| RegistrationDate | DATETIME(6) | No | |
| Status | VARCHAR(20) | No | `CustomerStatus`: Active / Inactive / Blacklisted |
| IsDeleted | TINYINT(1) | No | Default false — soft delete |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## CustomerAccounts

| Column | Type | Nullable | Notes |
|---|---|---|---|
| CustomerAccountId | INT | No | PK, identity |
| CustomerId | INT | No | FK → Customers, unique (one-to-one) |
| CreditLimit | DECIMAL(18,2) | No | ≥ 0 |
| CurrentBalance | DECIMAL(18,2) | No | = TotalBilled − TotalPaid |
| TotalBilled | DECIMAL(18,2) | No | Sum of Active invoice totals |
| TotalPaid | DECIMAL(18,2) | No | Sum of completed, non-reversed payments |
| AccountStatus | VARCHAR(20) | No | `AccountStatus`: Active / Suspended / Closed |
| ConcurrencyVersion | INT UNSIGNED | No | App-incremented on every mutation |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## Invoices

| Column | Type | Nullable | Notes |
|---|---|---|---|
| InvoiceId | BIGINT | No | PK, identity |
| CustomerId | INT | No | FK → Customers |
| BranchId | INT | No | FK → Branches |
| InvoiceNumber | VARCHAR(30) | No | Unique |
| InvoiceDate | DATETIME(6) | No | |
| DueDate | DATETIME(6) | Yes | Defaulted to +30 days at Activate if unset |
| Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount | DECIMAL(18,2) | No | See [CRUD-Queries.md](CRUD-Queries.md) for the calculation |
| PaymentStatus | VARCHAR(20) | No | Unpaid / PartiallyPaid / Paid |
| InvoiceStatus | VARCHAR(20) | No | Draft / Active / Cancelled |
| CreatedByUserId | VARCHAR(255) | No | FK → AspNetUsers |
| IsDeleted | TINYINT(1) | No | Default false |
| ConcurrencyVersion | INT UNSIGNED | No | App-incremented on every mutation |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## InvoiceItems

| Column | Type | Nullable | Notes |
|---|---|---|---|
| InvoiceItemId | BIGINT | No | PK, identity |
| InvoiceId | BIGINT | No | FK → Invoices (CASCADE delete) |
| Description | VARCHAR(300) | No | |
| Quantity | DECIMAL(18,2) | No | CHECK > 0 |
| UnitPrice | DECIMAL(18,2) | No | CHECK ≥ 0 |
| DiscountAmount, TaxAmount, LineTotal | DECIMAL(18,2) | No | LineTotal = Qty×Price − Discount + Tax |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## Payments

| Column | Type | Nullable | Notes |
|---|---|---|---|
| PaymentId | BIGINT | No | PK, identity |
| InvoiceId | BIGINT | No | FK → Invoices |
| CustomerId | INT | No | FK → Customers |
| BranchId | INT | No | FK → Branches |
| PaymentNumber | VARCHAR(30) | No | Unique |
| PaymentDate | DATETIME(6) | No | |
| Amount | DECIMAL(18,2) | No | CHECK > 0 |
| PaymentMethod | VARCHAR(20) | No | Cash / BankTransfer / Card / MobileWallet / Cheque / Other |
| TransactionReference | VARCHAR(100) | Yes | |
| PaymentStatus | VARCHAR(20) | No | Completed / Reversed |
| ReceivedByUserId | VARCHAR(255) | No | FK → AspNetUsers |
| ReversedPaymentId | BIGINT | Yes | Self-referencing FK → Payments |
| ReversalReason | VARCHAR(500) | Yes | Required by app logic when reversing |
| Notes | VARCHAR(500) | Yes | |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## InstallmentPlans

| Column | Type | Nullable | Notes |
|---|---|---|---|
| InstallmentPlanId | BIGINT | No | PK, identity |
| InvoiceId | BIGINT | No | FK → Invoices, unique (one plan per invoice) |
| NumberOfInstallments | INT | No | CHECK > 0 |
| TotalInstallmentAmount | DECIMAL(18,2) | No | = Outstanding − DownPayment at creation |
| DownPayment | DECIMAL(18,2) | No | |
| StartDate / EndDate | DATETIME(6) | No | CHECK StartDate ≤ EndDate |
| Frequency | VARCHAR(20) | No | Weekly / BiWeekly / Monthly / Quarterly |
| Status | VARCHAR(20) | No | PendingApproval / Active / Completed / Cancelled |
| ApprovedByUserId | VARCHAR(255) | Yes | FK → AspNetUsers |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## InstallmentSchedules

| Column | Type | Nullable | Notes |
|---|---|---|---|
| InstallmentScheduleId | BIGINT | No | PK, identity |
| InstallmentPlanId | BIGINT | No | FK → InstallmentPlans (CASCADE delete) |
| InstallmentNumber | INT | No | Unique with InstallmentPlanId |
| DueDate | DATETIME(6) | No | |
| AmountDue | DECIMAL(18,2) | No | CHECK > 0 |
| AmountPaid | DECIMAL(18,2) | No | CHECK ≥ 0 |
| PaidDate | DATETIME(6) | Yes | Set when AmountPaid ≥ AmountDue |
| Status | VARCHAR(20) | No | Pending / Paid / Overdue / Cancelled |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## CustomerInteractions

| Column | Type | Nullable | Notes |
|---|---|---|---|
| CustomerInteractionId | BIGINT | No | PK, identity |
| CustomerId | INT | No | FK → Customers |
| BranchId | INT | No | FK → Branches |
| InteractionType | VARCHAR(30) | No | PhoneCall / Complaint / PaymentReminder / Email / PhysicalVisit / FollowUp / AccountQuery |
| Subject | VARCHAR(200) | No | |
| Description | VARCHAR(2000) | No | |
| InteractionDate | DATETIME(6) | No | |
| FollowUpDate | DATETIME(6) | Yes | CHECK ≥ InteractionDate when set |
| Status | VARCHAR(30) | No | Open / FollowUpScheduled / Resolved / Closed |
| RecordedByUserId | VARCHAR(255) | No | FK → AspNetUsers |
| CreatedAtUtc / UpdatedAtUtc | DATETIME(6) | No / Yes | |

## AuditLogs

| Column | Type | Nullable | Notes |
|---|---|---|---|
| AuditLogId | BIGINT | No | PK, identity |
| UserId | VARCHAR(450) | Yes | No FK — see [Delete-Behaviors.md](Delete-Behaviors.md) |
| BranchId | INT | Yes | No FK |
| TableName | VARCHAR(100) | No | |
| RecordId | VARCHAR(50) | No | |
| ActionType | VARCHAR(30) | No | Create / Update / Cancel / Reverse / Activate / Reconcile / TriggerAuditInsert / ... |
| OldValuesJson / NewValuesJson | LONGTEXT | Yes | Sensitive fields filtered before serialization |
| IpAddress | VARCHAR(45) | Yes | |
| CorrelationId | VARCHAR(100) | Yes | |
| ReviewStatus | VARCHAR(30) | No | Unreviewed / Reviewed / FlaggedForFollowUp |
| AdminNote | VARCHAR(1000) | Yes | |
| IsArchived | TINYINT(1) | No | Default false |
| CreatedAtUtc | DATETIME(6) | No | |

## BackupHistories

| Column | Type | Nullable | Notes |
|---|---|---|---|
| BackupHistoryId | BIGINT | No | PK, identity |
| BackupType | VARCHAR(20) | No | Full / SchemaOnly / DataOnly |
| FileName / FilePath | VARCHAR(260) / VARCHAR(1000) | No | |
| FileSize | BIGINT | Yes | Set only on Completed |
| Status | VARCHAR(20) | No | InProgress / Completed / Failed |
| StartedAtUtc / CompletedAtUtc | DATETIME(6) | No / Yes | |
| CreatedByUserId | VARCHAR(255) | No | FK → AspNetUsers |
| ErrorMessage | VARCHAR(2000) | Yes | Set only on Failed |
| CreatedAtUtc | DATETIME(6) | No | |
