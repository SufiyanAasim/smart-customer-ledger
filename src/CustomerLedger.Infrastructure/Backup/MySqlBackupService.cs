using System.Diagnostics;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CustomerLedger.Infrastructure.Backup;

/// <summary>
/// Shells out to the real `mysqldump` client rather than reimplementing a dump format —
/// this is the same tool a DBA would run by hand, so a CustomerLedger backup restores with
/// the standard `mysql` client too (see MySqlRestoreService). The password is passed via
/// the MYSQL_PWD environment variable for the child process only, never as a visible
/// command-line argument (which would otherwise leak into the OS process list).
/// </summary>
public class MySqlBackupService : IBackupService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MySqlBackupService> _logger;

    public MySqlBackupService(ApplicationDbContext db, IConfiguration configuration, ILogger<MySqlBackupService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<BackupHistory> CreateBackupAsync(BackupType backupType, string createdByUserId, CancellationToken cancellationToken = default)
    {
        var directory = _configuration["BackupSettings:Directory"] ?? "App_Data/Backups";
        Directory.CreateDirectory(directory);

        var fileName = $"customerledger_{backupType}_{DateTime.UtcNow:yyyyMMddHHmmss}.sql";
        var filePath = Path.Combine(directory, fileName);

        var history = new BackupHistory
        {
            BackupType = backupType,
            FileName = fileName,
            FilePath = filePath,
            Status = BackupStatus.InProgress,
            StartedAtUtc = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.BackupHistories.Add(history);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
            var builder = new MySqlConnectionStringBuilder(connectionString);

            var mysqldumpPath = _configuration["BackupSettings:MysqldumpPath"] ?? "mysqldump";
            var arguments = BuildDumpArguments(builder, backupType);

            var exitCode = await RunProcessAsync(mysqldumpPath, arguments, builder.Password, filePath, cancellationToken);

            var fileInfo = new FileInfo(filePath);
            if (exitCode == 0 && fileInfo.Exists && fileInfo.Length > 0)
            {
                history.Status = BackupStatus.Completed;
                history.FileSize = fileInfo.Length;
                history.CompletedAtUtc = DateTime.UtcNow;
            }
            else
            {
                history.Status = BackupStatus.Failed;
                history.ErrorMessage = $"mysqldump exited with code {exitCode} or produced no output file.";
                history.CompletedAtUtc = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            // Never report success unless the process actually completed — a caught
            // exception here (missing mysqldump binary, bad credentials, etc.) is always
            // recorded as Failed, never silently swallowed into a false Completed row.
            _logger.LogError(ex, "Backup {FileName} failed.", fileName);
            history.Status = BackupStatus.Failed;
            history.ErrorMessage = ex.Message;
            history.CompletedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return history;
    }

    private static string BuildDumpArguments(MySqlConnectionStringBuilder builder, BackupType backupType)
    {
        var args = $"--host={builder.Server} --port={builder.Port} --user={builder.UserID} " +
                    "--single-transaction --routines --triggers";

        args += backupType switch
        {
            BackupType.SchemaOnly => " --no-data",
            BackupType.DataOnly => " --no-create-info",
            _ => string.Empty
        };

        return $"{args} {builder.Database}";
    }

    private static async Task<int> RunProcessAsync(string fileName, string arguments, string password, string outputFilePath, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.Environment["MYSQL_PWD"] = password;

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        await using (var outputFile = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
        {
            await process.StandardOutput.BaseStream.CopyToAsync(outputFile, cancellationToken);
        }

        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(stderr))
        {
            throw new InvalidOperationException($"mysqldump error: {stderr}");
        }

        return process.ExitCode;
    }
}
