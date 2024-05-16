# Indexes

Full list with the query pattern each one supports — source of truth:
`database/indexes/CreateIndexes.sql`.

| Index | Table | Columns | Supports |
|---|---|---|---|
| UQ_Branches_BranchCode | Branches | BranchCode | Unique branch lookup |
| UQ_AspNetUsers_EmployeeCode | AspNetUsers | EmployeeCode | Unique employee lookup |
| IX_AspNetUsers_BranchId_IsActive | AspNetUsers | BranchId, IsActive | "Active staff for this branch" |
| UQ_Customers_CustomerCode | Customers | CustomerCode | Unique customer lookup |
| IX_Customers_PhoneNumber | Customers | PhoneNumber | Search-by-phone |
| IX_Customers_CNIC | Customers | CNIC | Search-by-CNIC |
| IX_Customers_BranchId_Status_IsDeleted | Customers | BranchId, Status, IsDeleted | Every customer list screen's WHERE clause |
| UQ_CustomerAccounts_CustomerId | CustomerAccounts | CustomerId | One-to-one lookup |
| UQ_Invoices_InvoiceNumber | Invoices | InvoiceNumber | Unique invoice/receipt lookup |
| IX_Invoices_CustomerId_PaymentStatus | Invoices | CustomerId, PaymentStatus | Customer statement / outstanding list |
| IX_Invoices_BranchId_InvoiceDate | Invoices | BranchId, InvoiceDate | Branch revenue reports by date |
| IX_Invoices_BranchId_InvoiceStatus_InvoiceDate | Invoices | BranchId, InvoiceStatus, InvoiceDate | Invoice list screen filter+sort |
| UQ_Payments_PaymentNumber | Payments | PaymentNumber | Unique payment/receipt lookup |
| IX_Payments_InvoiceId_PaymentStatus | Payments | InvoiceId, PaymentStatus | Invoice detail's payment history |
| IX_Payments_CustomerId_PaymentDate | Payments | CustomerId, PaymentDate | Customer statement |
| IX_Payments_BranchId_PaymentDate | Payments | BranchId, PaymentDate | `vw_DailyTransactionSummary` |
| IX_InstallmentSchedules_Status_DueDate | InstallmentSchedules | Status, DueDate | `vw_OverdueInstallments`, overdue sweep |
| IX_CustomerInteractions_CustomerId_InteractionDate | CustomerInteractions | CustomerId, InteractionDate | Interaction history |
| IX_AuditLogs_TableName_RecordId | AuditLogs | TableName, RecordId | "Audit trail for this record" |
| IX_AuditLogs_BranchId_CreatedAtUtc | AuditLogs | BranchId, CreatedAtUtc | Admin audit review screen |

## Verifying index usage

`database/indexes/VerifyIndexes.sql` and `database/verification/VerifyViews.sql` both
include `EXPLAIN` statements against representative queries. A properly working index shows
up in the `key` column of `EXPLAIN`'s output (or at least in `possible_keys`); a full table
scan shows `type = ALL` and `key = NULL`. `IndexUsageTests.InvoiceListQuery_UsesBranchStatusDateIndex`
automates exactly this check for the invoice list query.

## What indexes cannot do

MySQL views (see [Views.md](Views.md)) cannot be indexed directly — the six reporting views
are only as fast as the indexes on their underlying tables, listed above.
