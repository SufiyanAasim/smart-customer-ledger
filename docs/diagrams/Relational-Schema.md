# Relational Schema

The physical column-level schema, matching `database/schema/02_CreateTables.sql` and the EF
Core InitialCreate migration exactly (table/column names, types, nullability).

```
Branches
├─ BranchId            INT PK AUTO_INCREMENT
├─ BranchCode           VARCHAR(20)  UNIQUE NOT NULL
├─ Name                 VARCHAR(150) NOT NULL
├─ Email                VARCHAR(256) NULL
├─ PhoneNumber          VARCHAR(20)  NOT NULL
├─ Address              VARCHAR(300) NOT NULL
├─ City                 VARCHAR(100) NOT NULL
├─ IsActive             TINYINT(1) DEFAULT 1
├─ CreatedAtUtc         DATETIME(6) NOT NULL
└─ UpdatedAtUtc         DATETIME(6) NULL

AspNetUsers (ASP.NET Core Identity + CustomerLedger extensions)
├─ Id                   VARCHAR(255) PK
├─ FullName             VARCHAR(150) NOT NULL            ← CustomerLedger addition
├─ BranchId             INT NULL FK → Branches            ← CustomerLedger addition
├─ EmployeeCode         VARCHAR(30) UNIQUE NOT NULL       ← CustomerLedger addition
├─ IsActive             TINYINT(1) DEFAULT 1              ← CustomerLedger addition
├─ CreatedAtUtc         DATETIME(6) NOT NULL              ← CustomerLedger addition
├─ LastLoginAtUtc       DATETIME(6) NULL                  ← CustomerLedger addition
└─ ... (UserName, Email, PasswordHash, etc. — standard Identity columns)

Customers
├─ CustomerId           INT PK AUTO_INCREMENT
├─ BranchId             INT NOT NULL FK → Branches
├─ CustomerCode         VARCHAR(20)  UNIQUE NOT NULL
├─ FullName             VARCHAR(150) NOT NULL
├─ Email                VARCHAR(256) NULL
├─ PhoneNumber          VARCHAR(20)  NOT NULL
├─ CNIC                 VARCHAR(20)  NULL
├─ Address              VARCHAR(300) NOT NULL
├─ City                 VARCHAR(100) NOT NULL
├─ RegistrationDate     DATETIME(6) NOT NULL
├─ Status               VARCHAR(20) NOT NULL   -- CustomerStatus enum-as-string
├─ IsDeleted            TINYINT(1) DEFAULT 0
├─ CreatedAtUtc         DATETIME(6) NOT NULL
└─ UpdatedAtUtc         DATETIME(6) NULL

CustomerAccounts
├─ CustomerAccountId    INT PK AUTO_INCREMENT
├─ CustomerId           INT UNIQUE NOT NULL FK → Customers
├─ CreditLimit          DECIMAL(18,2) NOT NULL
├─ CurrentBalance       DECIMAL(18,2) NOT NULL
├─ TotalBilled          DECIMAL(18,2) NOT NULL
├─ TotalPaid            DECIMAL(18,2) NOT NULL
├─ AccountStatus        VARCHAR(20) NOT NULL   -- AccountStatus enum-as-string
├─ CreatedAtUtc         DATETIME(6) NOT NULL
├─ UpdatedAtUtc         DATETIME(6) NULL
└─ ConcurrencyVersion   INT UNSIGNED NOT NULL  -- optimistic concurrency token

Invoices
├─ InvoiceId            BIGINT PK AUTO_INCREMENT
├─ CustomerId           INT NOT NULL FK → Customers
├─ BranchId             INT NOT NULL FK → Branches
├─ InvoiceNumber        VARCHAR(30) UNIQUE NOT NULL
├─ InvoiceDate          DATETIME(6) NOT NULL
├─ DueDate              DATETIME(6) NULL
├─ Subtotal/DiscountAmount/TaxAmount/TotalAmount/PaidAmount/OutstandingAmount  DECIMAL(18,2)
├─ PaymentStatus        VARCHAR(20) NOT NULL
├─ InvoiceStatus        VARCHAR(20) NOT NULL
├─ CreatedByUserId      VARCHAR(255) NOT NULL FK → AspNetUsers
├─ IsDeleted            TINYINT(1) DEFAULT 0
├─ CreatedAtUtc / UpdatedAtUtc
└─ ConcurrencyVersion   INT UNSIGNED NOT NULL

InvoiceItems
├─ InvoiceItemId        BIGINT PK AUTO_INCREMENT
├─ InvoiceId            BIGINT NOT NULL FK → Invoices (ON DELETE CASCADE)
├─ Description           VARCHAR(300) NOT NULL
├─ Quantity/UnitPrice/DiscountAmount/TaxAmount/LineTotal  DECIMAL(18,2)
└─ CreatedAtUtc / UpdatedAtUtc

Payments
├─ PaymentId            BIGINT PK AUTO_INCREMENT
├─ InvoiceId            BIGINT NOT NULL FK → Invoices
├─ CustomerId           INT NOT NULL FK → Customers
├─ BranchId             INT NOT NULL FK → Branches
├─ PaymentNumber        VARCHAR(30) UNIQUE NOT NULL
├─ PaymentDate / Amount / PaymentMethod / TransactionReference
├─ PaymentStatus        VARCHAR(20) NOT NULL
├─ ReceivedByUserId     VARCHAR(255) NOT NULL FK → AspNetUsers
├─ ReversedPaymentId    BIGINT NULL FK → Payments (self-referencing)
├─ ReversalReason / Notes
└─ CreatedAtUtc / UpdatedAtUtc

InstallmentPlans
├─ InstallmentPlanId    BIGINT PK AUTO_INCREMENT
├─ InvoiceId            BIGINT UNIQUE NOT NULL FK → Invoices
├─ NumberOfInstallments / TotalInstallmentAmount / DownPayment
├─ StartDate / EndDate / Frequency / Status
├─ ApprovedByUserId     VARCHAR(255) NULL FK → AspNetUsers
└─ CreatedAtUtc / UpdatedAtUtc

InstallmentSchedules
├─ InstallmentScheduleId BIGINT PK AUTO_INCREMENT
├─ InstallmentPlanId    BIGINT NOT NULL FK → InstallmentPlans (ON DELETE CASCADE)
├─ InstallmentNumber    INT NOT NULL         -- UNIQUE with InstallmentPlanId
├─ DueDate / AmountDue / AmountPaid / PaidDate / Status
└─ CreatedAtUtc / UpdatedAtUtc

CustomerInteractions
├─ CustomerInteractionId BIGINT PK AUTO_INCREMENT
├─ CustomerId           INT NOT NULL FK → Customers
├─ BranchId             INT NOT NULL FK → Branches
├─ InteractionType / Subject / Description / InteractionDate / FollowUpDate / Status
├─ RecordedByUserId     VARCHAR(255) NOT NULL FK → AspNetUsers
└─ CreatedAtUtc / UpdatedAtUtc

AuditLogs
├─ AuditLogId           BIGINT PK AUTO_INCREMENT
├─ UserId / BranchId    (no FK — see ER-Diagram.md)
├─ TableName / RecordId / ActionType
├─ OldValuesJson / NewValuesJson  LONGTEXT
├─ IpAddress / CorrelationId
├─ ReviewStatus / AdminNote / IsArchived
└─ CreatedAtUtc

BackupHistories
├─ BackupHistoryId      BIGINT PK AUTO_INCREMENT
├─ BackupType / FileName / FilePath / FileSize / Status
├─ StartedAtUtc / CompletedAtUtc
├─ CreatedByUserId      VARCHAR(255) NOT NULL FK → AspNetUsers
└─ ErrorMessage / CreatedAtUtc
```

Full column-by-column detail with types, defaults, and constraints:
[docs/database/Tables-and-Columns.md](../database/Tables-and-Columns.md).
