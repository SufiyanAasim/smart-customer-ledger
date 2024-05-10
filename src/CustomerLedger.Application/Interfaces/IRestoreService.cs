namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Executes an actual `mysql` client process to restore a previously created backup file.
/// Deliberately destructive (overwrites current data) — Administrator-only, and the
/// controller must require explicit confirmation before calling this.
/// </summary>
public interface IRestoreService
{
    /// <returns>True if the restore process exited successfully.</returns>
    Task<bool> RestoreAsync(long backupHistoryId, CancellationToken cancellationToken = default);
}
