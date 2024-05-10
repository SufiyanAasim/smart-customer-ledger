-- =====================================================================
-- CustomerLedger — DemonstrationSeed.sql
-- A small, coherent story across two branches: active customers, a
-- fully paid invoice, a partially paid invoice, an unpaid invoice, an
-- installment plan in progress, and a couple of customer interactions —
-- enough to exercise every dashboard tile and list screen filter without
-- needing the LargeDatasetSeed.sql volume. Requires at least one
-- AspNetUsers row to already exist (run the app once first).
-- =====================================================================

USE customerledger;

SET @seed_user_id = (SELECT Id FROM AspNetUsers ORDER BY CreatedAtUtc LIMIT 1);

-- ---------------------------------------------------------------------
-- Branches
-- ---------------------------------------------------------------------
INSERT INTO Branches (BranchCode, Name, Email, PhoneNumber, Address, City, IsActive, CreatedAtUtc)
SELECT * FROM (SELECT 'DEMO-GULSHAN' AS c, 'Gulshan Demo Branch' AS n, 'gulshan.demo@customerledger.local' AS e, '021-2222222' AS p, 'Block 13, Gulshan-e-Iqbal' AS a, 'Karachi' AS ct, 1 AS act, UTC_TIMESTAMP(6) AS ts) AS src
WHERE NOT EXISTS (SELECT 1 FROM Branches WHERE BranchCode = 'DEMO-GULSHAN');

INSERT INTO Branches (BranchCode, Name, Email, PhoneNumber, Address, City, IsActive, CreatedAtUtc)
SELECT * FROM (SELECT 'DEMO-DEFENCE' AS c, 'Defence Demo Branch' AS n, 'defence.demo@customerledger.local' AS e, '021-3333333' AS p, 'Phase 5, DHA' AS a, 'Karachi' AS ct, 1 AS act, UTC_TIMESTAMP(6) AS ts) AS src
WHERE NOT EXISTS (SELECT 1 FROM Branches WHERE BranchCode = 'DEMO-DEFENCE');

SET @gulshan_id = (SELECT BranchId FROM Branches WHERE BranchCode = 'DEMO-GULSHAN');
SET @defence_id = (SELECT BranchId FROM Branches WHERE BranchCode = 'DEMO-DEFENCE');

-- ---------------------------------------------------------------------
-- Customers + accounts (Gulshan: 3, Defence: 2)
-- ---------------------------------------------------------------------
INSERT INTO Customers (BranchId, CustomerCode, FullName, Email, PhoneNumber, Address, City, RegistrationDate, Status, IsDeleted, CreatedAtUtc)
SELECT v.branch_id, v.code, v.name, v.email, v.phone, v.address, v.city, UTC_TIMESTAMP(6), 'Active', 0, UTC_TIMESTAMP(6)
FROM (
    SELECT @gulshan_id AS branch_id, 'DEMO-CUST-001' AS code, 'Bilal Ahmed' AS name, 'bilal.ahmed@example.com' AS email, '0301-1000001' AS phone, 'House 4, Block 13' AS address, 'Karachi' AS city
    UNION ALL SELECT @gulshan_id, 'DEMO-CUST-002', 'Sana Malik', 'sana.malik@example.com', '0301-1000002', 'House 9, Block 13', 'Karachi'
    UNION ALL SELECT @gulshan_id, 'DEMO-CUST-003', 'Usman Tariq', 'usman.tariq@example.com', '0301-1000003', 'House 21, Block 6', 'Karachi'
    UNION ALL SELECT @defence_id, 'DEMO-CUST-004', 'Hina Farooq', 'hina.farooq@example.com', '0301-1000004', 'Street 12, Phase 5', 'Karachi'
    UNION ALL SELECT @defence_id, 'DEMO-CUST-005', 'Kamran Ali', 'kamran.ali@example.com', '0301-1000005', 'Street 30, Phase 5', 'Karachi'
) AS v
WHERE NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerCode = v.code);

INSERT INTO CustomerAccounts (CustomerId, CreditLimit, CurrentBalance, TotalBilled, TotalPaid, AccountStatus, CreatedAtUtc, ConcurrencyVersion)
SELECT c.CustomerId, 50000, 0, 0, 0, 'Active', UTC_TIMESTAMP(6), 0
FROM Customers c
WHERE c.CustomerCode LIKE 'DEMO-CUST-%'
  AND NOT EXISTS (SELECT 1 FROM CustomerAccounts a WHERE a.CustomerId = c.CustomerId);

-- ---------------------------------------------------------------------
-- Invoice 1 (Bilal, Gulshan): fully paid
-- ---------------------------------------------------------------------
INSERT INTO Invoices (CustomerId, BranchId, InvoiceNumber, InvoiceDate, DueDate, Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount, PaymentStatus, InvoiceStatus, CreatedByUserId, IsDeleted, CreatedAtUtc, ConcurrencyVersion)
SELECT c.CustomerId, c.BranchId, 'DEMO-INV-001', DATE_SUB(UTC_TIMESTAMP(6), INTERVAL 20 DAY), DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 10 DAY),
       35000, 0, 0, 35000, 35000, 0, 'Paid', 'Active', @seed_user_id, 0, UTC_TIMESTAMP(6), 0
