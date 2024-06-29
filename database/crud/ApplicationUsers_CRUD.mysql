-- =====================================================================
-- CustomerLedger — ApplicationUsers_CRUD.sql
-- Explicit SQL CRUD for AspNetUsers (CustomerLedger's ApplicationUser).
-- Password hashing/verification is always handled by ASP.NET Core
-- Identity's UserManager — never write or compare PasswordHash directly
-- from raw SQL, even for demonstration.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT (Identity columns such as PasswordHash/SecurityStamp are
-- populated by UserManager.CreateAsync in the application; shown here
-- with placeholder values purely to document the full row shape).
-- ---------------------------------------------------------------------
INSERT INTO AspNetUsers (
    Id, FullName, BranchId, EmployeeCode, IsActive, CreatedAtUtc,
    UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
    PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
) VALUES (
    ?, ?, ?, ?, 1, UTC_TIMESTAMP(6),
    ?, UPPER(?), ?, UPPER(?), 1,
    ?, ?, ?,
    0, 0, 1, 0
);

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT Id, FullName, BranchId, EmployeeCode, IsActive, Email, CreatedAtUtc, LastLoginAtUtc
FROM AspNetUsers
WHERE Id = ?;

-- ---------------------------------------------------------------------
-- SELECT list with assigned role (JOIN example)
-- ---------------------------------------------------------------------
SELECT u.Id, u.FullName, u.Email, u.EmployeeCode, u.BranchId, u.IsActive, r.Name AS RoleName
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON ur.UserId = u.Id
LEFT JOIN AspNetRoles r ON r.Id = ur.RoleId
ORDER BY u.FullName;

-- ---------------------------------------------------------------------
-- Search / filter (by name, email, or employee code)
-- ---------------------------------------------------------------------
SELECT Id, FullName, Email, EmployeeCode, BranchId, IsActive
FROM AspNetUsers
WHERE FullName LIKE CONCAT('%', ?, '%')
   OR Email LIKE CONCAT('%', ?, '%')
   OR EmployeeCode LIKE CONCAT('%', ?, '%')
ORDER BY FullName;

-- ---------------------------------------------------------------------
-- UPDATE (profile fields only — never PasswordHash/SecurityStamp here)
-- ---------------------------------------------------------------------
UPDATE AspNetUsers
SET FullName = ?,
    EmployeeCode = ?,
    BranchId = ?
WHERE Id = ?;

-- ---------------------------------------------------------------------
-- Deactivate / reactivate instead of destructive delete.
-- ---------------------------------------------------------------------
UPDATE AspNetUsers SET IsActive = 0 WHERE Id = ?;
UPDATE AspNetUsers SET IsActive = 1 WHERE Id = ?;

-- ---------------------------------------------------------------------
-- Record a successful login timestamp.
-- ---------------------------------------------------------------------
UPDATE AspNetUsers SET LastLoginAtUtc = UTC_TIMESTAMP(6) WHERE Id = ?;
