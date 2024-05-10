using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.DatabaseTests.Fixtures;

/// <summary>
/// Applies EF Core migrations to the test database once per test class run, and tears the
/// schema back down afterward so repeated runs start from a clean slate. Skipped entirely
/// when MySQL isn't reachable — see MySqlAvailableFactAttribute.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(TestDatabaseSettings.ConnectionString, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new ApplicationDbContext(options);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateContext();
            await context.Database.MigrateAsync();
        }
        catch
        {
            // If MySQL is unreachable, MySqlAvailableFactAttribute has already skipped
            // every test in the class — swallow here so xUnit's fixture setup doesn't
            // itself throw and mask the (more informative) per-test skip reason.
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
        }
        catch
        {
            // Same reasoning as InitializeAsync.
        }
    }
}
