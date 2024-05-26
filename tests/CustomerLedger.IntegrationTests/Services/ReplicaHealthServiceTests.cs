using CustomerLedger.DatabaseTests;
using CustomerLedger.Infrastructure.Data.Contexts;
using CustomerLedger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CustomerLedger.IntegrationTests.Services;

public class ReplicaHealthServiceTests
{
    [MySqlAvailableFact]
    public async Task IsReplicaHealthyAsync_WithReachableConnection_ReturnsTrue()
    {
        var options = new DbContextOptionsBuilder<ReplicaDbContext>()
            .UseMySql(TestDatabaseSettings.ConnectionString, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
        await using var replicaDb = new ReplicaDbContext(options);
        var sut = new ReplicaHealthService(replicaDb, NullLogger<ReplicaHealthService>.Instance);

        var healthy = await sut.IsReplicaHealthyAsync();

        Assert.True(healthy);
    }

    [MySqlAvailableFact]
    public async Task IsReplicaHealthyAsync_WithUnreachableHost_ReturnsFalse()
    {
        var options = new DbContextOptionsBuilder<ReplicaDbContext>()
            .UseMySql("Server=192.0.2.1;Port=3306;Database=nonexistent;Uid=nobody;Pwd=nothing;ConnectionTimeout=2;", new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;
        await using var replicaDb = new ReplicaDbContext(options);
        var sut = new ReplicaHealthService(replicaDb, NullLogger<ReplicaHealthService>.Instance);

        var healthy = await sut.IsReplicaHealthyAsync();

        Assert.False(healthy);
    }
}
