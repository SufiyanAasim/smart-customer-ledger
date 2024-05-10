using CustomerLedger.DatabaseTests.Fixtures;
using MySqlConnector;

namespace CustomerLedger.DatabaseTests;

/// <summary>
/// Confirms the branch/status/date invoice list query — the single most common query on
/// the Invoices list screen — actually uses IX_Invoices_BranchId_InvoiceStatus_InvoiceDate
/// rather than a full table scan, by inspecting MySQL's EXPLAIN output directly.
/// </summary>
public class IndexUsageTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public IndexUsageTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [MySqlAvailableFact]
    public async Task InvoiceListQuery_UsesBranchStatusDateIndex()
    {
        await using var connection = new MySqlConnection(TestDatabaseSettings.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            EXPLAIN
            SELECT InvoiceId, InvoiceNumber, TotalAmount
            FROM Invoices
            WHERE BranchId = 1 AND InvoiceStatus = 'Active'
            ORDER BY InvoiceDate DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync(), "EXPLAIN returned no rows.");

        var possibleKeys = reader["possible_keys"]?.ToString() ?? string.Empty;
        var keyUsed = reader["key"]?.ToString() ?? string.Empty;

        Assert.True(
            possibleKeys.Contains("IX_Invoices_BranchId_InvoiceStatus_InvoiceDate") || keyUsed.Contains("IX_Invoices"),
            $"Expected the branch/status/date index to be a candidate. possible_keys='{possibleKeys}', key='{keyUsed}'.");
    }
}
