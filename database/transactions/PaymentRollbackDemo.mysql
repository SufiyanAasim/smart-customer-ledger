-- =====================================================================
-- CustomerLedger — PaymentRollbackDemo.sql
-- Demonstrates Atomicity and Durability: a forced ROLLBACK leaves the
-- invoice exactly as it was before the transaction started — nothing
-- partially applied. Run each numbered block in order and compare the
-- "before" and "after rollback" query results (they must be identical).
-- =====================================================================

USE customerledger;

-- 1. BEFORE: snapshot the invoice's current state.
SELECT InvoiceId, PaidAmount, OutstandingAmount, PaymentStatus, ConcurrencyVersion
FROM Invoices
WHERE InvoiceId = ?;

-- 2. Start a transaction, apply a payment, but deliberately roll back
--    instead of committing (simulating a mid-transaction failure —
--    e.g. the CustomerAccounts UPDATE below throwing).
START TRANSACTION;

UPDATE Invoices
SET PaidAmount = PaidAmount + 500,
    OutstandingAmount = OutstandingAmount - 500,
    PaymentStatus = 'PartiallyPaid',
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE InvoiceId = ?;

-- Imagine the equivalent CustomerAccounts UPDATE step failing here
-- (constraint violation, deadlock, application exception, etc.).

ROLLBACK;

-- 3. AFTER ROLLBACK: must match step 1 exactly — PaidAmount,
--    OutstandingAmount, PaymentStatus, and ConcurrencyVersion unchanged.
SELECT InvoiceId, PaidAmount, OutstandingAmount, PaymentStatus, ConcurrencyVersion
FROM Invoices
WHERE InvoiceId = ?;

-- 4. Confirm no orphaned Payments row was left behind either — count
--    should be identical before and after this whole script.
SELECT COUNT(*) AS payment_count FROM Payments WHERE InvoiceId = ?;
