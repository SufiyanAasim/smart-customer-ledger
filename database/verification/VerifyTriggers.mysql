-- =====================================================================
-- CustomerLedger — VerifyTriggers.sql
-- Confirms all triggers exist, then demonstrates each firing (a mix of
-- expected failures and expected audit-log side effects).
-- =====================================================================

USE customerledger;

-- 1. Inventory of triggers.
SELECT trigger_name, event_manipulation, event_object_table, action_timing
FROM information_schema.triggers
WHERE trigger_schema = DATABASE()
ORDER BY event_object_table, action_timing, event_manipulation;

-- 2. trg_InvoiceItems_BeforeInsert — expect SIGNAL 45000 (quantity <= 0).
INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
VALUES (1, 'Trigger test — should be rejected', 0, 100, 0, 0, 0, UTC_TIMESTAMP(6));

-- 3. trg_Payments_BeforeInsert — expect SIGNAL 45000 when InvoiceId
--    references a Cancelled invoice. Substitute a real cancelled
--    invoice id from your dataset before running.
-- INSERT INTO Payments (InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentStatus, ReceivedByUserId, CreatedAtUtc)
-- VALUES (<cancelled_invoice_id>, 1, 1, 'TRG-TEST-1', UTC_TIMESTAMP(6), 100, 'Cash', 'Completed', '<user-id>', UTC_TIMESTAMP(6));

-- 4. trg_Payments_AfterInsert_Audit — after inserting a valid payment,
--    confirm a matching AuditLogs row with ActionType = 'TriggerAuditInsert'
--    was created automatically.
SELECT * FROM AuditLogs WHERE ActionType = 'TriggerAuditInsert' ORDER BY CreatedAtUtc DESC LIMIT 5;

-- 5. trg_Customers_BeforeDelete / trg_Invoices_BeforeDelete /
--    trg_Payments_BeforeDelete — expect SIGNAL 45000 on every attempt.
-- DELETE FROM Customers WHERE CustomerId = 1;
-- DELETE FROM Invoices WHERE InvoiceId = 1;
-- DELETE FROM Payments WHERE PaymentId = 1;
