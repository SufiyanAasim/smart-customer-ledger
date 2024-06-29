-- =====================================================================
-- CustomerLedger — CreateAnalyticsViews.sql (v7.0.0 — Capital)
-- An additional, non-required view supporting the analytics/data-mining
-- release — separate from the six views mandated by the original
-- project specification (database/views/CreateViews.sql), which remain
-- unchanged. This view exposes exactly the raw feature values
-- CustomerRiskScoringService computes in C# (CreditUtilization,
-- UnpaidInvoiceRatio, AverageInvoiceAmount, TotalOutstanding,
-- CustomerAgeDays), so the same feature set can be inspected directly
-- in MySQL Workbench without needing to run the application.
-- =====================================================================

USE customerledger;

DROP VIEW IF EXISTS vw_CustomerRiskFeatures;
CREATE VIEW vw_CustomerRiskFeatures AS
SELECT
    c.CustomerId,
    c.CustomerCode,
    c.FullName AS CustomerName,
    c.BranchId,
    CASE WHEN a.CreditLimit > 0 THEN a.CurrentBalance / a.CreditLimit ELSE 0 END AS CreditUtilization,
    COALESCE(inv.UnpaidRatio, 0) AS UnpaidInvoiceRatio,
    COALESCE(inv.AverageInvoiceAmount, 0) AS AverageInvoiceAmount,
    COALESCE(inv.TotalOutstanding, 0) AS TotalOutstanding,
    DATEDIFF(UTC_TIMESTAMP(), c.RegistrationDate) AS CustomerAgeDays,
    -- The same heuristic training label CustomerRiskScoringService uses —
    -- exposed here so its derivation is fully visible in SQL, not hidden in C#.
    (EXISTS (
        SELECT 1 FROM InstallmentSchedules s
        JOIN InstallmentPlans p ON p.InstallmentPlanId = s.InstallmentPlanId
        JOIN Invoices i2 ON i2.InvoiceId = p.InvoiceId
        WHERE i2.CustomerId = c.CustomerId AND s.Status = 'Overdue'
    ) OR EXISTS (
        SELECT 1 FROM Payments pay WHERE pay.CustomerId = c.CustomerId AND pay.PaymentStatus = 'Reversed'
    )) AS IsHeuristicHighRiskLabel
FROM Customers c
LEFT JOIN CustomerAccounts a ON a.CustomerId = c.CustomerId
LEFT JOIN (
    SELECT
        CustomerId,
        COUNT(*) AS TotalInvoices,
        SUM(CASE WHEN PaymentStatus <> 'Paid' THEN 1 ELSE 0 END) / COUNT(*) AS UnpaidRatio,
        AVG(TotalAmount) AS AverageInvoiceAmount,
        SUM(OutstandingAmount) AS TotalOutstanding
    FROM Invoices
    WHERE IsDeleted = 0
    GROUP BY CustomerId
) AS inv ON inv.CustomerId = c.CustomerId
WHERE c.IsDeleted = 0 AND c.Status = 'Active';
