using System.Diagnostics;
using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CustomerLedger.Infrastructure.Backup;

/// <summary>Restores a previously created backup file via the real `mysql` client — the counterpart to MySqlBackupService.</summary>
public class MySqlRestoreService : IRestoreService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MySqlRestoreService> _logger;

    public MySqlRestoreService(ApplicationDbContext db, IConfiguration configuration, ILogger<MySqlRestoreService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> RestoreAsync(long backupHistoryId, CancellationToken cancellationToken = default)
    {
        var history = await _db.BackupHistories.FirstOrDefaultAsync(b => b.BackupHistoryId == backupHistoryId, cancellationToken)
            ?? throw new BusinessRuleException("Backup record not found.");

        if (history.Status != BackupStatus.Completed)
        {
            throw new BusinessRuleException("Only a successfully completed backup can be restored.");
        }

        if (!File.Exists(history.FilePath))
        {
            throw new BusinessRuleException("The backup file no longer exists on disk.");
        }

        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        var builder = new MySqlConnectionStringBuilder(connectionString);

        var mysqlPath = _configuration["BackupSettings:MysqlClientPath"] ?? "mysql";
        var arguments = $"--host={builder.Server} --port={builder.Port} --user={builder.UserID} {builder.Database}";

        var startInfo = new ProcessStartInfo(mysqlPath, arguments)
        {
            RedirectStandardInput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.Environment["MYSQL_PWD"] = builder.Password;

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        await using (var sourceFile = new FileStream(history.FilePath, FileMode.Open, FileAccess.Read))
        {
            await sourceFile.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
        }
        process.StandardInput.Close();

        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Restore from {FilePath} failed: {Error}", history.FilePath, stderr);
            return false;
        }

        _logger.LogWarning("Database restored from backup {BackupHistoryId} ({FileName}).", history.BackupHistoryId, history.FileName);
        return true;
    }
}
