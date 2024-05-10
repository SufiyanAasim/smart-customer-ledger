-- =====================================================================
-- CustomerLedger — Reconciliation.sql
-- Mirrors ReconciliationService: recomputes a customer account's totals
-- purely from source rows (active invoices, completed non-reversed
-- payments) and corrects the stored account row if it has drifted.
-- =====================================================================

USE customerledger;

START TRANSACTION;

SELECT
    COALESCE((SELECT SUM(TotalAmount) FROM Invoices WHERE CustomerId = ? AND IsDeleted = 0 AND InvoiceStatus = 'Active'), 0),
    COALESCE((SELECT SUM(Amount) FROM Payments WHERE CustomerId = ? AND PaymentStatus = 'Completed'), 0)
INTO @recalculated_billed, @recalculated_paid;

SELECT TotalBilled, TotalPaid, CurrentBalance
INTO @previous_billed, @previous_paid, @previous_balance
FROM CustomerAccounts
WHERE CustomerId = ?
FOR UPDATE;

-- Report the mismatch (if any) before correcting — this is what the
-- application's ReconciliationReport surfaces to an Administrator.
SELECT
    @previous_billed AS previous_total_billed, @recalculated_billed AS recalculated_total_billed,
    @previous_paid AS previous_total_paid, @recalculated_paid AS recalculated_total_paid,
    @previous_balance AS previous_current_balance, (@recalculated_billed - @recalculated_paid) AS recalculated_current_balance;

UPDATE CustomerAccounts
SET TotalBilled = @recalculated_billed,
    TotalPaid = @recalculated_paid,
    CurrentBalance = @recalculated_billed - @recalculated_paid,
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE CustomerId = ?
  AND (TotalBilled <> @recalculated_billed OR TotalPaid <> @recalculated_paid OR CurrentBalance <> @recalculated_billed - @recalculated_paid);

INSERT INTO AuditLogs (BranchId, TableName, RecordId, ActionType, OldValuesJson, NewValuesJson, CreatedAtUtc, ReviewStatus, IsArchived)
SELECT c.BranchId, 'CustomerAccounts', a.CustomerAccountId, 'Reconcile',
       JSON_OBJECT('TotalBilled', @previous_billed, 'TotalPaid', @previous_paid, 'CurrentBalance', @previous_balance),
       JSON_OBJECT('TotalBilled', @recalculated_billed, 'TotalPaid', @recalculated_paid, 'CurrentBalance', @recalculated_billed - @recalculated_paid),
       UTC_TIMESTAMP(6), 'Unreviewed', 0
FROM CustomerAccounts a
JOIN Customers c ON c.CustomerId = a.CustomerId
WHERE a.CustomerId = ?
  AND (@previous_billed <> @recalculated_billed OR @previous_paid <> @recalculated_paid);

COMMIT;
