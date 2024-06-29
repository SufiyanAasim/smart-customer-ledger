-- =====================================================================
-- CustomerLedger — Customers_CRUD.sql
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT
-- ---------------------------------------------------------------------
INSERT INTO Customers (
    BranchId, CustomerCode, FullName, Email, PhoneNumber, CNIC,
    Address, City, RegistrationDate, Status, IsDeleted, CreatedAtUtc
) VALUES (
    ?, ?, ?, ?, ?, ?,
    ?, ?, UTC_TIMESTAMP(6), 'Active', 0, UTC_TIMESTAMP(6)
);

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT CustomerId, BranchId, CustomerCode, FullName, Email, PhoneNumber, CNIC,
       Address, City, RegistrationDate, Status, IsDeleted
FROM Customers
WHERE CustomerId = ? AND IsDeleted = 0;

-- ---------------------------------------------------------------------
-- SELECT list scoped to a branch (branch isolation applied in SQL, not
-- only in application code, when this query is run directly)
-- ---------------------------------------------------------------------
SELECT CustomerId, CustomerCode, FullName, PhoneNumber, Status
FROM Customers
WHERE BranchId = ? AND IsDeleted = 0
ORDER BY RegistrationDate DESC
LIMIT ? OFFSET ?;

-- ---------------------------------------------------------------------
-- Search / filter (name, code, phone, status) with pagination
-- ---------------------------------------------------------------------
SELECT CustomerId, CustomerCode, FullName, PhoneNumber, Status
FROM Customers
WHERE IsDeleted = 0
  AND (? IS NULL OR BranchId = ?)                              -- @branchId (NULL = all branches, admin only)
  AND (? = '' OR FullName LIKE CONCAT('%', ?, '%')
              OR CustomerCode LIKE CONCAT('%', ?, '%')
              OR PhoneNumber LIKE CONCAT('%', ?, '%'))          -- @search
  AND (? = '' OR Status = ?)                                    -- @status
ORDER BY RegistrationDate DESC
LIMIT ? OFFSET ?;

-- ---------------------------------------------------------------------
-- UPDATE
-- ---------------------------------------------------------------------
UPDATE Customers
SET FullName = ?,
    Email = ?,
    PhoneNumber = ?,
    CNIC = ?,
    Address = ?,
    City = ?,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE CustomerId = ? AND IsDeleted = 0;

-- ---------------------------------------------------------------------
-- Deactivate (soft state change) — customers with financial history must
-- never be physically deleted (spec section 6.3).
-- ---------------------------------------------------------------------
UPDATE Customers
SET Status = 'Inactive', UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE CustomerId = ?;

-- ---------------------------------------------------------------------
-- Soft delete (only ever used for erroneous registrations with zero
-- financial history — enforce that check in application code first).
-- ---------------------------------------------------------------------
UPDATE Customers
SET IsDeleted = 1, UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE CustomerId = ?;

-- ---------------------------------------------------------------------
-- JOIN example: customer with branch name and current account balance.
-- ---------------------------------------------------------------------
SELECT c.CustomerId, c.CustomerCode, c.FullName, b.Name AS BranchName,
       a.CurrentBalance, a.CreditLimit
FROM Customers c
JOIN Branches b ON b.BranchId = c.BranchId
LEFT JOIN CustomerAccounts a ON a.CustomerId = c.CustomerId
WHERE c.IsDeleted = 0
ORDER BY c.FullName;

-- ---------------------------------------------------------------------
-- Transaction example: register a customer and create their financial
-- account atomically (mirrors CustomerService.CreateAsync).
-- ---------------------------------------------------------------------
START TRANSACTION;

INSERT INTO Customers (BranchId, CustomerCode, FullName, PhoneNumber, Address, City, RegistrationDate, Status, IsDeleted, CreatedAtUtc)
VALUES (?, ?, ?, ?, ?, ?, UTC_TIMESTAMP(6), 'Active', 0, UTC_TIMESTAMP(6));

SET @new_customer_id = LAST_INSERT_ID();

INSERT INTO CustomerAccounts (CustomerId, CreditLimit, CurrentBalance, TotalBilled, TotalPaid, AccountStatus, CreatedAtUtc, ConcurrencyVersion)
VALUES (@new_customer_id, ?, 0, 0, 0, 'Active', UTC_TIMESTAMP(6), 0);

COMMIT;
-- On any failure between START TRANSACTION and COMMIT, issue ROLLBACK instead.
-- Full ACID rollback/isolation demonstrations ship with v2.0.0 — Balance,
-- at database/transactions/ACID-Demonstrations.sql.
