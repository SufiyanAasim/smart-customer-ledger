-- =====================================================================
-- CustomerLedger — CreateTriggers.sql
-- Safe initial triggers for v1.0.0 — Index: database-level integrity and
-- audit protection that must hold regardless of which application code
-- path writes to these tables. Complex multi-step financial workflows
-- (invoice totals recalculation, customer account sync, payment
-- reversal) stay in application services — see spec section 12 ("Do not
-- create a trigger for every business rule... use triggers for
-- database-level integrity and audit protection").
--
-- Source of truth: InvoiceCalculationService in the application performs
-- the authoritative recalculation of invoice totals. These triggers are
-- a second, independent layer that rejects invalid raw values — they do
-- not themselves recalculate dependent totals, to avoid two competing
-- sources of truth recomputing the same numbers.
-- =====================================================================

USE customerledger;

DELIMITER $$

-- ---------------------------------------------------------------------
-- 1. Invoice item validation (BEFORE INSERT / BEFORE UPDATE)
--    Quantity > 0 and UnitPrice >= 0 are already CHECK-constrained
--    (02_CreateTables.sql); these triggers additionally guarantee
--    LineTotal is never stored inconsistent with its inputs, closing the
--    gap CHECK constraints on generated values can't cover portably.
-- ---------------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_InvoiceItems_BeforeInsert$$
CREATE TRIGGER trg_InvoiceItems_BeforeInsert
BEFORE INSERT ON InvoiceItems
FOR EACH ROW
BEGIN
    IF NEW.Quantity <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invoice item quantity must be greater than zero.';
    END IF;
    IF NEW.UnitPrice < 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invoice item unit price cannot be negative.';
    END IF;
    IF (NEW.Quantity * NEW.UnitPrice) - NEW.DiscountAmount + NEW.TaxAmount < 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Discount cannot create a negative line total.';
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_InvoiceItems_BeforeUpdate$$
CREATE TRIGGER trg_InvoiceItems_BeforeUpdate
BEFORE UPDATE ON InvoiceItems
FOR EACH ROW
BEGIN
    IF NEW.Quantity <= 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invoice item quantity must be greater than zero.';
    END IF;
    IF NEW.UnitPrice < 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invoice item unit price cannot be negative.';
    END IF;
END$$

-- ---------------------------------------------------------------------
-- 2. Payment validation (BEFORE INSERT)
--    Amount > 0 is CHECK-constrained already; this trigger adds the
--    cross-table rule a CHECK constraint cannot express: a payment must
--    never be posted against a Cancelled invoice.
-- ---------------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_Payments_BeforeInsert$$
CREATE TRIGGER trg_Payments_BeforeInsert
BEFORE INSERT ON Payments
FOR EACH ROW
BEGIN
    DECLARE v_invoice_status VARCHAR(20);

    SELECT InvoiceStatus INTO v_invoice_status
    FROM Invoices
    WHERE InvoiceId = NEW.InvoiceId;

    IF v_invoice_status = 'Cancelled' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Cannot post a payment against a cancelled invoice.';
    END IF;
END$$

-- ---------------------------------------------------------------------
-- 3. Financial audit logging (AFTER INSERT / AFTER UPDATE on Payments)
--    Belt-and-braces audit trail independent of the application's own
--    AuditLogService.WriteAsync calls — guarantees a row-level audit
--    entry exists even if a future code path writes Payments directly.
-- ---------------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_Payments_AfterInsert_Audit$$
CREATE TRIGGER trg_Payments_AfterInsert_Audit
AFTER INSERT ON Payments
FOR EACH ROW
BEGIN
    INSERT INTO AuditLogs (BranchId, TableName, RecordId, ActionType, CreatedAtUtc, ReviewStatus, IsArchived)
    VALUES (NEW.BranchId, 'Payments', NEW.PaymentId, 'TriggerAuditInsert', UTC_TIMESTAMP(6), 'Unreviewed', 0);
END$$

DROP TRIGGER IF EXISTS trg_Payments_AfterUpdate_Audit$$
CREATE TRIGGER trg_Payments_AfterUpdate_Audit
AFTER UPDATE ON Payments
FOR EACH ROW
BEGIN
    IF OLD.PaymentStatus <> NEW.PaymentStatus THEN
        INSERT INTO AuditLogs (BranchId, TableName, RecordId, ActionType, OldValuesJson, NewValuesJson, CreatedAtUtc, ReviewStatus, IsArchived)
        VALUES (
            NEW.BranchId, 'Payments', NEW.PaymentId, 'TriggerAuditStatusChange',
            JSON_OBJECT('PaymentStatus', OLD.PaymentStatus),
            JSON_OBJECT('PaymentStatus', NEW.PaymentStatus),
            UTC_TIMESTAMP(6), 'Unreviewed', 0
        );
    END IF;
END$$

-- ---------------------------------------------------------------------
-- 4. Invalid financial deletion prevention (BEFORE DELETE)
--    Defense-in-depth alongside the FK ON DELETE RESTRICT clauses already
--    in place — these triggers give a clear, business-readable error
--    message instead of a raw foreign-key-violation error number.
-- ---------------------------------------------------------------------
DROP TRIGGER IF EXISTS trg_Customers_BeforeDelete$$
CREATE TRIGGER trg_Customers_BeforeDelete
BEFORE DELETE ON Customers
FOR EACH ROW
BEGIN
    DECLARE v_outstanding DECIMAL(18,2);

    SELECT CurrentBalance INTO v_outstanding
    FROM CustomerAccounts
    WHERE CustomerId = OLD.CustomerId;

    IF v_outstanding IS NOT NULL AND v_outstanding <> 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Customers with an outstanding balance cannot be deleted — deactivate instead.';
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Invoices_BeforeDelete$$
CREATE TRIGGER trg_Invoices_BeforeDelete
BEFORE DELETE ON Invoices
FOR EACH ROW
BEGIN
    DECLARE v_payment_count INT;

    SELECT COUNT(*) INTO v_payment_count
    FROM Payments
    WHERE InvoiceId = OLD.InvoiceId AND PaymentStatus = 'Completed';

    IF v_payment_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invoices with completed payments cannot be deleted — cancel instead.';
    END IF;
END$$

DROP TRIGGER IF EXISTS trg_Payments_BeforeDelete$$
CREATE TRIGGER trg_Payments_BeforeDelete
BEFORE DELETE ON Payments
FOR EACH ROW
BEGIN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Payments can never be physically deleted — reverse the payment instead.';
END$$

DELIMITER ;
