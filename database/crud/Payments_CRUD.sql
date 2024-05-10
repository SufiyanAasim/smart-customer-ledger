-- =====================================================================
-- CustomerLedger — Payments_CRUD.sql
-- Completed payments are never physically deleted — see the Reverse
-- statement below, which creates a linked second row instead.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT + invoice balance sync, as one transaction (mirrors
-- PaymentService.RecordPaymentAsync). Guarded by the outstanding-balance
-- check so a payment can never push OutstandingAmount negative.
-- ---------------------------------------------------------------------
START TRANSACTION;

SELECT OutstandingAmount, TotalAmount INTO @outstanding, @total
FROM Invoices
WHERE InvoiceId = ? AND InvoiceStatus = 'Active'
FOR UPDATE;

-- Application code aborts here (ROLLBACK) if @outstanding is NULL
-- (invoice not found / not active) or the payment amount exceeds it.

INSERT INTO Payments (
    InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate, Amount,
    PaymentMethod, TransactionReference, PaymentStatus, ReceivedByUserId, Notes, CreatedAtUtc
) VALUES (
    ?, ?, ?, ?, UTC_TIMESTAMP(6), ?,
    ?, ?, 'Completed', ?, ?, UTC_TIMESTAMP(6)
);

UPDATE Invoices
SET PaidAmount = PaidAmount + ?,
    OutstandingAmount = TotalAmount - (PaidAmount + ?),
    PaymentStatus = IF(TotalAmount - (PaidAmount + ?) <= 0, 'Paid', 'PartiallyPaid'),
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE InvoiceId = ?;

COMMIT;
-- ROLLBACK instead of COMMIT on any validation failure. The full worked
-- transaction/rollback/concurrency demonstration ships with v2.0.0 — Balance,
-- at database/transactions/PaymentTransaction.sql.

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT PaymentId, InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate,
       Amount, PaymentMethod, TransactionReference, PaymentStatus, ReversedPaymentId
FROM Payments
WHERE PaymentId = ?;

-- ---------------------------------------------------------------------
-- SELECT list with search/filter/pagination
-- ---------------------------------------------------------------------
SELECT PaymentId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentStatus
FROM Payments
WHERE (? IS NULL OR BranchId = ?)     -- @branchId
  AND (? IS NULL OR InvoiceId = ?)    -- @invoiceId
ORDER BY PaymentDate DESC
LIMIT ? OFFSET ?;

-- ---------------------------------------------------------------------
-- Limited metadata correction (never Amount/InvoiceId/PaymentStatus).
-- ---------------------------------------------------------------------
UPDATE Payments
SET TransactionReference = ?,
    Notes = ?,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE PaymentId = ? AND PaymentStatus = 'Completed';

-- ---------------------------------------------------------------------
-- Reverse instead of destructive delete: create a linked reversal row,
-- mark the original as Reversed, and resync the invoice — one transaction.
-- ---------------------------------------------------------------------
START TRANSACTION;

UPDATE Payments
SET PaymentStatus = 'Reversed', ReversalReason = ?, UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE PaymentId = ? AND PaymentStatus = 'Completed';

INSERT INTO Payments (
    InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate, Amount,
    PaymentMethod, PaymentStatus, ReceivedByUserId, ReversedPaymentId, ReversalReason, CreatedAtUtc
)
SELECT InvoiceId, CustomerId, BranchId, CONCAT(PaymentNumber, '-REV'), UTC_TIMESTAMP(6), Amount,
       PaymentMethod, 'Reversed', ?, PaymentId, ?, UTC_TIMESTAMP(6)
FROM Payments
WHERE PaymentId = ?;

UPDATE Invoices i
JOIN Payments p ON p.InvoiceId = i.InvoiceId
SET i.PaidAmount = i.PaidAmount - p.Amount,
    i.OutstandingAmount = i.TotalAmount - (i.PaidAmount - p.Amount),
    i.PaymentStatus = IF(i.PaidAmount - p.Amount <= 0, 'Unpaid', 'PartiallyPaid'),
    i.UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE p.PaymentId = ?;

COMMIT;

-- ---------------------------------------------------------------------
-- JOIN example: payment with customer and invoice number, for a receipt.
-- ---------------------------------------------------------------------
SELECT p.PaymentNumber, p.PaymentDate, p.Amount, c.FullName AS CustomerName, i.InvoiceNumber
FROM Payments p
JOIN Customers c ON c.CustomerId = p.CustomerId
JOIN Invoices i ON i.InvoiceId = p.InvoiceId
WHERE p.PaymentId = ?;
