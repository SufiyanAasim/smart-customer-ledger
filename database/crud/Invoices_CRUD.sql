-- =====================================================================
-- CustomerLedger — Invoices_CRUD.sql
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT (Draft header — items added afterward, see InvoiceItems_CRUD.sql)
-- ---------------------------------------------------------------------
INSERT INTO Invoices (
    CustomerId, BranchId, InvoiceNumber, InvoiceDate, DueDate,
    Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount,
    PaymentStatus, InvoiceStatus, CreatedByUserId, IsDeleted, CreatedAtUtc, ConcurrencyVersion
) VALUES (
    ?, ?, ?, ?, ?,
    0, 0, 0, 0, 0, 0,
    'Unpaid', 'Draft', ?, 0, UTC_TIMESTAMP(6), 0
);

-- ---------------------------------------------------------------------
-- SELECT by primary key (with items via JOIN)
-- ---------------------------------------------------------------------
SELECT i.InvoiceId, i.InvoiceNumber, i.InvoiceDate, i.DueDate, i.TotalAmount,
       i.PaidAmount, i.OutstandingAmount, i.PaymentStatus, i.InvoiceStatus,
       ii.InvoiceItemId, ii.Description, ii.Quantity, ii.UnitPrice, ii.LineTotal
FROM Invoices i
LEFT JOIN InvoiceItems ii ON ii.InvoiceId = i.InvoiceId
WHERE i.InvoiceId = ? AND i.IsDeleted = 0;

-- ---------------------------------------------------------------------
-- SELECT list with search/filter/pagination
-- ---------------------------------------------------------------------
SELECT InvoiceId, InvoiceNumber, InvoiceDate, TotalAmount, OutstandingAmount, PaymentStatus, InvoiceStatus
FROM Invoices
WHERE IsDeleted = 0
  AND (? IS NULL OR BranchId = ?)                 -- @branchId
  AND (? IS NULL OR CustomerId = ?)                -- @customerId
  AND (? = '' OR InvoiceStatus = ?)                -- @status
ORDER BY InvoiceDate DESC
LIMIT ? OFFSET ?;

-- ---------------------------------------------------------------------
-- UPDATE: recalculated totals after an item is added/removed (mirrors
-- InvoiceCalculationService.RecalculateInvoiceTotals — called from the
-- application, never computed independently in SQL, to keep one source
-- of truth for the arithmetic).
-- ---------------------------------------------------------------------
UPDATE Invoices
SET Subtotal = ?,
    DiscountAmount = ?,
    TaxAmount = ?,
    TotalAmount = ?,
    OutstandingAmount = TotalAmount - PaidAmount,
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE InvoiceId = ? AND InvoiceStatus = 'Draft' AND ConcurrencyVersion = ?;

-- ---------------------------------------------------------------------
-- Activate: Draft -> Active (items can no longer be changed afterward).
-- ---------------------------------------------------------------------
UPDATE Invoices
SET InvoiceStatus = 'Active',
    DueDate = COALESCE(DueDate, DATE_ADD(InvoiceDate, INTERVAL 30 DAY)),
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE InvoiceId = ? AND InvoiceStatus = 'Draft';

-- ---------------------------------------------------------------------
-- Cancel instead of destructive delete — only permitted with zero
-- completed payments (enforced in application code before this runs).
-- ---------------------------------------------------------------------
UPDATE Invoices
SET InvoiceStatus = 'Cancelled', UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE InvoiceId = ?
  AND NOT EXISTS (
      SELECT 1 FROM Payments p WHERE p.InvoiceId = Invoices.InvoiceId AND p.PaymentStatus = 'Completed'
  );

-- ---------------------------------------------------------------------
-- JOIN example: invoice with customer and branch names for a receipt header.
-- ---------------------------------------------------------------------
SELECT i.InvoiceNumber, i.InvoiceDate, i.TotalAmount, c.FullName AS CustomerName, b.Name AS BranchName
FROM Invoices i
JOIN Customers c ON c.CustomerId = i.CustomerId
JOIN Branches b ON b.BranchId = i.BranchId
WHERE i.InvoiceId = ?;
