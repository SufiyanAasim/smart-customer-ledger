-- =====================================================================
-- CustomerLedger — VerifySeedData.sql
-- Confirms DevelopmentSeed.sql produced the expected rows.
-- =====================================================================

USE customerledger;

SELECT COUNT(*) AS branch_count FROM Branches;
SELECT COUNT(*) AS customer_count FROM Customers WHERE IsDeleted = 0;
SELECT COUNT(*) AS invoice_count FROM Invoices WHERE IsDeleted = 0;

SELECT BranchCode, Name, City FROM Branches ORDER BY BranchCode;
SELECT CustomerCode, FullName, PhoneNumber FROM Customers WHERE IsDeleted = 0 ORDER BY CustomerCode;
SELECT InvoiceNumber, TotalAmount, OutstandingAmount, InvoiceStatus FROM Invoices ORDER BY InvoiceNumber;
