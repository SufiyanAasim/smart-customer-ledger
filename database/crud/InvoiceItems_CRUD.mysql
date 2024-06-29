-- =====================================================================
-- CustomerLedger — InvoiceItems_CRUD.sql
-- LineTotal = (Quantity * UnitPrice) - DiscountAmount + TaxAmount, always
-- computed by InvoiceCalculationService before these statements run —
-- never trust a client-supplied LineTotal.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT (only while the parent invoice is Draft — enforced in the
-- application's IInvoiceService.AddItemAsync, mirrored here defensively)
-- ---------------------------------------------------------------------
INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
SELECT ?, ?, ?, ?, ?, ?, (? * ?) - ? + ?, UTC_TIMESTAMP(6)
FROM Invoices
WHERE InvoiceId = ? AND InvoiceStatus = 'Draft';

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT InvoiceItemId, InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal
FROM InvoiceItems
WHERE InvoiceItemId = ?;

-- ---------------------------------------------------------------------
-- SELECT list (all items for one invoice)
-- ---------------------------------------------------------------------
SELECT InvoiceItemId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal
FROM InvoiceItems
WHERE InvoiceId = ?
ORDER BY InvoiceItemId;

-- ---------------------------------------------------------------------
-- UPDATE (before invoice finalization only)
-- ---------------------------------------------------------------------
UPDATE InvoiceItems ii
JOIN Invoices i ON i.InvoiceId = ii.InvoiceId AND i.InvoiceStatus = 'Draft'
SET ii.Description = ?,
    ii.Quantity = ?,
    ii.UnitPrice = ?,
    ii.DiscountAmount = ?,
    ii.TaxAmount = ?,
    ii.LineTotal = (? * ?) - ? + ?,
    ii.UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE ii.InvoiceItemId = ?;

-- ---------------------------------------------------------------------
-- DELETE (before invoice finalization only — invoice totals must be
-- recalculated by the application immediately afterward)
-- ---------------------------------------------------------------------
DELETE ii FROM InvoiceItems ii
JOIN Invoices i ON i.InvoiceId = ii.InvoiceId AND i.InvoiceStatus = 'Draft'
WHERE ii.InvoiceItemId = ?;

-- ---------------------------------------------------------------------
-- JOIN example: item with its invoice number, for a printable receipt.
-- ---------------------------------------------------------------------
SELECT i.InvoiceNumber, ii.Description, ii.Quantity, ii.UnitPrice, ii.LineTotal
FROM InvoiceItems ii
JOIN Invoices i ON i.InvoiceId = ii.InvoiceId
WHERE i.InvoiceId = ?
ORDER BY ii.InvoiceItemId;
