-- =====================================================================
-- CustomerLedger — DevelopmentSeed.sql
-- Small, idempotent local-development dataset: one extra branch and a
-- couple of demonstration customers/invoices, layered on top of
-- whatever AdminUserSeeder/RoleSeeder already created at application
-- startup (run the app once against a fresh database before this
-- script, so at least one AspNetUsers row exists to attribute records to).
--
-- Demonstration-scale and large-volume seed data (DemonstrationSeed.sql,
-- LargeDatasetSeed.sql) ship with v3.0.0 — Snapshot.
-- =====================================================================

USE customerledger;

INSERT INTO Branches (BranchCode, Name, Email, PhoneNumber, Address, City, IsActive, CreatedAtUtc)
SELECT 'NORTH', 'North Karachi Branch', 'north.branch@customerledger.local', '021-1111111', 'Block 5, North Karachi', 'Karachi', 1, UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM Branches WHERE BranchCode = 'NORTH');

-- Look up whichever user was seeded first (AdminUserSeeder / a manually
-- created account) to attribute the demonstration records to.
SET @seed_user_id = (SELECT Id FROM AspNetUsers ORDER BY CreatedAtUtc LIMIT 1);
SET @main_branch_id = (SELECT BranchId FROM Branches WHERE BranchCode = 'MAIN' LIMIT 1);

INSERT INTO Customers (BranchId, CustomerCode, FullName, Email, PhoneNumber, Address, City, RegistrationDate, Status, IsDeleted, CreatedAtUtc)
SELECT @main_branch_id, 'CUST-0001', 'Ayesha Khan', 'ayesha.khan@example.com', '0300-1234567', 'House 12, Gulshan-e-Iqbal', 'Karachi', UTC_TIMESTAMP(6), 'Active', 0, UTC_TIMESTAMP(6)
WHERE @main_branch_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerCode = 'CUST-0001');

INSERT INTO CustomerAccounts (CustomerId, CreditLimit, CurrentBalance, TotalBilled, TotalPaid, AccountStatus, CreatedAtUtc, ConcurrencyVersion)
SELECT c.CustomerId, 50000, 0, 0, 0, 'Active', UTC_TIMESTAMP(6), 0
FROM Customers c
WHERE c.CustomerCode = 'CUST-0001'
  AND NOT EXISTS (SELECT 1 FROM CustomerAccounts a WHERE a.CustomerId = c.CustomerId);

INSERT INTO Invoices (
    CustomerId, BranchId, InvoiceNumber, InvoiceDate, DueDate,
    Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount,
    PaymentStatus, InvoiceStatus, CreatedByUserId, IsDeleted, CreatedAtUtc, ConcurrencyVersion
)
SELECT c.CustomerId, c.BranchId, 'INV-SEED-0001', UTC_TIMESTAMP(6), DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 30 DAY),
       25000, 0, 0, 25000, 0, 25000,
       'Unpaid', 'Active', @seed_user_id, 0, UTC_TIMESTAMP(6), 0
FROM Customers c
WHERE c.CustomerCode = 'CUST-0001' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Invoices WHERE InvoiceNumber = 'INV-SEED-0001');

INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
SELECT i.InvoiceId, 'LED Television 43-inch', 1, 25000, 0, 0, 25000, UTC_TIMESTAMP(6)
FROM Invoices i
WHERE i.InvoiceNumber = 'INV-SEED-0001'
  AND NOT EXISTS (SELECT 1 FROM InvoiceItems ii WHERE ii.InvoiceId = i.InvoiceId);
