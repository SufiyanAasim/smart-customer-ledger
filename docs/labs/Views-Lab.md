# Lab: Views

**Goal**: create the six required views, query them, and prove they rely on underlying
indexes rather than full table scans.

## Steps

1. Run `database/views/CreateViews.sql` against a database that has
   `DemonstrationSeed.sql` applied.
2. Run each of the six `SELECT * FROM vw_...` statements in
   `database/verification/VerifyViews.sql` and compare the output columns against the
   "Expected columns" comment above each one.
3. Run the two `EXPLAIN` statements at the bottom of `VerifyViews.sql`. Confirm the `key`
   column names an actual index (e.g. `IX_Invoices_CustomerId_PaymentStatus`), not `NULL`.
4. Modify `vw_OverdueInstallments`'s underlying query by hand (in a scratch query, not the
   real view) to remove the `WHERE Status = 'Pending'` predicate, run `EXPLAIN` again, and
   observe the `rows` estimate grow — this demonstrates why the predicate (and its
   supporting index) matters.
5. Drop the views with `database/views/DropViews.sql` and re-create them with
   `CreateViews.sql` to confirm the drop/recreate cycle is clean (no leftover dependent
   objects).

## Expected outcomes

- All six views return sensible data immediately after `DemonstrationSeed.sql`.
- `EXPLAIN` shows index usage for the two representative queries.
- Dropping and recreating the views is idempotent and produces no errors.

## Automated coverage

None — MySQL views have no first-class xUnit testing story in this project; they are
verified via the SQL scripts above. `IndexUsageTests` (C#) tests the underlying index usage
directly, without going through a view.
