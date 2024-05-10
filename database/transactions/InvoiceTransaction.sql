-- =====================================================================
-- CustomerLedger — InvoiceTransaction.sql
-- Mirrors InvoiceService.ActivateAsync: activating a Draft invoice and
-- syncing the customer's account TotalBilled/CurrentBalance happen in
-- one transaction — either both changes land, or neither does.
-- =====================================================================

USE customerledger;

START TRANSACTION;

SELECT TotalAmount, InvoiceStatus INTO @total_amount, @invoice_status
FROM Invoices
WHERE InvoiceId = ? AND InvoiceStatus = 'Draft'
FOR UPDATE;

-- Application aborts (ROLLBACK) here if @invoice_status is NULL (not Draft / not found).

UPDATE Invoices
SET InvoiceStatus = 'Active',
    DueDate = COALESCE(DueDate, DATE_ADD(InvoiceDate, INTERVAL 30 DAY)),
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE InvoiceId = ?;

UPDATE CustomerAccounts a
JOIN Invoices i ON i.CustomerId = a.CustomerId
SET a.TotalBilled = a.TotalBilled + @total_amount,
    a.CurrentBalance = a.TotalBilled + @total_amount - a.TotalPaid,
    a.UpdatedAtUtc = UTC_TIMESTAMP(6),
    a.ConcurrencyVersion = a.ConcurrencyVersion + 1
WHERE i.InvoiceId = ?;

COMMIT;
