-- =====================================================================
-- CustomerLedger — PaymentTransaction.sql
-- Mirrors PaymentService.RecordPaymentAsync. FOR UPDATE takes an
-- exclusive row lock on the invoice for the lifetime of the transaction
-- — a concurrent session running the same statements against the same
-- InvoiceId blocks at the SELECT ... FOR UPDATE until this one commits
-- or rolls back, which is what makes two simultaneous payments unable
-- to both read the same OutstandingAmount and jointly overpay it.
-- =====================================================================

USE customerledger;

START TRANSACTION;

SELECT TotalAmount, PaidAmount, OutstandingAmount, InvoiceStatus, CustomerId
INTO @total, @paid, @outstanding, @status, @customer_id
FROM Invoices
WHERE InvoiceId = ?
FOR UPDATE;

-- Application-level guards before proceeding (all abort with ROLLBACK):
--   @status must be 'Active'
--   @outstanding must be > 0
--   the payment amount (@amount) must be <= @outstanding
--   @amount must be > 0

INSERT INTO Payments (InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentStatus, ReceivedByUserId, CreatedAtUtc)
VALUES (?, @customer_id, ?, ?, UTC_TIMESTAMP(6), ?, ?, 'Completed', ?, UTC_TIMESTAMP(6));

UPDATE Invoices
SET PaidAmount = @paid + ?,
    OutstandingAmount = @total - (@paid + ?),
    PaymentStatus = IF(@total - (@paid + ?) <= 0, 'Paid', 'PartiallyPaid'),
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE InvoiceId = ?;

UPDATE CustomerAccounts
SET TotalPaid = TotalPaid + ?,
    CurrentBalance = TotalBilled - (TotalPaid + ?),
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE CustomerId = @customer_id;

COMMIT;
