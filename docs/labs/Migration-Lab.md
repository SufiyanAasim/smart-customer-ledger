# Lab: Migrations

**Goal**: apply, inspect, and (carefully) roll back an EF Core migration.

## Applying the existing migration

```bash
dotnet ef database update \
  --project src/CustomerLedger.Infrastructure \
  --startup-project src/CustomerLedger.Web
```

Confirm with `docs/database/Database-Dictionary.md`'s table list that every expected table
now exists (`database/verification/VerifySchema.sql` also confirms this via SQL).

## Adding a new migration (demonstration only — do not commit unless intentional)

1. Make a trivial, reversible model change (e.g. add a `[MaxLength(500)]` somewhere, or add
   a temporary property to a demo entity).
2. Run:

   ```bash
   dotnet ef migrations add DemoLabMigration \
     --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
   ```

3. Inspect the generated file under `src/CustomerLedger.Infrastructure/Data/Migrations/` —
   confirm the `Up`/`Down` methods match the change you made.
4. Apply it: `dotnet ef database update ...` (same command as above).
5. Roll it back:

   ```bash
   dotnet ef database update <PreviousMigrationName> \
     --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
   ```

6. Remove the unused migration file:

   ```bash
   dotnet ef migrations remove \
     --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
   ```

7. Revert your step 1 model change.

## Confirming no pending model changes (used throughout this project's release process)

```bash
dotnet ef migrations add CheckForPendingChanges \
  --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
```

If the generated migration's `Up`/`Down` methods are empty, the current code matches the
last migration exactly — delete the generated files and move on. This exact technique was
used at the end of both the Balance and Snapshot releases to confirm neither changed the
schema.

## Database reset (development only — destroys all data)

```bash
dotnet ef database drop --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web --force
dotnet ef database update --project src/CustomerLedger.Infrastructure --startup-project src/CustomerLedger.Web
```

## Naming convention

Migrations use a descriptive PascalCase name of what changed (`InitialCreate`,
`AddCustomerAccounts`, `AddPaymentModule`) — never `Test`, `Fix`, or `NewMigration`. This
project has exactly one migration so far (`InitialCreate`), since Balance and Snapshot both
added only business logic and SQL scripts, not schema changes.

## Rollback limitations

EF Core's `Down` method can undo a schema change, but it cannot undo data already written
under the new schema (e.g. if a column was dropped and its data lost). Always back up
(`database/backup/` workflow, see `docs/labs/Backup-Restore-Lab.md`) before rolling back a
migration against a database with real data.
