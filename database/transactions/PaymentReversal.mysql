-- =====================================================================
-- CustomerLedger — PaymentReversal.sql
-- Mirrors PaymentService.ReverseAsync. The original payment row is
-- never deleted — it is marked Reversed and a second, linked row
-- records the reversal, so the full history remains traceable.
-- =====================================================================

USE customerledger;

START TRANSACTION;

SELECT InvoiceId, CustomerId, BranchId, Amount, PaymentMethod, PaymentNumber, PaymentStatus
INTO @invoice_id, @customer_id, @branch_id, @amount, @method, @payment_number, @status
FROM Payments
WHERE PaymentId = ?
FOR UPDATE;

-- Application aborts here (ROLLBACK) if @status <> 'Completed', or if a
-- prior reversal already exists (SELECT 1 FROM Payments WHERE ReversedPaymentId = ?).

UPDATE Payments
SET PaymentStatus = 'Reversed',
    ReversalReason = ?,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE PaymentId = ?;

INSERT INTO Payments (InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentStatus, ReceivedByUserId, ReversedPaymentId, ReversalReason, CreatedAtUtc)
VALUES (@invoice_id, @customer_id, @branch_id, CONCAT(@payment_number, '-REV'), UTC_TIMESTAMP(6), @amount, @method, 'Reversed', ?, ?, ?, UTC_TIMESTAMP(6));

UPDATE Invoices
SET PaidAmount = PaidAmount - @amount,
    OutstandingAmount = TotalAmount - (PaidAmount - @amount),
    PaymentStatus = IF(PaidAmount - @amount <= 0, 'Unpaid', 'PartiallyPaid'),
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE InvoiceId = @invoice_id;

UPDATE CustomerAccounts
SET TotalPaid = TotalPaid - @amount,
    CurrentBalance = TotalBilled - (TotalPaid - @amount),
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE CustomerId = @customer_id;

COMMIT;
