-- =====================================================================
-- CustomerLedger — Branches_CRUD.sql
-- Explicit SQL CRUD for the Branches table. `?` marks a bind parameter —
-- in application code these are supplied via MySqlCommand.Parameters
-- (MySqlConnector), never string-concatenated. For manual MySQL Workbench
-- testing, replace each `?` with a literal value or SET a session
-- variable and substitute it by hand.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT
-- ---------------------------------------------------------------------
INSERT INTO Branches (BranchCode, Name, Email, PhoneNumber, Address, City, IsActive, CreatedAtUtc)
VALUES (?, ?, ?, ?, ?, ?, 1, UTC_TIMESTAMP(6));

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT BranchId, BranchCode, Name, Email, PhoneNumber, Address, City, IsActive, CreatedAtUtc, UpdatedAtUtc
FROM Branches
WHERE BranchId = ?;

-- ---------------------------------------------------------------------
-- SELECT list (active branches, alphabetical)
-- ---------------------------------------------------------------------
SELECT BranchId, BranchCode, Name, City, IsActive
FROM Branches
WHERE IsActive = 1
ORDER BY Name;

-- ---------------------------------------------------------------------
-- Search / filter (by name, code, or city; optionally include inactive)
-- ---------------------------------------------------------------------
SELECT BranchId, BranchCode, Name, City, IsActive
FROM Branches
WHERE (? = 1 OR IsActive = 1)                              -- @includeInactive
  AND (Name LIKE CONCAT('%', ?, '%')                        -- @search
       OR BranchCode LIKE CONCAT('%', ?, '%')
       OR City LIKE CONCAT('%', ?, '%'))
ORDER BY Name
LIMIT ? OFFSET ?;                                            -- @pageSize, @offset

-- ---------------------------------------------------------------------
-- UPDATE
-- ---------------------------------------------------------------------
UPDATE Branches
SET BranchCode = ?,
    Name = ?,
    Email = ?,
    PhoneNumber = ?,
    Address = ?,
    City = ?,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE BranchId = ?;

-- ---------------------------------------------------------------------
-- Deactivate instead of destructive delete (branches with referenced
-- financial records must not be physically deleted — spec section 6.1).
-- ---------------------------------------------------------------------
UPDATE Branches
SET IsActive = 0,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE BranchId = ?;

UPDATE Branches
SET IsActive = 1,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE BranchId = ?;

-- ---------------------------------------------------------------------
-- JOIN example: branch with its active customer count.
-- ---------------------------------------------------------------------
SELECT b.BranchId, b.Name, COUNT(c.CustomerId) AS ActiveCustomerCount
FROM Branches b
LEFT JOIN Customers c ON c.BranchId = b.BranchId AND c.IsDeleted = 0 AND c.Status = 'Active'
GROUP BY b.BranchId, b.Name
ORDER BY b.Name;
