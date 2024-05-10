-- =====================================================================
-- CustomerLedger — 03_AlterTables.sql
-- Post-creation ALTER TABLE statements. Empty at the initial schema
-- version; each later release appends its own ALTER TABLE block below
-- (never edits 02_CreateTables.sql after it has shipped), so this file
-- becomes the running history of schema evolution outside of EF Core
-- migrations. Run once, in order, after 02_CreateTables.sql.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- v1.0.0 — Index: defense-in-depth CHECK constraints.
-- The application (InvoiceCalculationService, service-layer validation)
-- already rejects these values — these constraints exist so a mistaken
-- direct SQL UPDATE/INSERT against the database cannot silently corrupt
-- financial data, per spec section 8 ("also validate important rules in
-- application services... use triggers where database-level protection
-- is necessary").
-- ---------------------------------------------------------------------

ALTER TABLE CustomerAccounts
    ADD CONSTRAINT CK_CustomerAccounts_CreditLimit_NonNegative CHECK (CreditLimit >= 0);

ALTER TABLE Invoices
    ADD CONSTRAINT CK_Invoices_TotalAmount_NonNegative CHECK (TotalAmount >= 0),
    ADD CONSTRAINT CK_Invoices_PaidAmount_NonNegative CHECK (PaidAmount >= 0);

ALTER TABLE InstallmentSchedules
    ADD CONSTRAINT CK_InstallmentSchedules_AmountDue_Positive CHECK (AmountDue > 0),
    ADD CONSTRAINT CK_InstallmentSchedules_AmountPaid_NonNegative CHECK (AmountPaid >= 0);
