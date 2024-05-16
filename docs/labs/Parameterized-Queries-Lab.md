# Lab: Parameterized Queries and SQL Injection

**Goal**: see why every query in this project uses parameters, by comparing against the
vulnerable alternative.

## The vulnerable pattern (do not run against a real database — for illustration only)

```csharp
// NEVER DO THIS:
string sql = "SELECT * FROM Customers WHERE PhoneNumber = '" + phoneNumber + "'";
```

If `phoneNumber` were user input containing `' OR '1'='1`, the resulting query becomes:

```sql
SELECT * FROM Customers WHERE PhoneNumber = '' OR '1'='1'
```

— returning every customer row, not just the one matching the intended phone number. Worse
inputs (e.g. `'; DROP TABLE Customers; --`) can destroy data entirely.

## The actual pattern used throughout CustomerLedger

From `database/crud/Customers_CRUD.sql`:

```sql
SELECT CustomerId, BranchId, CustomerCode, FullName, ...
FROM Customers
WHERE PhoneNumber = ?;
```

And the equivalent MySqlConnector call (see `docs/database/CRUD-Queries.md`):

```csharp
const string sql = """
    SELECT CustomerId, CustomerCode, FullName, Email, PhoneNumber
    FROM Customers
    WHERE PhoneNumber = @phoneNumber AND IsDeleted = FALSE;
    """;

await using var command = new MySqlCommand(sql, connection);
command.Parameters.Add("@phoneNumber", MySqlDbType.VarChar).Value = phoneNumber;
```

The database driver sends the SQL text and the parameter value **separately** — the value
is never parsed as part of the SQL grammar, so no input can change the query's structure.

## Steps

1. Open any file under `database/crud/` and confirm every `WHERE`/`INSERT`/`UPDATE`
   statement uses `?` placeholders, never a literal value that looks like it came from user
   input.
2. Open `src/CustomerLedger.Infrastructure/Services/CustomerService.cs` and confirm every
   EF Core LINQ query (e.g. `_db.Customers.Where(c => c.PhoneNumber == phoneNumber)`) is
   parameterized automatically by EF Core — no `FromSqlRaw` with string interpolation
   appears anywhere in this project except the two deliberate `FromSqlInterpolated` calls in
   `PaymentService` (for `SELECT ... FOR UPDATE`), which **also** parameterize the
   interpolated value safely (`FromSqlInterpolated` is not string concatenation — it
   produces a parameterized command under the hood).
3. Attempt to find a single `+` string concatenation building a SQL string anywhere in
   `src/` or `database/` — there should be none.

## Expected outcome

Confirm, by direct inspection, that no SQL statement in this codebase concatenates
untrusted input — the project's one structural defense against SQL injection.