FROM Customers c WHERE c.CustomerCode = 'DEMO-CUST-001' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Invoices WHERE InvoiceNumber = 'DEMO-INV-001');

INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
SELECT i.InvoiceId, 'Split AC Unit 1.5 Ton', 1, 35000, 0, 0, 35000, UTC_TIMESTAMP(6)
FROM Invoices i WHERE i.InvoiceNumber = 'DEMO-INV-001'
  AND NOT EXISTS (SELECT 1 FROM InvoiceItems WHERE InvoiceId = i.InvoiceId);

INSERT INTO Payments (InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentStatus, ReceivedByUserId, CreatedAtUtc)
SELECT i.InvoiceId, i.CustomerId, i.BranchId, 'DEMO-PAY-001', DATE_SUB(UTC_TIMESTAMP(6), INTERVAL 18 DAY), 35000, 'Cash', 'Completed', @seed_user_id, UTC_TIMESTAMP(6)
FROM Invoices i WHERE i.InvoiceNumber = 'DEMO-INV-001' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentNumber = 'DEMO-PAY-001');

UPDATE CustomerAccounts a JOIN Customers c ON c.CustomerId = a.CustomerId
SET a.TotalBilled = 35000, a.TotalPaid = 35000, a.CurrentBalance = 0
WHERE c.CustomerCode = 'DEMO-CUST-001';

-- ---------------------------------------------------------------------
-- Invoice 2 (Sana, Gulshan): partially paid
-- ---------------------------------------------------------------------
INSERT INTO Invoices (CustomerId, BranchId, InvoiceNumber, InvoiceDate, DueDate, Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount, PaymentStatus, InvoiceStatus, CreatedByUserId, IsDeleted, CreatedAtUtc, ConcurrencyVersion)
SELECT c.CustomerId, c.BranchId, 'DEMO-INV-002', DATE_SUB(UTC_TIMESTAMP(6), INTERVAL 10 DAY), DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 20 DAY),
       60000, 0, 0, 60000, 20000, 40000, 'PartiallyPaid', 'Active', @seed_user_id, 0, UTC_TIMESTAMP(6), 0
FROM Customers c WHERE c.CustomerCode = 'DEMO-CUST-002' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Invoices WHERE InvoiceNumber = 'DEMO-INV-002');

INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
SELECT i.InvoiceId, 'Refrigerator 18 cu ft', 1, 60000, 0, 0, 60000, UTC_TIMESTAMP(6)
FROM Invoices i WHERE i.InvoiceNumber = 'DEMO-INV-002'
  AND NOT EXISTS (SELECT 1 FROM InvoiceItems WHERE InvoiceId = i.InvoiceId);

INSERT INTO Payments (InvoiceId, CustomerId, BranchId, PaymentNumber, PaymentDate, Amount, PaymentMethod, PaymentStatus, ReceivedByUserId, CreatedAtUtc)
SELECT i.InvoiceId, i.CustomerId, i.BranchId, 'DEMO-PAY-002', DATE_SUB(UTC_TIMESTAMP(6), INTERVAL 8 DAY), 20000, 'BankTransfer', 'Completed', @seed_user_id, UTC_TIMESTAMP(6)
FROM Invoices i WHERE i.InvoiceNumber = 'DEMO-INV-002' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Payments WHERE PaymentNumber = 'DEMO-PAY-002');

UPDATE CustomerAccounts a JOIN Customers c ON c.CustomerId = a.CustomerId
SET a.TotalBilled = 60000, a.TotalPaid = 20000, a.CurrentBalance = 40000
WHERE c.CustomerCode = 'DEMO-CUST-002';

-- ---------------------------------------------------------------------
-- Invoice 3 (Usman, Gulshan): unpaid, past due — feeds the overdue view
-- ---------------------------------------------------------------------
INSERT INTO Invoices (CustomerId, BranchId, InvoiceNumber, InvoiceDate, DueDate, Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount, PaymentStatus, InvoiceStatus, CreatedByUserId, IsDeleted, CreatedAtUtc, ConcurrencyVersion)
SELECT c.CustomerId, c.BranchId, 'DEMO-INV-003', DATE_SUB(UTC_TIMESTAMP(6), INTERVAL 45 DAY), DATE_SUB(UTC_TIMESTAMP(6), INTERVAL 15 DAY),
       15000, 0, 0, 15000, 0, 15000, 'Unpaid', 'Active', @seed_user_id, 0, UTC_TIMESTAMP(6), 0
FROM Customers c WHERE c.CustomerCode = 'DEMO-CUST-003' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Invoices WHERE InvoiceNumber = 'DEMO-INV-003');

INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
SELECT i.InvoiceId, 'Microwave Oven', 1, 15000, 0, 0, 15000, UTC_TIMESTAMP(6)
FROM Invoices i WHERE i.InvoiceNumber = 'DEMO-INV-003'
  AND NOT EXISTS (SELECT 1 FROM InvoiceItems WHERE InvoiceId = i.InvoiceId);

