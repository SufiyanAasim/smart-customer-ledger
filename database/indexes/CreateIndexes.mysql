-- =====================================================================
-- CustomerLedger — CreateIndexes.sql
-- Authoritative list of every index required by spec section 8, with the
-- query pattern each one supports. Most are already created inline as
-- KEY clauses in database/schema/02_CreateTables.sql (and by EF Core's
-- InitialCreate migration) — this script is safe to re-run afterward
-- because it checks information_schema before creating anything, so it
-- can also be used to backfill indexes onto a database that was created
-- purely from application data without ever running the schema scripts.
-- =====================================================================

USE customerledger;

DROP PROCEDURE IF EXISTS sp_add_index_if_missing;

DELIMITER $$
CREATE PROCEDURE sp_add_index_if_missing(
    IN p_table VARCHAR(64),
    IN p_index VARCHAR(64),
    IN p_ddl VARCHAR(1024)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = p_table
          AND index_name = p_index
    ) THEN
        SET @ddl = p_ddl;
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$
DELIMITER ;

-- Branches(BranchCode) — unique login/lookup by branch code.
CALL sp_add_index_if_missing('Branches', 'UQ_Branches_BranchCode',
    'CREATE UNIQUE INDEX UQ_Branches_BranchCode ON Branches (BranchCode)');

-- ApplicationUsers(EmployeeCode) — unique employee lookup.
CALL sp_add_index_if_missing('AspNetUsers', 'UQ_AspNetUsers_EmployeeCode',
    'CREATE UNIQUE INDEX UQ_AspNetUsers_EmployeeCode ON AspNetUsers (EmployeeCode)');

-- ApplicationUsers(BranchId, IsActive) — "active staff for this branch" list screens.
CALL sp_add_index_if_missing('AspNetUsers', 'IX_AspNetUsers_BranchId_IsActive',
    'CREATE INDEX IX_AspNetUsers_BranchId_IsActive ON AspNetUsers (BranchId, IsActive)');

-- Customers(CustomerCode) — unique customer lookup.
CALL sp_add_index_if_missing('Customers', 'UQ_Customers_CustomerCode',
    'CREATE UNIQUE INDEX UQ_Customers_CustomerCode ON Customers (CustomerCode)');

-- Customers(PhoneNumber) — search-by-phone during registration/lookup.
CALL sp_add_index_if_missing('Customers', 'IX_Customers_PhoneNumber',
    'CREATE INDEX IX_Customers_PhoneNumber ON Customers (PhoneNumber)');

-- Customers(CNIC) — search-by-CNIC during registration/lookup.
CALL sp_add_index_if_missing('Customers', 'IX_Customers_CNIC',
    'CREATE INDEX IX_Customers_CNIC ON Customers (CNIC)');

-- Customers(BranchId, Status, IsDeleted) — every customer list screen's WHERE clause.
CALL sp_add_index_if_missing('Customers', 'IX_Customers_BranchId_Status_IsDeleted',
    'CREATE INDEX IX_Customers_BranchId_Status_IsDeleted ON Customers (BranchId, Status, IsDeleted)');

-- CustomerAccounts(CustomerId) — one-to-one account lookup by customer.
CALL sp_add_index_if_missing('CustomerAccounts', 'UQ_CustomerAccounts_CustomerId',
    'CREATE UNIQUE INDEX UQ_CustomerAccounts_CustomerId ON CustomerAccounts (CustomerId)');

-- Invoices(InvoiceNumber) — unique invoice lookup / receipt printing.
CALL sp_add_index_if_missing('Invoices', 'UQ_Invoices_InvoiceNumber',
    'CREATE UNIQUE INDEX UQ_Invoices_InvoiceNumber ON Invoices (InvoiceNumber)');

-- Invoices(CustomerId, PaymentStatus) — customer account statement / outstanding list.
CALL sp_add_index_if_missing('Invoices', 'IX_Invoices_CustomerId_PaymentStatus',
    'CREATE INDEX IX_Invoices_CustomerId_PaymentStatus ON Invoices (CustomerId, PaymentStatus)');

-- Invoices(BranchId, InvoiceDate) — branch revenue reports by date range.
CALL sp_add_index_if_missing('Invoices', 'IX_Invoices_BranchId_InvoiceDate',
    'CREATE INDEX IX_Invoices_BranchId_InvoiceDate ON Invoices (BranchId, InvoiceDate)');

-- Invoices(BranchId, InvoiceStatus, InvoiceDate) — invoice list screen filter+sort.
CALL sp_add_index_if_missing('Invoices', 'IX_Invoices_BranchId_InvoiceStatus_InvoiceDate',
    'CREATE INDEX IX_Invoices_BranchId_InvoiceStatus_InvoiceDate ON Invoices (BranchId, InvoiceStatus, InvoiceDate)');

-- Payments(PaymentNumber) — unique payment lookup / receipt printing.
CALL sp_add_index_if_missing('Payments', 'UQ_Payments_PaymentNumber',
    'CREATE UNIQUE INDEX UQ_Payments_PaymentNumber ON Payments (PaymentNumber)');

-- Payments(InvoiceId, PaymentStatus) — invoice detail's payment history panel.
CALL sp_add_index_if_missing('Payments', 'IX_Payments_InvoiceId_PaymentStatus',
    'CREATE INDEX IX_Payments_InvoiceId_PaymentStatus ON Payments (InvoiceId, PaymentStatus)');

-- Payments(CustomerId, PaymentDate) — customer statement / payment history.
CALL sp_add_index_if_missing('Payments', 'IX_Payments_CustomerId_PaymentDate',
    'CREATE INDEX IX_Payments_CustomerId_PaymentDate ON Payments (CustomerId, PaymentDate)');

-- Payments(BranchId, PaymentDate) — vw_DailyTransactionSummary and daily reports.
CALL sp_add_index_if_missing('Payments', 'IX_Payments_BranchId_PaymentDate',
    'CREATE INDEX IX_Payments_BranchId_PaymentDate ON Payments (BranchId, PaymentDate)');

-- InstallmentSchedules(Status, DueDate) — vw_OverdueInstallments computation.
CALL sp_add_index_if_missing('InstallmentSchedules', 'IX_InstallmentSchedules_Status_DueDate',
    'CREATE INDEX IX_InstallmentSchedules_Status_DueDate ON InstallmentSchedules (Status, DueDate)');

-- CustomerInteractions(CustomerId, InteractionDate) — customer interaction history.
CALL sp_add_index_if_missing('CustomerInteractions', 'IX_CustomerInteractions_CustomerId_InteractionDate',
    'CREATE INDEX IX_CustomerInteractions_CustomerId_InteractionDate ON CustomerInteractions (CustomerId, InteractionDate)');

-- AuditLogs(TableName, RecordId) — "show audit trail for this record" lookup.
CALL sp_add_index_if_missing('AuditLogs', 'IX_AuditLogs_TableName_RecordId',
    'CREATE INDEX IX_AuditLogs_TableName_RecordId ON AuditLogs (TableName, RecordId)');

-- AuditLogs(BranchId, CreatedAtUtc) — admin audit log review screen.
CALL sp_add_index_if_missing('AuditLogs', 'IX_AuditLogs_BranchId_CreatedAtUtc',
    'CREATE INDEX IX_AuditLogs_BranchId_CreatedAtUtc ON AuditLogs (BranchId, CreatedAtUtc)');

DROP PROCEDURE IF EXISTS sp_add_index_if_missing;
