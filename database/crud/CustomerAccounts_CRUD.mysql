-- =====================================================================
-- CustomerLedger — CustomerAccounts_CRUD.sql
-- TotalBilled/TotalPaid/CurrentBalance are never set directly by a form —
-- they are only ever recalculated by transactional application services
-- (Invoice/Payment posting, reconciliation). The UPDATE below intentionally
-- only exposes CreditLimit, matching CustomerAccountService.UpdateCreditLimitAsync.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT (created automatically alongside a new customer — see
-- Customers_CRUD.sql's transaction example)
-- ---------------------------------------------------------------------
INSERT INTO CustomerAccounts (CustomerId, CreditLimit, CurrentBalance, TotalBilled, TotalPaid, AccountStatus, CreatedAtUtc, ConcurrencyVersion)
VALUES (?, ?, 0, 0, 0, 'Active', UTC_TIMESTAMP(6), 0);

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT CustomerAccountId, CustomerId, CreditLimit, CurrentBalance, TotalBilled, TotalPaid, AccountStatus, ConcurrencyVersion
FROM CustomerAccounts
WHERE CustomerAccountId = ?;

-- ---------------------------------------------------------------------
-- SELECT by customer (one-to-one lookup)
-- ---------------------------------------------------------------------
SELECT CustomerAccountId, CreditLimit, CurrentBalance, TotalBilled, TotalPaid, AccountStatus
FROM CustomerAccounts
WHERE CustomerId = ?;

-- ---------------------------------------------------------------------
-- SELECT list: accounts nearing or over their credit limit.
-- ---------------------------------------------------------------------
SELECT a.CustomerAccountId, c.FullName, a.CreditLimit, a.CurrentBalance
FROM CustomerAccounts a
JOIN Customers c ON c.CustomerId = a.CustomerId
WHERE a.CreditLimit > 0 AND a.CurrentBalance >= a.CreditLimit * 0.9
ORDER BY a.CurrentBalance DESC;

-- ---------------------------------------------------------------------
-- UPDATE — the only directly editable field, with optimistic concurrency.
-- ---------------------------------------------------------------------
UPDATE CustomerAccounts
SET CreditLimit = ?,
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE CustomerAccountId = ? AND ConcurrencyVersion = ?;
-- If this UPDATE affects 0 rows, another transaction changed the account
-- first — the application must re-read and ask the operator to retry.

-- ---------------------------------------------------------------------
-- Controlled internal update: recalculating totals (application-service
-- use only — never exposed through a plain edit form). Mirrors the
-- balance-sync step of PaymentService.RecordPaymentAsync.
-- ---------------------------------------------------------------------
UPDATE CustomerAccounts
SET TotalBilled = ?,
    TotalPaid = ?,
    CurrentBalance = TotalBilled - TotalPaid,
    UpdatedAtUtc = UTC_TIMESTAMP(6),
    ConcurrencyVersion = ConcurrencyVersion + 1
WHERE CustomerAccountId = ? AND ConcurrencyVersion = ?;

-- No DELETE statement: a customer's financial account is never removed
-- while the customer record exists (FK restricts it — see 02_CreateTables.sql).
