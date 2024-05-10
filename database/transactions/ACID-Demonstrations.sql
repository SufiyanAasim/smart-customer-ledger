-- =====================================================================
-- CustomerLedger — ACID-Demonstrations.sql
-- A guided tour of all four ACID properties as implemented in this
-- project. Each section names the concrete mechanism and where to run
-- it — this file itself does not repeat the full statements already in
-- InvoiceTransaction.sql / PaymentTransaction.sql / PaymentRollbackDemo.sql
-- / PaymentReversal.sql / Reconciliation.sql.
--
-- Isolation level: MySQL's default, REPEATABLE READ, combined with
-- explicit SELECT ... FOR UPDATE row locks on the Invoice being paid.
-- This project does not lower isolation to READ UNCOMMITTED anywhere —
-- see PaymentService.RecordPaymentAsync / ReverseAsync in the C# code
-- for the exact lock scope.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- ATOMICITY
-- ---------------------------------------------------------------------
-- Successful commit: PaymentTransaction.sql — INSERT Payments + UPDATE
--   Invoices + UPDATE CustomerAccounts all succeed together inside one
--   START TRANSACTION / COMMIT block.
-- Forced rollback: PaymentRollbackDemo.sql — proves that a ROLLBACK mid-
--   transaction leaves every table exactly as it was, with no partial
--   Payments row or partially-updated Invoice.

-- ---------------------------------------------------------------------
-- CONSISTENCY
-- ---------------------------------------------------------------------
-- Constraint violation: this should raise a CHECK-constraint error
-- (error 3819) rather than silently corrupt the row.
INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
VALUES (1, 'Consistency demo — should fail', -1, 100, 0, 0, -100, UTC_TIMESTAMP(6));

-- Overpayment rejection is enforced in the application layer
-- (PaymentService.RecordPaymentAsync: "payment.Amount > invoice.OutstandingAmount")
-- rather than a database CHECK, because it depends on comparing two
-- columns of the SAME row read under a row lock — see PaymentTransaction.sql's
-- application-level guard comment.

-- ---------------------------------------------------------------------
-- ISOLATION
-- ---------------------------------------------------------------------
-- Concurrent payment attempt — run in two separate MySQL Workbench tabs
-- (two separate sessions) against the same invoice, substituting a real
-- InvoiceId with OutstandingAmount = 1000:
--
--   Session A                              Session B
--   ---------                              ---------
--   START TRANSACTION;
--   SELECT * FROM Invoices
--     WHERE InvoiceId = 1 FOR UPDATE;
--   -- (holds the lock, does not commit yet)
--                                           START TRANSACTION;
--                                           SELECT * FROM Invoices
--                                             WHERE InvoiceId = 1 FOR UPDATE;
--                                           -- BLOCKS here until Session A finishes
--   UPDATE Invoices SET PaidAmount = ...
--   COMMIT;
--                                           -- now unblocks, sees Session A's committed
--                                           -- PaidAmount, and validates against it —
--                                           -- so the two payments cannot both succeed
--                                           -- if their combined amount would overpay.
--
-- This is exactly what PaymentService.RecordPaymentAsync's FOR UPDATE lock
-- guarantees at the application layer.

-- ---------------------------------------------------------------------
-- DURABILITY
-- ---------------------------------------------------------------------
-- After running PaymentTransaction.sql's COMMIT, restart the MySQL
-- server (or simply reconnect in a fresh session) and re-run:
SELECT PaymentId, PaymentNumber, Amount, PaymentStatus
FROM Payments
ORDER BY PaymentId DESC
LIMIT 1;
-- The committed row must still be present — InnoDB's redo log guarantees
-- this without any CustomerLedger-specific configuration.
