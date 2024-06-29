-- =====================================================================
-- CustomerLedger — VerifyViews.sql
-- Confirms all six views exist, runs an example SELECT against each with
-- expected column output, and EXPLAINs the two most query-heavy ones.
-- =====================================================================

USE customerledger;

-- 1. Confirm all six views exist.
SELECT table_name AS view_name
FROM information_schema.views
WHERE table_schema = DATABASE()
ORDER BY view_name;
-- Expected rows: vw_BranchRevenueSummary, vw_CustomerAccountSummary,
-- vw_CustomerInteractionHistory, vw_DailyTransactionSummary,
-- vw_InvoicePaymentStatus, vw_OverdueInstallments

-- 2. vw_CustomerAccountSummary
-- Expected columns: CustomerId, CustomerCode, CustomerName, BranchId,
-- BranchName, TotalInvoices, TotalBilled, TotalPaid, OutstandingBalance,
-- CreditLimit, AccountStatus
SELECT * FROM vw_CustomerAccountSummary LIMIT 10;

-- 3. vw_InvoicePaymentStatus
SELECT * FROM vw_InvoicePaymentStatus WHERE PaymentStatus <> 'Paid' LIMIT 10;

-- 4. vw_OverdueInstallments
SELECT * FROM vw_OverdueInstallments ORDER BY DaysOverdue DESC LIMIT 10;

-- 5. vw_BranchRevenueSummary
SELECT * FROM vw_BranchRevenueSummary ORDER BY TotalBilled DESC;

-- 6. vw_CustomerInteractionHistory
SELECT * FROM vw_CustomerInteractionHistory ORDER BY InteractionDate DESC LIMIT 10;

-- 7. vw_DailyTransactionSummary
SELECT * FROM vw_DailyTransactionSummary ORDER BY TransactionDate DESC LIMIT 10;

-- 8. EXPLAIN: the view is expanded into its underlying joins — a normal
--    MySQL view has no index of its own, so this confirms the
--    UNDERLYING table indexes (IX_Invoices_CustomerId_PaymentStatus etc.)
--    are what keep this view's queries fast, not the view itself.
EXPLAIN SELECT * FROM vw_InvoicePaymentStatus WHERE PaymentStatus = 'PartiallyPaid';
EXPLAIN SELECT * FROM vw_OverdueInstallments;
