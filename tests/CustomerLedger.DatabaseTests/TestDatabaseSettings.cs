namespace CustomerLedger.DatabaseTests;

public static class TestDatabaseSettings
{
    /// <summary>
    /// CUSTOMERLEDGER_TEST_CONNECTION takes precedence so CI/local test runs can point at a
    /// disposable database distinct from the development one; falls back to
    /// ConnectionStrings__DefaultConnection for convenience.
    /// </summary>
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("CUSTOMERLEDGER_TEST_CONNECTION")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? "Server=localhost;Port=3306;Database=customerledger_test;Uid=root;Pwd=root;";
}
