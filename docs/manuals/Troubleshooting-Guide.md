# Troubleshooting Guide

| Symptom | Likely Cause | Fix |
|---|---|---|
| `Connection string 'DefaultConnection' not found` at startup | User secrets not configured | Run the `dotnet user-secrets set` commands in [Configuration-Guide.md](Configuration-Guide.md) |
| App starts but every page 500s on first load | Migration not applied | `dotnet ef database update --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web` |
| Can't sign in with the account you expect | No `SeedAdmin:*` configured, so no admin was seeded | Set `SeedAdmin:Email`/`SeedAdmin:Password` in user secrets and restart the app once against a fresh database |
| "Invalid login attempt" for a known-correct password | Account `IsActive = false`, or 5 recent failed attempts triggered a lockout | An Administrator must reactivate the account under Admin → Users; wait out the lockout window otherwise |
| `dotnet ef` commands fail with "No executable found" | `dotnet-ef` tool not installed | `dotnet tool install --global dotnet-ef` |
| `dotnet ef migrations add` produces a non-empty migration when you expected no changes | A Fluent API configuration or entity was edited without intending a schema change | Review the generated `Up`/`Down` — if unintended, revert the entity/config change and re-run; if intended, this is a real, legitimate migration |
| Backup always reports Failed | `mysqldump` not on PATH | Set `BackupSettings:MysqldumpPath` to the full binary path |
| Restore does nothing | Confirmation text wasn't exactly `RESTORE`, or the backup file was moved/deleted from `BackupSettings:Directory` | Retype `RESTORE` exactly; confirm the file still exists on disk |
| CSV import rejects every row | Missing a required header column (CustomerCode, FullName, PhoneNumber, Address, City) | Fix the CSV header — see [Import-Export-Lab.md](../labs/Import-Export-Lab.md) |
| A test class reports "Skipped" instead of running | No MySQL reachable via `CUSTOMERLEDGER_TEST_CONNECTION`/`ConnectionStrings__DefaultConnection` | Point one of those at a real MySQL 8.0+ instance |
| `EXPLAIN` shows `key = NULL` for a query you expect to use an index | The index doesn't exist yet, or MySQL's optimizer chose a full scan for a small table (expected on tiny demo datasets) | Run `database/indexes/CreateIndexes.sql`; re-test after `LargeDatasetSeed.sql` for a more realistic row count |
| CHECK constraint doesn't reject invalid data | MySQL server predates 8.0.16 (CHECK parsed but not enforced) | Upgrade to MySQL 8.0.16+, or rely on the application-layer validation, which is not version-dependent |
| Branch reassignment for a logged-in user doesn't take effect | Branch is carried as a login-time claim, not re-read per request | The user must sign out and back in |

## Getting more detail

Application logs (structured, via `ILogger<T>`) are the first place to look for an
unhandled exception's real message — the UI deliberately shows a generic, user-safe error
instead of exposing internals. Check the console output where `dotnet run` is executing, or
your configured logging sink in production.
