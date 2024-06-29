-- =====================================================================
-- CustomerLedger — InstallmentPlans_CRUD.sql
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT (header only — schedule rows generated separately, see
-- InstallmentSchedules_CRUD.sql; mirrors InstallmentPlanService.CreateAsync)
-- ---------------------------------------------------------------------
INSERT INTO InstallmentPlans (
    InvoiceId, NumberOfInstallments, TotalInstallmentAmount, DownPayment,
    StartDate, EndDate, Frequency, Status, CreatedAtUtc
) VALUES (
    ?, ?, ?, ?,
    ?, ?, ?, 'PendingApproval', UTC_TIMESTAMP(6)
);

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT InstallmentPlanId, InvoiceId, NumberOfInstallments, TotalInstallmentAmount,
       DownPayment, StartDate, EndDate, Frequency, Status, ApprovedByUserId
FROM InstallmentPlans
WHERE InstallmentPlanId = ?;

-- ---------------------------------------------------------------------
-- SELECT by invoice (one-to-one lookup)
-- ---------------------------------------------------------------------
SELECT InstallmentPlanId, NumberOfInstallments, TotalInstallmentAmount, Status
FROM InstallmentPlans
WHERE InvoiceId = ?;

-- ---------------------------------------------------------------------
-- SELECT list: plans pending approval for a branch (JOIN to Invoices)
-- ---------------------------------------------------------------------
SELECT p.InstallmentPlanId, i.InvoiceNumber, p.TotalInstallmentAmount, p.NumberOfInstallments
FROM InstallmentPlans p
JOIN Invoices i ON i.InvoiceId = p.InvoiceId
WHERE i.BranchId = ? AND p.Status = 'PendingApproval'
ORDER BY p.CreatedAtUtc;

-- ---------------------------------------------------------------------
-- UPDATE (while Pending only)
-- ---------------------------------------------------------------------
UPDATE InstallmentPlans
SET NumberOfInstallments = ?,
    DownPayment = ?,
    StartDate = ?,
    EndDate = ?,
    Frequency = ?,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE InstallmentPlanId = ? AND Status = 'PendingApproval';

-- ---------------------------------------------------------------------
-- Approve (restricted to Administrator/Branch Manager in the application)
-- ---------------------------------------------------------------------
UPDATE InstallmentPlans
SET Status = 'Active', ApprovedByUserId = ?, UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE InstallmentPlanId = ? AND Status = 'PendingApproval';

-- ---------------------------------------------------------------------
-- Cancel instead of destructive delete.
-- ---------------------------------------------------------------------
UPDATE InstallmentPlans
SET Status = 'Cancelled', UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE InstallmentPlanId = ?;
