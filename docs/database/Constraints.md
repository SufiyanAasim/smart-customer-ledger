# Constraints

## Unique constraints

| Table | Column(s) | Purpose |
|---|---|---|
| Branches | BranchCode | Unique branch lookup |
| AspNetUsers | EmployeeCode | Unique employee lookup |
| Customers | CustomerCode | Unique customer lookup |
| CustomerAccounts | CustomerId | Enforces one-to-one with Customers |
| Invoices | InvoiceNumber | Unique invoice/receipt lookup |
| Payments | PaymentNumber | Unique payment/receipt lookup |
| InstallmentPlans | InvoiceId | At most one plan per invoice |
| InstallmentSchedules | (InstallmentPlanId, InstallmentNumber) | No duplicate installment numbers within a plan |

## CHECK constraints

Defined in `database/schema/03_AlterTables.sql` and `database/constraints/CreateConstraints.sql`:

| Table | Constraint | Rule |
|---|---|---|
| InvoiceItems | CK_InvoiceItems_Quantity_Positive | Quantity > 0 |
| InvoiceItems | CK_InvoiceItems_UnitPrice_NonNegative | UnitPrice ≥ 0 |
| Payments | CK_Payments_Amount_Positive | Amount > 0 |
| InstallmentPlans | CK_InstallmentPlans_NumberOfInstallments_Positive | NumberOfInstallments > 0 |
| CustomerAccounts | CK_CustomerAccounts_CreditLimit_NonNegative | CreditLimit ≥ 0 |
| Invoices | CK_Invoices_TotalAmount_NonNegative / PaidAmount_NonNegative | Never negative |
| InstallmentSchedules | CK_InstallmentSchedules_AmountDue_Positive / AmountPaid_NonNegative | |
| InstallmentPlans | CK_InstallmentPlans_StartDate_LE_EndDate | StartDate ≤ EndDate |
| CustomerInteractions | CK_CustomerInteractions_FollowUp_After_Interaction | FollowUpDate ≥ InteractionDate when set |
| Invoices | CK_Invoices_DueDate_After_InvoiceDate | DueDate ≥ InvoiceDate when set |
| Branches | CK_Branches_Email_Format | Loose sanity check (`LIKE '%_@_%'`), not full RFC validation |

**Important caveat** (documented in the SQL files themselves): MySQL enforces CHECK
constraints from version 8.0.16 onward; earlier 8.0.x builds parse but silently ignore
them. `database/verification/VerifyConstraints.sql` includes a negative-test INSERT that
should fail — run it to confirm CHECK enforcement is actually active on your server before
relying on it.

## Application-layer validation (defense in depth, not a replacement)

Every CHECK constraint above also has a matching guard in application code — e.g.
`InvoiceCalculationService.CalculateLineTotal` throws `BusinessRuleException` for the same
quantity/price rules the database CHECK enforces. Neither layer is meant to substitute for
the other: the database constraint protects against a mistaken direct SQL write; the
application check gives the user a readable error message before a round-trip to the
database is even attempted.
