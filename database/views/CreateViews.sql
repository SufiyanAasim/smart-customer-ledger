-- =====================================================================
-- CustomerLedger — CreateViews.sql
-- The six required reporting views (spec section 11). MySQL views are
-- not physically indexed — performance comes entirely from indexes on
-- the underlying tables (see database/indexes/CreateIndexes.sql),
-- selective predicates, and avoiding unnecessary columns. See
-- database/verification/VerifyViews.sql for EXPLAIN evidence.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- 1. vw_CustomerAccountSummary
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vw_CustomerAccountSummary;
CREATE VIEW vw_CustomerAccountSummary AS
SELECT
    c.CustomerId,
    c.CustomerCode,
    c.FullName AS CustomerName,
    c.BranchId,
    b.Name AS BranchName,
    COUNT(DISTINCT i.InvoiceId) AS TotalInvoices,
    a.TotalBilled,
    a.TotalPaid,
    a.CurrentBalance AS OutstandingBalance,
    a.CreditLimit,
    a.AccountStatus
FROM Customers c
JOIN Branches b ON b.BranchId = c.BranchId
LEFT JOIN CustomerAccounts a ON a.CustomerId = c.CustomerId
LEFT JOIN Invoices i ON i.CustomerId = c.CustomerId AND i.IsDeleted = 0
WHERE c.IsDeleted = 0
GROUP BY c.CustomerId, c.CustomerCode, c.FullName, c.BranchId, b.Name,
         a.TotalBilled, a.TotalPaid, a.CurrentBalance, a.CreditLimit, a.AccountStatus;

-- ---------------------------------------------------------------------
-- 2. vw_InvoicePaymentStatus
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vw_InvoicePaymentStatus;
CREATE VIEW vw_InvoicePaymentStatus AS
SELECT
    i.InvoiceId,
    i.InvoiceNumber,
    i.CustomerId,
    c.FullName AS CustomerName,
    i.BranchId,
    i.InvoiceDate,
    i.DueDate,
    i.TotalAmount,
    i.PaidAmount,
    i.OutstandingAmount,
    i.PaymentStatus,
    i.InvoiceStatus
FROM Invoices i
JOIN Customers c ON c.CustomerId = i.CustomerId
WHERE i.IsDeleted = 0;

-- ---------------------------------------------------------------------
-- 3. vw_OverdueInstallments
-- DaysOverdue is a computed value — a normal MySQL view cannot be
-- indexed, so this stays cheap only because InstallmentSchedules(Status,
-- DueDate) is indexed and this predicate uses it directly.
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vw_OverdueInstallments;
CREATE VIEW vw_OverdueInstallments AS
SELECT
    c.CustomerId,
    c.FullName AS CustomerName,
    c.PhoneNumber,
    i.BranchId,
    i.InvoiceNumber,
    p.InstallmentPlanId,
    s.InstallmentNumber,
    s.DueDate,
    s.AmountDue,
    s.AmountPaid,
    (s.AmountDue - s.AmountPaid) AS OutstandingAmount,
    DATEDIFF(UTC_TIMESTAMP(), s.DueDate) AS DaysOverdue,
    s.Status AS InstallmentStatus
FROM InstallmentSchedules s
JOIN InstallmentPlans p ON p.InstallmentPlanId = s.InstallmentPlanId
JOIN Invoices i ON i.InvoiceId = p.InvoiceId
JOIN Customers c ON c.CustomerId = i.CustomerId
WHERE s.Status = 'Pending' AND s.DueDate < UTC_TIMESTAMP();

-- ---------------------------------------------------------------------
-- 4. vw_BranchRevenueSummary
-- ---------------------------------------------------------------------
-- Customer count is aggregated separately from invoices in a derived
-- table before joining, so a branch with many invoices per customer
-- does not inflate COUNT(DISTINCT c.CustomerId) via the join fan-out.
DROP VIEW IF EXISTS vw_BranchRevenueSummary;
CREATE VIEW vw_BranchRevenueSummary AS
SELECT
    b.BranchId,
    b.BranchCode,
    b.Name AS BranchName,
    COALESCE(cust.TotalCustomers, 0) AS TotalCustomers,
    COUNT(i.InvoiceId) AS TotalInvoices,
    COALESCE(SUM(i.TotalAmount), 0) AS TotalBilled,
    COALESCE(SUM(i.PaidAmount), 0) AS TotalCollected,
    COALESCE(SUM(i.OutstandingAmount), 0) AS TotalOutstanding,
    SUM(CASE WHEN i.PaymentStatus = 'PartiallyPaid' THEN 1 ELSE 0 END) AS PartiallyPaidInvoiceCount,
    SUM(CASE WHEN i.PaymentStatus = 'Unpaid' THEN 1 ELSE 0 END) AS UnpaidInvoiceCount
FROM Branches b
LEFT JOIN (
    SELECT BranchId, COUNT(*) AS TotalCustomers
    FROM Customers
    WHERE IsDeleted = 0
    GROUP BY BranchId
) AS cust ON cust.BranchId = b.BranchId
LEFT JOIN Invoices i ON i.BranchId = b.BranchId AND i.IsDeleted = 0
GROUP BY b.BranchId, b.BranchCode, b.Name, cust.TotalCustomers;

-- ---------------------------------------------------------------------
-- 5. vw_CustomerInteractionHistory
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vw_CustomerInteractionHistory;
CREATE VIEW vw_CustomerInteractionHistory AS
SELECT
    ci.CustomerInteractionId,
    ci.CustomerId,
    c.CustomerCode,
    c.FullName AS CustomerName,
    ci.BranchId,
    ci.InteractionType,
    ci.Subject,
    ci.InteractionDate,
    ci.FollowUpDate,
    u.FullName AS StaffName,
    ci.Status AS InteractionStatus
FROM CustomerInteractions ci
JOIN Customers c ON c.CustomerId = ci.CustomerId
JOIN AspNetUsers u ON u.Id = ci.RecordedByUserId;

-- ---------------------------------------------------------------------
-- 6. vw_DailyTransactionSummary
-- ---------------------------------------------------------------------
DROP VIEW IF EXISTS vw_DailyTransactionSummary;
CREATE VIEW vw_DailyTransactionSummary AS
SELECT
    DATE(p.PaymentDate) AS TransactionDate,
    p.BranchId,
    COUNT(*) AS PaymentCount,
    SUM(CASE WHEN p.PaymentMethod = 'Cash' THEN p.Amount ELSE 0 END) AS CashAmount,
    SUM(CASE WHEN p.PaymentMethod = 'BankTransfer' THEN p.Amount ELSE 0 END) AS BankTransferAmount,
    SUM(CASE WHEN p.PaymentMethod = 'Card' THEN p.Amount ELSE 0 END) AS CardAmount,
    SUM(CASE WHEN p.PaymentMethod NOT IN ('Cash', 'BankTransfer', 'Card') THEN p.Amount ELSE 0 END) AS OtherPaymentAmount,
    SUM(p.Amount) AS TotalCollected
FROM Payments p
WHERE p.PaymentStatus = 'Completed'
GROUP BY DATE(p.PaymentDate), p.BranchId;
