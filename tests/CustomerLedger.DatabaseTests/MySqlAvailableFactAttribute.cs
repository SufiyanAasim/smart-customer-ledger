using MySqlConnector;
using Xunit;

namespace CustomerLedger.DatabaseTests;

/// <summary>
/// A [Fact] that probes the test MySQL connection (CUSTOMERLEDGER_TEST_CONNECTION,
/// falling back to ConnectionStrings__DefaultConnection) at discovery time and marks
/// itself Skip'd with a clear reason when no server answers, instead of failing every
/// database test in environments without MySQL installed (e.g. this sandbox). Point it
/// at a real MySQL 8.0+ instance to actually execute these tests — see
/// docs/manuals/Database-Setup-Guide.md.
/// </summary>
public sealed class MySqlAvailableFactAttribute : FactAttribute
{
    public MySqlAvailableFactAttribute()
    {
        var connectionString = TestDatabaseSettings.ConnectionString;

        try
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
        }
        catch (Exception ex)
        {
            Skip = $"MySQL test database not reachable ({ex.GetType().Name}: {ex.Message}). " +
                   "Set CUSTOMERLEDGER_TEST_CONNECTION to a real MySQL 8.0+ instance to run this test.";
        }
    }
}
