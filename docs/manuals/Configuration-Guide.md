# Configuration Guide

All configuration keys, with which release introduced them and where they should live.
**Never** put real values in `appsettings.json` or `appsettings.Development.json` — use
user secrets (development) or environment variables (production/CI).

## Setting up user secrets

```bash
cd src/CustomerLedger.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=customerledger;Uid=customerledger_app;Pwd=<your-password>;"
dotnet user-secrets set "SeedAdmin:Email" "admin@customerledger.local"
dotnet user-secrets set "SeedAdmin:Password" "<a-strong-password>"
```

## Configuration keys

| Key | Introduced | Required? | Purpose |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | Index | Yes | MySQL connection string |
| `MySqlServerVersion` | Index | No (defaults to 8.0.36) | Avoids a live-connection version probe at startup |
| `SeedAdmin:Email` / `SeedAdmin:Password` | Index | No (skips admin seeding if absent) | First Administrator account |
| `SeedAdmin:FullName` / `SeedAdmin:EmployeeCode` | Index | No | Cosmetic defaults for the seeded admin |
| `BackupSettings:Directory` | Snapshot | No (defaults to `App_Data/Backups`) | Where `mysqldump` output is written |
| `BackupSettings:MysqldumpPath` | Snapshot | No (defaults to `mysqldump` on PATH) | Override if the binary isn't on PATH |
| `BackupSettings:MysqlClientPath` | Snapshot | No (defaults to `mysql` on PATH) | Override for the restore client |

See `src/CustomerLedger.Web/appsettings.Example.json` for the full example file with every
key above.

## Environment-variable equivalents (for CI / containerized deployment)

ASP.NET Core's configuration system maps `:` to `__` for environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Port=3306;Database=customerledger;Uid=...;Pwd=...;"
export SeedAdmin__Email="admin@customerledger.local"
export SeedAdmin__Password="..."
```

## Test configuration

`CustomerLedger.DatabaseTests`/`IntegrationTests` read `CUSTOMERLEDGER_TEST_CONNECTION`
first, falling back to `ConnectionStrings__DefaultConnection` — see
`tests/CustomerLedger.DatabaseTests/TestDatabaseSettings.cs`. Point this at a disposable
test database, never at production data (tests call `Database.EnsureDeletedAsync()`).
