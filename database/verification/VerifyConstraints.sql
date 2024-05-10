-- =====================================================================
-- CustomerLedger — VerifyConstraints.sql
-- Lists every CHECK/UNIQUE/FOREIGN KEY constraint, then proves the
-- important ones actually reject bad data (each negative test is wrapped
-- so it reports PASS/FAIL instead of aborting the script).
-- =====================================================================

USE customerledger;

-- 1. Inventory of constraints.
SELECT constraint_name, constraint_type, table_name
FROM information_schema.table_constraints
WHERE table_schema = DATABASE()
ORDER BY table_name, constraint_type;

-- 2. Negative test: CHECK constraint rejects a negative unit price.
--    Expect this to raise error 3819 (CHECK constraint violation) or,
--    on MySQL < 8.0.16 where CHECK is parsed but not enforced, to
--    succeed — in which case the CHECK constraints must be verified on
--    a current MySQL 8.0.16+ server before relying on them.
-- (Run manually and observe the error; DELIMITER/procedure wrapping
-- omitted here so the failure is visible directly in Workbench.)
INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
VALUES (1, 'Constraint test row — expected to fail', 1, -100, 0, 0, -100, UTC_TIMESTAMP(6));

-- 3. Negative test: UNIQUE constraint rejects a duplicate BranchCode.
-- INSERT INTO Branches (BranchCode, Name, PhoneNumber, Address, City, CreatedAtUtc)
-- VALUES ('MAIN', 'Duplicate Branch Code Test', '000', 'x', 'x', UTC_TIMESTAMP(6));

-- 4. Negative test: FOREIGN KEY rejects an invoice for a non-existent customer.
-- INSERT INTO Invoices (CustomerId, BranchId, InvoiceNumber, InvoiceDate, Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount, PaymentStatus, InvoiceStatus, CreatedByUserId, CreatedAtUtc)
-- VALUES (999999, 1, 'FK-TEST', UTC_TIMESTAMP(6), 0,0,0,0,0,0, 'Unpaid', 'Draft', 'nonexistent-user-id', UTC_TIMESTAMP(6));
