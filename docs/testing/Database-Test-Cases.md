# Database Test Cases

Covers schema-level behavior verified against a real MySQL 8.0+ instance —
`CustomerLedger.DatabaseTests` plus the manual verification scripts under
`database/verification/`.

| Area | Test | Verified by |
|---|---|---|
| Referential integrity | FK rejects a Customer pointing at a non-existent Branch | `ReferentialIntegrityTests.Customer_WithNonExistentBranch_IsRejectedByForeignKey` |
| Referential integrity | A Branch referenced by a Customer cannot be deleted | `ReferentialIntegrityTests.Branch_CannotBeDeleted_WhilePhysicallyReferenced` |
| Unique constraints | Duplicate `BranchCode` rejected | `UniqueConstraintTests.Branch_DuplicateBranchCode_IsRejected` |
| Unique constraints | Second `CustomerAccount` for one customer rejected | `UniqueConstraintTests.CustomerAccount_SecondAccountForSameCustomer_IsRejected` |
| Index usage | Branch/status/date invoice query uses its composite index | `IndexUsageTests.InvoiceListQuery_UsesBranchStatusDateIndex` |
| CHECK constraints | Negative `InvoiceItems.Quantity`/`UnitPrice` rejected | `database/verification/VerifyConstraints.sql` (manual — MySQL 8.0.16+ required) |
| Views | All six views return expected columns and rely on underlying indexes | `database/verification/VerifyViews.sql` (manual) |
| Triggers | All eight triggers exist and fire correctly | `database/verification/VerifyTriggers.sql` (manual) |
| Schema completeness | Every expected table exists with InnoDB + utf8mb4 | `database/verification/VerifySchema.sql` (manual) |
| Seed data | `DevelopmentSeed.sql` produces the expected row counts | `database/verification/VerifySeedData.sql` (manual) |

## Why some are manual, not xUnit

Views and triggers are pure MySQL objects with no C# surface to unit test directly (a
`SELECT * FROM vw_X` in a C# test would just be re-testing EF Core's SQL execution, not the
view logic itself in any meaningfully different way from the manual SQL check). They are
verified via the SQL scripts in `database/verification/`, run manually per
[docs/labs/Views-Lab.md](../labs/Views-Lab.md) and
[docs/labs/Triggers-Lab.md](../labs/Triggers-Lab.md).
