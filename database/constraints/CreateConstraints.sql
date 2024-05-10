-- =====================================================================
-- CustomerLedger — CreateConstraints.sql
-- Business-rule CHECK constraints beyond the column-level NOT NULL /
-- UNIQUE / FOREIGN KEY constraints already defined in
-- database/schema/02_CreateTables.sql and 03_AlterTables.sql. Run after
-- both of those scripts.
--
-- MySQL 8.0.16+ enforces CHECK constraints; earlier 8.0.x releases parse
-- but silently ignore them. Verify with database/verification/VerifyConstraints.sql
-- rather than assuming these are active on every deployment target.
-- =====================================================================

USE customerledger;

-- An installment plan's date range must be chronologically valid.
ALTER TABLE InstallmentPlans
    ADD CONSTRAINT CK_InstallmentPlans_StartDate_LE_EndDate CHECK (StartDate <= EndDate);

-- A follow-up can never be scheduled before the interaction that created it.
ALTER TABLE CustomerInteractions
    ADD CONSTRAINT CK_CustomerInteractions_FollowUp_After_Interaction
        CHECK (FollowUpDate IS NULL OR FollowUpDate >= InteractionDate);

-- Invoice due date, when set, cannot precede the invoice date itself.
ALTER TABLE Invoices
    ADD CONSTRAINT CK_Invoices_DueDate_After_InvoiceDate
        CHECK (DueDate IS NULL OR DueDate >= InvoiceDate);

-- A branch's contact email, when provided, has to at least contain an '@'.
-- Deliberately loose — full RFC validation belongs in the application layer
-- (see BranchFormViewModel's [EmailAddress] attribute), this is only a
-- last-resort guard against obviously malformed direct SQL writes.
ALTER TABLE Branches
    ADD CONSTRAINT CK_Branches_Email_Format CHECK (Email IS NULL OR Email LIKE '%_@_%');
