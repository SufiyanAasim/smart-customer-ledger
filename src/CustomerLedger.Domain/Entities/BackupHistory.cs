using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

/// <summary>
/// Records the outcome of an actual backup run. Never write a Completed row unless the
/// backup process itself finished successfully — see the Snapshot release for the workflow
/// that populates this table. Index only implements viewing/foundation for this entity.
/// </summary>
public class BackupHistory
{
    public long BackupHistoryId { get; set; }
    public BackupType BackupType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long? FileSize { get; set; }
    public BackupStatus Status { get; set; } = BackupStatus.InProgress;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;
}
