# Test Cases (Detailed)

Format: ID, Description, Preconditions, Steps, Expected Result, Automated? (Y/N + test name).

| ID | Description | Preconditions | Steps | Expected Result | Automated |
|---|---|---|---|---|---|
| TC-01 | Line total calculation | — | Compute (Qty=3, Price=100, Discount=20, Tax=10) | 290 | Y — `CalculateLineTotal_ComputesGrossMinusDiscountPlusTax` |
| TC-02 | Reject zero/negative quantity | — | Compute with Qty=0 and Qty=-1 | `BusinessRuleException` | Y — `CalculateLineTotal_RejectsNonPositiveQuantity` |
| TC-03 | Admin can access any branch | Admin claim | `CanAccessBranch(anyId)` | true | Y — `CanAccessBranch_Administrator_CanAccessAnyBranch` |
| TC-04 | Staff limited to own branch | BranchId=5 claim | `CanAccessBranch(5)` / `CanAccessBranch(6)` | true / false | Y — `CanAccessBranch_BranchManager_CanOnlyAccessOwnBranch` |
| TC-05 | CSV formula injection neutralized | — | Escape `"=SUM(A1:A2)"` | `"'=SUM(A1:A2)"` | Y — `EscapeField_NeutralizesFormulaInjectionCharacters` |
| TC-06 | Customer registration creates linked account | Branch exists | `CustomerService.CreateAsync(..., creditLimit: 10000)` | `CustomerAccount` exists with `CreditLimit=10000` | Y — `CreateAsync_RegistersCustomerAndCreatesLinkedAccount` |
| TC-07 | Duplicate CustomerCode rejected | A customer with code X exists | Create another with code X | `BusinessRuleException` | Y — `CreateAsync_DuplicateCustomerCode_ThrowsBusinessRuleException` |
| TC-08 | Cross-branch customer create rejected | Current user scoped to Branch A | Create customer for Branch B | `BranchAccessDeniedException` | Y — `CreateAsync_ForDifferentBranchThanCurrentUser_ThrowsBranchAccessDeniedException` |
| TC-09 | Cross-branch customer read rejected | Customer in Branch B | Staff of Branch A calls `GetByIdAsync` | `BranchAccessDeniedException` | Y — `GetByIdAsync_FromAnotherBranch_ThrowsBranchAccessDeniedException` |
| TC-10 | Invoice draft totals | Customer + branch exist | `CreateDraftAsync` with 1 item (Qty=2,Price=100,Discount=10,Tax=5) | `TotalAmount=195` | Y — `CreateDraftAsync_CalculatesTotalsFromItems` |
| TC-11 | Cannot activate empty invoice | Draft invoice, no items | `ActivateAsync` | `BusinessRuleException` | Y — `ActivateAsync_WithNoItems_ThrowsBusinessRuleException` |
| TC-12 | Cannot invoice an inactive customer | Customer.Status=Inactive | `CreateDraftAsync` | `BusinessRuleException` | Y — `CreateDraftAsync_ForInactiveCustomer_ThrowsBusinessRuleException` |
| TC-13 | Full payment marks Paid | Active invoice, Outstanding=1000 | Pay 1000 | `PaymentStatus=Paid`, `Outstanding=0` | Y — `RecordPaymentAsync_FullPayment_MarksInvoicePaid` |
| TC-14 | Partial payment marks PartiallyPaid | Active invoice, Outstanding=1000 | Pay 400 | `PaymentStatus=PartiallyPaid`, `Outstanding=600` | Y — `RecordPaymentAsync_PartialPayment_MarksInvoicePartiallyPaid` |
| TC-15 | Zero payment rejected | Active invoice | Pay 0 | `BusinessRuleException` | Y — `RecordPaymentAsync_ZeroAmount_ThrowsBusinessRuleException` |
| TC-16 | Overpayment rejected | Active invoice, Outstanding=1000 | Pay 1500 | `BusinessRuleException` | Y — `RecordPaymentAsync_Overpayment_ThrowsBusinessRuleException` |
| TC-17 | Payment against cancelled invoice rejected | Invoice.Status=Cancelled | Pay 100 | `BusinessRuleException` | Y — `RecordPaymentAsync_AgainstCancelledInvoice_ThrowsBusinessRuleException` |
| TC-18 | Cross-branch payment rejected | Invoice in Branch A | Staff of Branch B pays it | `BranchAccessDeniedException` | Y — `RecordPaymentAsync_FromDifferentBranch_ThrowsBranchAccessDeniedException` |
| TC-19 | Reversal restores balances | Payment of 400 recorded | `ReverseAsync(paymentId, reason)` | Invoice/account balances restored | Y — `ReverseAsync_RestoresInvoiceAndAccountBalances` |
| TC-20 | Double reversal rejected | Payment already reversed | `ReverseAsync` again | `BusinessRuleException` | Y — `ReverseAsync_CalledTwice_ThrowsBusinessRuleException` |
| TC-21 | Reversal without reason rejected | Completed payment | `ReverseAsync(id, "")` | `BusinessRuleException` | Y — `ReverseAsync_WithoutReason_ThrowsBusinessRuleException` |
| TC-22 | Installment payment marks row Paid | Active plan, Pending schedule row | `PayInstallmentAsync(full amount)` | Row `Status=Paid`, `PaidDate` set | Y — `PayInstallmentAsync_FullyPaidRow_MarksScheduleAndPlanComplete` |
| TC-23 | Plan completes when all rows settled | Last Pending row paid | Pay final row | Plan `Status=Completed` | Y — same test as TC-22 |
| TC-24 | Reconciliation corrects drift | Account totals deliberately wrong | `ReconcileCustomerAccountAsync` | Totals corrected, `HadMismatch=true` | Y — `ReconcileCustomerAccountAsync_CorrectsDriftedTotals` |
| TC-25 | Reconciliation no-op when correct | Account already correct | `ReconcileCustomerAccountAsync` | `HadMismatch=false`, nothing changed | Y — `ReconcileCustomerAccountAsync_WhenAlreadyCorrect_ReportsNoMismatch` |
| TC-26 | Concurrent overpayment prevented | Invoice Outstanding=1000 | Two connections each pay 700 simultaneously | Exactly one succeeds; final `PaidAmount=700` | Y — `TwoConcurrentPayments_ThatWouldJointlyOverpay_OnlyOneSucceeds` |
| TC-27 | Missing mysqldump recorded as Failed | Bogus `MysqldumpPath` configured | `CreateBackupAsync` | `Status=Failed`, `ErrorMessage` set | Y — `CreateBackupAsync_WithMissingMysqldumpBinary_RecordsFailedNotCompleted` |
| TC-28 | Anonymous request redirected to Login | App running | `GET /` unauthenticated | 302 → `/Identity/Account/Login` | Y — `AnonymousRequestToDashboard_RedirectsToLogin` |
| TC-29 | Foreign key rejects invalid branch | — | Insert Customer with `BranchId=999999` | `DbUpdateException` | Y — `Customer_WithNonExistentBranch_IsRejectedByForeignKey` |
| TC-30 | Branch deletion blocked while referenced | Branch has a customer | `DELETE FROM Branches` | `DbUpdateException` | Y — `Branch_CannotBeDeleted_WhilePhysicallyReferenced` |
| TC-31 | Duplicate BranchCode rejected | Branch "DUP-TEST" exists | Insert another with same code | `DbUpdateException` | Y — `Branch_DuplicateBranchCode_IsRejected` |
| TC-32 | Second CustomerAccount rejected | Customer already has an account | Insert a second `CustomerAccount` | `DbUpdateException` | Y — `CustomerAccount_SecondAccountForSameCustomer_IsRejected` |
| TC-33 | Invoice list query uses composite index | — | `EXPLAIN` the branch/status/date query | `key` names the composite index | Y — `InvoiceListQuery_UsesBranchStatusDateIndex` |

See [Requirements-Traceability-Matrix.md](Requirements-Traceability-Matrix.md) for which
specification requirement each TC maps to.
