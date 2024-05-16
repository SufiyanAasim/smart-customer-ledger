# Seed Data

| Script | Purpose | Idempotent? |
|---|---|---|
| `RoleSeeder` / `AdminUserSeeder` (C#, run at app startup) | Seeds the three roles and (if `SeedAdmin:*` is configured) the first Administrator account | Yes — checks existence first |
| `DevelopmentDataSeeder` (C#, Development environment only) | Adds one "Main Branch" if no branch exists | Yes |
| `database/seed/DevelopmentSeed.sql` | A minimal demonstration branch + customer + invoice, layered on whatever the C# seeders created | Yes — every INSERT is guarded by `WHERE NOT EXISTS` |
| `database/seed/DemonstrationSeed.sql` | A coherent two-branch, five-customer story: fully paid, partially paid, overdue-unpaid, and installment-plan invoices, plus two customer interactions | Yes |
| `database/seed/LargeDatasetSeed.sql` | A bounded, explicitly-reported volume of synthetic customers/invoices (2,000 by default) in a dedicated `PERF-TEST` branch | Yes — the generating loop checks for each `CustomerCode` before inserting |

## Running order

1. Run the application once against a fresh database (applies the EF Core migration and
   seeds roles/admin via C#).
2. Optionally run `DevelopmentSeed.sql` and/or `DemonstrationSeed.sql` for a small, readable
   dataset.
3. Optionally run `LargeDatasetSeed.sql` for `EXPLAIN`/index-usage demonstrations that need
   real row counts.

All three SQL seed scripts require at least one `AspNetUsers` row to already exist (they
look it up with `SELECT Id FROM AspNetUsers ORDER BY CreatedAtUtc LIMIT 1`), which is why
step 1 must run first.

## Why idempotent, not "run once and forget"

Every seed script can be re-run safely against a database that already has some or all of
its rows — each INSERT checks `WHERE NOT EXISTS` (or, for the large dataset, an explicit
per-row existence check inside the generation loop) before writing. This matters for a
grading/demo environment that may need to be reset and reseeded more than once.
