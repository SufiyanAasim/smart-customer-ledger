using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Executes an actual `mysqldump` process and records the real outcome in BackupHistory —
/// a row is only ever marked Completed if the process exited 0 and produced a non-empty
/// file. Administrator-only; see Web/Areas/Admin/Controllers/BackupHistoriesController.
/// </summary>
public interface IBackupService
{
    Task<BackupHistory> CreateBackupAsync(BackupType backupType, string createdByUserId, CancellationToken cancellationToken = default);
}