UPDATE CustomerAccounts a JOIN Customers c ON c.CustomerId = a.CustomerId
SET a.TotalBilled = 15000, a.CurrentBalance = 15000
WHERE c.CustomerCode = 'DEMO-CUST-003';

-- ---------------------------------------------------------------------
-- Invoice 4 (Hina, Defence): on an active installment plan
-- ---------------------------------------------------------------------
INSERT INTO Invoices (CustomerId, BranchId, InvoiceNumber, InvoiceDate, DueDate, Subtotal, DiscountAmount, TaxAmount, TotalAmount, PaidAmount, OutstandingAmount, PaymentStatus, InvoiceStatus, CreatedByUserId, IsDeleted, CreatedAtUtc, ConcurrencyVersion)
SELECT c.CustomerId, c.BranchId, 'DEMO-INV-004', DATE_SUB(UTC_TIMESTAMP(6), INTERVAL 5 DAY), DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 25 DAY),
       90000, 0, 0, 90000, 0, 90000, 'Unpaid', 'Active', @seed_user_id, 0, UTC_TIMESTAMP(6), 0
FROM Customers c WHERE c.CustomerCode = 'DEMO-CUST-004' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM Invoices WHERE InvoiceNumber = 'DEMO-INV-004');

INSERT INTO InvoiceItems (InvoiceId, Description, Quantity, UnitPrice, DiscountAmount, TaxAmount, LineTotal, CreatedAtUtc)
SELECT i.InvoiceId, 'Living Room Sofa Set', 1, 90000, 0, 0, 90000, UTC_TIMESTAMP(6)
FROM Invoices i WHERE i.InvoiceNumber = 'DEMO-INV-004'
  AND NOT EXISTS (SELECT 1 FROM InvoiceItems WHERE InvoiceId = i.InvoiceId);

UPDATE CustomerAccounts a JOIN Customers c ON c.CustomerId = a.CustomerId
SET a.TotalBilled = 90000, a.CurrentBalance = 90000
WHERE c.CustomerCode = 'DEMO-CUST-004';

INSERT INTO InstallmentPlans (InvoiceId, NumberOfInstallments, TotalInstallmentAmount, DownPayment, StartDate, EndDate, Frequency, Status, ApprovedByUserId, CreatedAtUtc)
SELECT i.InvoiceId, 3, 90000, 0, UTC_TIMESTAMP(6), DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 3 MONTH), 'Monthly', 'Active', @seed_user_id, UTC_TIMESTAMP(6)
FROM Invoices i WHERE i.InvoiceNumber = 'DEMO-INV-004' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM InstallmentPlans WHERE InvoiceId = i.InvoiceId);

INSERT INTO InstallmentSchedules (InstallmentPlanId, InstallmentNumber, DueDate, AmountDue, AmountPaid, Status, CreatedAtUtc)
SELECT p.InstallmentPlanId, n.installment_number, DATE_ADD(p.StartDate, INTERVAL n.installment_number MONTH), 30000, 0, 'Pending', UTC_TIMESTAMP(6)
FROM InstallmentPlans p
JOIN Invoices i ON i.InvoiceId = p.InvoiceId
JOIN (SELECT 1 AS installment_number UNION ALL SELECT 2 UNION ALL SELECT 3) AS n
WHERE i.InvoiceNumber = 'DEMO-INV-004'
  AND NOT EXISTS (SELECT 1 FROM InstallmentSchedules s WHERE s.InstallmentPlanId = p.InstallmentPlanId AND s.InstallmentNumber = n.installment_number);

-- ---------------------------------------------------------------------
-- Customer interactions
-- ---------------------------------------------------------------------
INSERT INTO CustomerInteractions (CustomerId, BranchId, InteractionType, Subject, Description, InteractionDate, FollowUpDate, Status, RecordedByUserId, CreatedAtUtc)
SELECT c.CustomerId, c.BranchId, 'PaymentReminder', 'Reminder call for DEMO-INV-003', 'Called to remind about the overdue microwave invoice.', UTC_TIMESTAMP(6), DATE_ADD(UTC_TIMESTAMP(6), INTERVAL 3 DAY), 'FollowUpScheduled', @seed_user_id, UTC_TIMESTAMP(6)
FROM Customers c WHERE c.CustomerCode = 'DEMO-CUST-003' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM CustomerInteractions WHERE Subject = 'Reminder call for DEMO-INV-003');

INSERT INTO CustomerInteractions (CustomerId, BranchId, InteractionType, Subject, Description, InteractionDate, Status, RecordedByUserId, CreatedAtUtc)
SELECT c.CustomerId, c.BranchId, 'Complaint', 'Delivery delay complaint', 'Customer reported the sofa set delivery was two days late.', UTC_TIMESTAMP(6), 'Resolved', @seed_user_id, UTC_TIMESTAMP(6)
FROM Customers c WHERE c.CustomerCode = 'DEMO-CUST-004' AND @seed_user_id IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM CustomerInteractions WHERE Subject = 'Delivery delay complaint');
