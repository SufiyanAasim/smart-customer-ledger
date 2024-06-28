# Database Setup Guide

## Option A — EF Core migration (recommended, always in sync with the C# model)

```bash
mysql -u root -p < database/schema/01_CreateDatabase.sql   # creates the DB + app user
dotnet ef database update \
  --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
```

This single command creates every table (business + ASP.NET Core Identity) from the EF
Core model. It does **not** create the six views or the eight triggers — those are pure SQL
objects with no EF Core equivalent.

## Option B — Manual SQL Workbench walkthrough (for manual inspection/demonstration)

Run these files, **in this exact order**, against a database EF has not yet touched:

```bash
mysql -u root -p < database/schema/01_CreateDatabase.sql
mysql -u root -p customerledger < database/schema/02_CreateTables.sql
mysql -u root -p customerledger < database/schema/03_AlterTables.sql
mysql -u root -p customerledger < database/constraints/CreateConstraints.sql
mysql -u root -p customerledger < database/indexes/CreateIndexes.sql
mysql -u root -p customerledger < database/views/CreateViews.sql
mysql -u root -p customerledger < database/triggers/CreateTriggers.sql
```

Do not run both Option A and Option B against the same database — Option A's EF migration
already creates every table Option B's `02_CreateTables.sql` would try to create again.

## After either option — views and triggers

Regardless of which schema-creation path you used, always run these (they are not part of
the EF Core migration):

```bash
mysql -u root -p customerledger < database/views/CreateViews.sql
mysql -u root -p customerledger < database/triggers/CreateTriggers.sql
```

## Seeding sample data

```bash
dotnet run --project src/CustomerLedger.Web   # seeds roles + admin (if SeedAdmin:* configured)
mysql -u root -p customerledger < database/seed/DevelopmentSeed.sql
mysql -u root -p customerledger < database/seed/DemonstrationSeed.sql
```

## Verifying the setup

```bash
mysql -u root -p customerledger < database/verification/VerifySchema.sql
mysql -u root -p customerledger < database/verification/VerifyConstraints.sql
mysql -u root -p customerledger < database/verification/VerifyViews.sql
mysql -u root -p customerledger < database/verification/VerifyTriggers.sql
mysql -u root -p customerledger < database/verification/VerifySeedData.sql
```

## Resetting for a clean re-demo

```bash
dotnet ef database drop \
  --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web --force
dotnet ef database update \
  --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
mysql -u root -p customerledger < database/views/CreateViews.sql
mysql -u root -p customerledger < database/triggers/CreateTriggers.sql
```
