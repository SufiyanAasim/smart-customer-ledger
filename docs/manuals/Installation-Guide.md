# Installation Guide

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server 8.0+
- `mysqldump`/`mysql` client tools on PATH (only needed for backup/restore, v3.0.0+)
- (Optional) `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

## Steps

1. **Clone the repository** and restore packages:

   ```bash
   git clone <this-repository-url>
   cd "Smart Customer Ledger"
   dotnet restore
   ```

2. **Create the database** — see
   [Database-Setup-Guide.md](Database-Setup-Guide.md) for the full walkthrough. Short
   version:

   ```bash
   mysql -u root -p < database/schema/01_CreateDatabase.sql
   ```

3. **Configure secrets** — see [Configuration-Guide.md](Configuration-Guide.md).

4. **Apply the EF Core migration**:

   ```bash
   dotnet ef database update \
     --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
   ```

5. **Run the application**:

   ```bash
   dotnet run --project src/CustomerLedger.Web
   ```

6. Navigate to the URL printed in the console (typically `https://localhost:5001` or
   similar) and sign in with the `SeedAdmin` credentials you configured.

## Verifying the installation

```bash
dotnet build   # expect: Build succeeded, 0 Warning(s), 0 Error(s)
dotnet test    # unit tests pass unconditionally; DB tests require CUSTOMERLEDGER_TEST_CONNECTION
```

See [Troubleshooting-Guide.md](Troubleshooting-Guide.md) if anything fails.
