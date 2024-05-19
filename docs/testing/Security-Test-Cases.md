# Security Test Cases

| ID | Area | Test | Expected Result | Status |
|---|---|---|---|---|
| SEC-01 | SQL injection | Every statement in `database/crud/*.sql` uses `?` placeholders | No string-concatenated SQL found | Verified by inspection — see [Parameterized-Queries-Lab.md](../labs/Parameterized-Queries-Lab.md) |
| SEC-02 | SQL injection | Every EF Core query in `src/CustomerLedger.Infrastructure/Services/*.cs` is LINQ (auto-parameterized) or `FromSqlInterpolated` (safely parameterized) | No raw string SQL concatenation | Verified by inspection |
| SEC-03 | Branch isolation | Staff of Branch A requests a Branch B customer/invoice/payment by URL id | 403 Forbidden, not the record | Automated — `CustomerServiceTests.GetByIdAsync_FromAnotherBranch_...`, `PaymentServiceTests.RecordPaymentAsync_FromDifferentBranch_...` |
| SEC-04 | Authorization | Non-Administrator/Branch-Manager attempts to reverse a payment | Blocked by `[Authorize(Roles=...)]` before the action runs | Manual — attempt as Staff, confirm 403 |
| SEC-05 | Authorization | Non-Administrator attempts to reach `/Admin/*` routes | Blocked by `[Authorize(Roles = Roles.Administrator)]` | Manual |
| SEC-06 | CSRF | Submit a state-changing form without the anti-forgery token | 400 Bad Request | Manual — strip `__RequestVerificationToken` from a POST and resubmit |
| SEC-07 | Credential handling | Search the codebase for any logging of `PasswordHash`, `SecurityStamp`, or plaintext passwords | None found | Verified by inspection |
| SEC-08 | Formula injection | Export a customer whose name is `=1+1` to CSV, open in Excel | Displays literal text, does not execute as a formula | Automated (unit-level) — `CsvUtilitiesTests.EscapeField_NeutralizesFormulaInjectionCharacters`; manual (Excel-level) — [Import-Export-Lab.md](../labs/Import-Export-Lab.md) |
| SEC-09 | Mass assignment | Submit extra/unexpected form fields (e.g. `IsDeleted=true`) to a Create/Edit action | Ignored — ViewModels only bind their declared properties, never raw entities | Verified by inspection of every Controller/ViewModel pair |
| SEC-10 | Secret handling | `appsettings.json` and `appsettings.Development.json` contain no real credentials | Confirmed empty connection string / no `SeedAdmin` values committed | Verified by inspection |
| SEC-11 | Process credential exposure | Backup/restore password is not visible via `ps`/Task Manager while `mysqldump`/`mysql` run | Passed via `MYSQL_PWD` environment variable, not a CLI argument | Verified by inspection of `MySqlBackupService`/`MySqlRestoreService` |
| SEC-12 | Destructive action friction | Restore requires typing the literal word `RESTORE` in addition to CSRF | Restore blocked if text doesn't match | Manual — [Backup-Restore-Lab.md](../labs/Backup-Restore-Lab.md) |
| SEC-13 | Lockout | 5 consecutive failed logins for one account | Account locked out for the configured `DefaultLockoutTimeSpan` | Manual — attempt 5 wrong passwords, confirm the 6th is rejected with a lockout message |
| SEC-14 | Self-registration | Attempt to reach a public registration endpoint | None exists — accounts are Administrator-created only | Verified by inspection (no `[AllowAnonymous]` Register action anywhere) |

## Verified vs. not yet automated

SEC-01, -02, -07, -09, -10, -11, -14 are structural properties verified by direct code
inspection (there is no meaningful way to "test" the absence of a vulnerable pattern other
than confirming it isn't there). SEC-04, -05, -06, -08 (Excel-level), -12, -13 require
manual verification in a running instance with a real database — track their execution in
[Evidence-Checklist.md](Evidence-Checklist.md).
