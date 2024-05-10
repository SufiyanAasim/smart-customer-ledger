using CustomerLedger.DatabaseTests;
using CustomerLedger.DatabaseTests.Fixtures;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Backup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace CustomerLedger.IntegrationTests.Services;

/// <summary>
/// Confirms the "never fabricate success" rule: when the configured mysqldump path is bogus
/// (which it always is here, since neither MySQL nor mysqldump is installed in this
/// sandbox), the resulting BackupHistory row must be Failed with a captured error message —
/// never silently marked Completed.
/// </summary>
public class BackupServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public BackupServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [MySqlAvailableFact]
    public async Task CreateBackupAsync_WithMissingMysqldumpBinary_RecordsFailedNotCompleted()
    {
        await using var db = _fixture.CreateContext();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = $"u-{Guid.NewGuid():N}@t.local",
            Email = $"u-{Guid.NewGuid():N}@t.local",
            FullName = "Backup Admin",
            EmployeeCode = $"E-{Guid.NewGuid():N}"[..15],
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestDatabaseSettings.ConnectionString,
                ["BackupSettings:MysqldumpPath"] = "definitely-not-a-real-binary-xyz"
            })
            .Build();

        var sut = new MySqlBackupService(db, configuration, NullLogger<MySqlBackupService>.Instance);

        var history = await sut.CreateBackupAsync(BackupType.Full, user.Id);

        Assert.Equal(BackupStatus.Failed, history.Status);
        Assert.False(string.IsNullOrWhiteSpace(history.ErrorMessage));
        Assert.NotNull(history.CompletedAtUtc);
    }
}
