-- =====================================================================
-- CustomerLedger — VerifyIndexes.sql
-- Confirms every index from CreateIndexes.sql exists, and demonstrates
-- with EXPLAIN that representative reporting queries actually use them
-- rather than falling back to a full table scan.
-- =====================================================================

USE customerledger;

-- 1. List every index on every CustomerLedger table.
SELECT table_name, index_name, GROUP_CONCAT(column_name ORDER BY seq_in_index) AS columns_in_index
FROM information_schema.statistics
WHERE table_schema = DATABASE()
GROUP BY table_name, index_name
ORDER BY table_name, index_name;

-- 2. EXPLAIN: branch invoice list filtered by status and sorted by date —
--    should use IX_Invoices_BranchId_InvoiceStatus_InvoiceDate, not a full scan.
EXPLAIN
SELECT InvoiceId, InvoiceNumber, TotalAmount, OutstandingAmount
FROM Invoices
WHERE BranchId = 1 AND InvoiceStatus = 'Active'
ORDER BY InvoiceDate DESC
LIMIT 15;

-- 3. EXPLAIN: overdue installment computation — should use
--    IX_InstallmentSchedules_Status_DueDate.
EXPLAIN
SELECT InstallmentScheduleId, DueDate, AmountDue
FROM InstallmentSchedules
WHERE Status = 'Pending' AND DueDate < UTC_TIMESTAMP();

-- 4. EXPLAIN: customer statement — should use IX_Payments_CustomerId_PaymentDate.
EXPLAIN
SELECT PaymentId, PaymentDate, Amount
FROM Payments
WHERE CustomerId = 1
ORDER BY PaymentDate DESC;
