-- =====================================================================
-- CustomerLedger — 02_CreateTables.sql
-- Full table DDL mirroring the EF Core InitialCreate migration
-- (src/CustomerLedger.Infrastructure/Data/Migrations/*_InitialCreate.cs).
--
-- Provided for MySQL Workbench walkthroughs and production review. In normal
-- development/deployment, run:
--   dotnet ef database update --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
-- which creates these same tables (including the AspNetUsers/AspNetRoles
-- ASP.NET Core Identity tables) from the C# model. Running both against
-- the same database is redundant — use this script only against a
-- database EF has not yet touched, for manual demonstration purposes.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- ASP.NET Core Identity tables (columns per Microsoft.AspNetCore.Identity
-- defaults). Only the columns CustomerLedger's ApplicationUser adds
-- (FullName, BranchId, EmployeeCode, IsActive, CreatedAtUtc, LastLoginAtUtc)
-- are CustomerLedger-specific; the rest are framework-managed.
-- ---------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id VARCHAR(255) NOT NULL,
    Name VARCHAR(256) NULL,
    NormalizedName VARCHAR(256) NULL,
    ConcurrencyStamp LONGTEXT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Branches (
    BranchId INT NOT NULL AUTO_INCREMENT,
    BranchCode VARCHAR(20) NOT NULL,
    Name VARCHAR(150) NOT NULL,
    Email VARCHAR(256) NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    Address VARCHAR(300) NOT NULL,
    City VARCHAR(100) NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    PRIMARY KEY (BranchId),
    UNIQUE KEY UQ_Branches_BranchCode (BranchCode)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id VARCHAR(255) NOT NULL,
    FullName VARCHAR(150) NOT NULL,
    BranchId INT NULL,
    EmployeeCode VARCHAR(30) NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAtUtc DATETIME(6) NOT NULL,
    LastLoginAtUtc DATETIME(6) NULL,
    UserName VARCHAR(256) NULL,
    NormalizedUserName VARCHAR(256) NULL,
    Email VARCHAR(256) NULL,
    NormalizedEmail VARCHAR(256) NULL,
    EmailConfirmed TINYINT(1) NOT NULL,
    PasswordHash LONGTEXT NULL,
    SecurityStamp LONGTEXT NULL,
    ConcurrencyStamp LONGTEXT NULL,
    PhoneNumber LONGTEXT NULL,
    PhoneNumberConfirmed TINYINT(1) NOT NULL,
    TwoFactorEnabled TINYINT(1) NOT NULL,
    LockoutEnd DATETIME(6) NULL,
    LockoutEnabled TINYINT(1) NOT NULL,
    AccessFailedCount INT NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY UQ_AspNetUsers_EmployeeCode (EmployeeCode),
    KEY IX_AspNetUsers_BranchId_IsActive (BranchId, IsActive),
    CONSTRAINT FK_AspNetUsers_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES Branches (BranchId) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
    Id INT NOT NULL AUTO_INCREMENT,
    RoleId VARCHAR(255) NOT NULL,
    ClaimType LONGTEXT NULL,
    ClaimValue LONGTEXT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AspNetUserClaims (
    Id INT NOT NULL AUTO_INCREMENT,
    UserId VARCHAR(255) NOT NULL,
    ClaimType LONGTEXT NULL,
    ClaimValue LONGTEXT NULL,
    PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AspNetUserLogins (
    LoginProvider VARCHAR(255) NOT NULL,
    ProviderKey VARCHAR(255) NOT NULL,
    ProviderDisplayName LONGTEXT NULL,
    UserId VARCHAR(255) NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId VARCHAR(255) NOT NULL,
    RoleId VARCHAR(255) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AspNetUserTokens (
    UserId VARCHAR(255) NOT NULL,
    LoginProvider VARCHAR(255) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Value LONGTEXT NULL,
    PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Business tables
-- ---------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS Customers (
    CustomerId INT NOT NULL AUTO_INCREMENT,
    BranchId INT NOT NULL,
    CustomerCode VARCHAR(20) NOT NULL,
    FullName VARCHAR(150) NOT NULL,
    Email VARCHAR(256) NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    CNIC VARCHAR(20) NULL,
    Address VARCHAR(300) NOT NULL,
    City VARCHAR(100) NOT NULL,
    RegistrationDate DATETIME(6) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    PRIMARY KEY (CustomerId),
    UNIQUE KEY UQ_Customers_CustomerCode (CustomerCode),
    KEY IX_Customers_PhoneNumber (PhoneNumber),
    KEY IX_Customers_CNIC (CNIC),
    KEY IX_Customers_BranchId_Status_IsDeleted (BranchId, Status, IsDeleted),
    CONSTRAINT FK_Customers_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES Branches (BranchId) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS CustomerAccounts (
    CustomerAccountId INT NOT NULL AUTO_INCREMENT,
    CustomerId INT NOT NULL,
    CreditLimit DECIMAL(18,2) NOT NULL,
    CurrentBalance DECIMAL(18,2) NOT NULL,
    TotalBilled DECIMAL(18,2) NOT NULL,
    TotalPaid DECIMAL(18,2) NOT NULL,
    AccountStatus VARCHAR(20) NOT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    ConcurrencyVersion INT UNSIGNED NOT NULL,
    PRIMARY KEY (CustomerAccountId),
    UNIQUE KEY UQ_CustomerAccounts_CustomerId (CustomerId),
    CONSTRAINT FK_CustomerAccounts_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers (CustomerId) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Invoices (
    InvoiceId BIGINT NOT NULL AUTO_INCREMENT,
    CustomerId INT NOT NULL,
    BranchId INT NOT NULL,
    InvoiceNumber VARCHAR(30) NOT NULL,
    InvoiceDate DATETIME(6) NOT NULL,
    DueDate DATETIME(6) NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) NOT NULL,
    OutstandingAmount DECIMAL(18,2) NOT NULL,
    PaymentStatus VARCHAR(20) NOT NULL,
    InvoiceStatus VARCHAR(20) NOT NULL,
    CreatedByUserId VARCHAR(255) NOT NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    ConcurrencyVersion INT UNSIGNED NOT NULL,
    PRIMARY KEY (InvoiceId),
    UNIQUE KEY UQ_Invoices_InvoiceNumber (InvoiceNumber),
    KEY IX_Invoices_CustomerId_PaymentStatus (CustomerId, PaymentStatus),
    KEY IX_Invoices_BranchId_InvoiceDate (BranchId, InvoiceDate),
    KEY IX_Invoices_BranchId_InvoiceStatus_InvoiceDate (BranchId, InvoiceStatus, InvoiceDate),
    CONSTRAINT FK_Invoices_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers (CustomerId) ON DELETE RESTRICT,
    CONSTRAINT FK_Invoices_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES Branches (BranchId) ON DELETE RESTRICT,
    CONSTRAINT FK_Invoices_AspNetUsers_CreatedByUserId FOREIGN KEY (CreatedByUserId) REFERENCES AspNetUsers (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS InvoiceItems (
    InvoiceItemId BIGINT NOT NULL AUTO_INCREMENT,
    InvoiceId BIGINT NOT NULL,
    Description VARCHAR(300) NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    PRIMARY KEY (InvoiceItemId),
    KEY IX_InvoiceItems_InvoiceId (InvoiceId),
    CONSTRAINT FK_InvoiceItems_Invoices_InvoiceId FOREIGN KEY (InvoiceId) REFERENCES Invoices (InvoiceId) ON DELETE CASCADE,
    CONSTRAINT CK_InvoiceItems_Quantity_Positive CHECK (Quantity > 0),
    CONSTRAINT CK_InvoiceItems_UnitPrice_NonNegative CHECK (UnitPrice >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS Payments (
    PaymentId BIGINT NOT NULL AUTO_INCREMENT,
    InvoiceId BIGINT NOT NULL,
    CustomerId INT NOT NULL,
    BranchId INT NOT NULL,
    PaymentNumber VARCHAR(30) NOT NULL,
    PaymentDate DATETIME(6) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod VARCHAR(20) NOT NULL,
    TransactionReference VARCHAR(100) NULL,
    PaymentStatus VARCHAR(20) NOT NULL,
    ReceivedByUserId VARCHAR(255) NOT NULL,
    ReversedPaymentId BIGINT NULL,
    ReversalReason VARCHAR(500) NULL,
    Notes VARCHAR(500) NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    PRIMARY KEY (PaymentId),
    UNIQUE KEY UQ_Payments_PaymentNumber (PaymentNumber),
    KEY IX_Payments_InvoiceId_PaymentStatus (InvoiceId, PaymentStatus),
    KEY IX_Payments_CustomerId_PaymentDate (CustomerId, PaymentDate),
    KEY IX_Payments_BranchId_PaymentDate (BranchId, PaymentDate),
    CONSTRAINT FK_Payments_Invoices_InvoiceId FOREIGN KEY (InvoiceId) REFERENCES Invoices (InvoiceId) ON DELETE RESTRICT,
    CONSTRAINT FK_Payments_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers (CustomerId) ON DELETE RESTRICT,
    CONSTRAINT FK_Payments_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES Branches (BranchId) ON DELETE RESTRICT,
    CONSTRAINT FK_Payments_AspNetUsers_ReceivedByUserId FOREIGN KEY (ReceivedByUserId) REFERENCES AspNetUsers (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Payments_Payments_ReversedPaymentId FOREIGN KEY (ReversedPaymentId) REFERENCES Payments (PaymentId) ON DELETE RESTRICT,
    CONSTRAINT CK_Payments_Amount_Positive CHECK (Amount > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS InstallmentPlans (
    InstallmentPlanId BIGINT NOT NULL AUTO_INCREMENT,
    InvoiceId BIGINT NOT NULL,
    NumberOfInstallments INT NOT NULL,
    TotalInstallmentAmount DECIMAL(18,2) NOT NULL,
    DownPayment DECIMAL(18,2) NOT NULL,
    StartDate DATETIME(6) NOT NULL,
    EndDate DATETIME(6) NOT NULL,
    Frequency VARCHAR(20) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    ApprovedByUserId VARCHAR(255) NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    PRIMARY KEY (InstallmentPlanId),
    UNIQUE KEY UQ_InstallmentPlans_InvoiceId (InvoiceId),
    CONSTRAINT FK_InstallmentPlans_Invoices_InvoiceId FOREIGN KEY (InvoiceId) REFERENCES Invoices (InvoiceId) ON DELETE RESTRICT,
    CONSTRAINT FK_InstallmentPlans_AspNetUsers_ApprovedByUserId FOREIGN KEY (ApprovedByUserId) REFERENCES AspNetUsers (Id) ON DELETE RESTRICT,
    CONSTRAINT CK_InstallmentPlans_NumberOfInstallments_Positive CHECK (NumberOfInstallments > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS InstallmentSchedules (
    InstallmentScheduleId BIGINT NOT NULL AUTO_INCREMENT,
    InstallmentPlanId BIGINT NOT NULL,
    InstallmentNumber INT NOT NULL,
    DueDate DATETIME(6) NOT NULL,
    AmountDue DECIMAL(18,2) NOT NULL,
    AmountPaid DECIMAL(18,2) NOT NULL,
    PaidDate DATETIME(6) NULL,
    Status VARCHAR(20) NOT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    PRIMARY KEY (InstallmentScheduleId),
    UNIQUE KEY UQ_InstallmentSchedules_Plan_Number (InstallmentPlanId, InstallmentNumber),
    KEY IX_InstallmentSchedules_Status_DueDate (Status, DueDate),
    CONSTRAINT FK_InstallmentSchedules_InstallmentPlans_InstallmentPlanId FOREIGN KEY (InstallmentPlanId) REFERENCES InstallmentPlans (InstallmentPlanId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS CustomerInteractions (
    CustomerInteractionId BIGINT NOT NULL AUTO_INCREMENT,
    CustomerId INT NOT NULL,
    BranchId INT NOT NULL,
    InteractionType VARCHAR(30) NOT NULL,
    Subject VARCHAR(200) NOT NULL,
    Description VARCHAR(2000) NOT NULL,
    InteractionDate DATETIME(6) NOT NULL,
    FollowUpDate DATETIME(6) NULL,
    Status VARCHAR(30) NOT NULL,
    RecordedByUserId VARCHAR(255) NOT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    UpdatedAtUtc DATETIME(6) NULL,
    PRIMARY KEY (CustomerInteractionId),
    KEY IX_CustomerInteractions_CustomerId_InteractionDate (CustomerId, InteractionDate),
    CONSTRAINT FK_CustomerInteractions_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers (CustomerId) ON DELETE RESTRICT,
    CONSTRAINT FK_CustomerInteractions_Branches_BranchId FOREIGN KEY (BranchId) REFERENCES Branches (BranchId) ON DELETE RESTRICT,
    CONSTRAINT FK_CustomerInteractions_AspNetUsers_RecordedByUserId FOREIGN KEY (RecordedByUserId) REFERENCES AspNetUsers (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS AuditLogs (
    AuditLogId BIGINT NOT NULL AUTO_INCREMENT,
    UserId VARCHAR(450) NULL,
    BranchId INT NULL,
    TableName VARCHAR(100) NOT NULL,
    RecordId VARCHAR(50) NOT NULL,
    ActionType VARCHAR(30) NOT NULL,
    OldValuesJson LONGTEXT NULL,
    NewValuesJson LONGTEXT NULL,
    IpAddress VARCHAR(45) NULL,
    CorrelationId VARCHAR(100) NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    ReviewStatus VARCHAR(30) NOT NULL,
    AdminNote VARCHAR(1000) NULL,
    IsArchived TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (AuditLogId),
    KEY IX_AuditLogs_TableName_RecordId (TableName, RecordId),
    KEY IX_AuditLogs_BranchId_CreatedAtUtc (BranchId, CreatedAtUtc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS BackupHistories (
    BackupHistoryId BIGINT NOT NULL AUTO_INCREMENT,
    BackupType VARCHAR(20) NOT NULL,
    FileName VARCHAR(260) NOT NULL,
    FilePath VARCHAR(1000) NOT NULL,
    FileSize BIGINT NULL,
    Status VARCHAR(20) NOT NULL,
    StartedAtUtc DATETIME(6) NOT NULL,
    CompletedAtUtc DATETIME(6) NULL,
    CreatedByUserId VARCHAR(255) NOT NULL,
    ErrorMessage VARCHAR(2000) NULL,
    CreatedAtUtc DATETIME(6) NOT NULL,
    PRIMARY KEY (BackupHistoryId),
    CONSTRAINT FK_BackupHistories_AspNetUsers_CreatedByUserId FOREIGN KEY (CreatedByUserId) REFERENCES AspNetUsers (Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
