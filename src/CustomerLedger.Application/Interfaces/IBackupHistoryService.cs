using CustomerLedger.Application.Results;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Index scope is viewing only. Actual backup/restore execution is Snapshot-release scope
/// — see docs/releases/v3.0.0-Snapshot.md.
/// </summary>
public interface IBackupHistoryService
{
    Task<PagedResult<BackupHistory>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<BackupHistory?> GetByIdAsync(long backupHistoryId, CancellationToken cancellationToken = default);
}
