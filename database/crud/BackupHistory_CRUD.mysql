-- =====================================================================
-- CustomerLedger — BackupHistory_CRUD.sql
-- Rows are only ever created/updated by the actual backup workflow
-- (Snapshot release) — a row must never be marked Completed unless the
-- backup process itself finished successfully.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT: a backup run starting (Status = InProgress).
-- ---------------------------------------------------------------------
INSERT INTO BackupHistories (BackupType, FileName, FilePath, Status, StartedAtUtc, CreatedByUserId, CreatedAtUtc)
VALUES (?, ?, ?, 'InProgress', UTC_TIMESTAMP(6), ?, UTC_TIMESTAMP(6));

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT BackupHistoryId, BackupType, FileName, FilePath, FileSize, Status,
       StartedAtUtc, CompletedAtUtc, CreatedByUserId, ErrorMessage
FROM BackupHistories
WHERE BackupHistoryId = ?;

-- ---------------------------------------------------------------------
-- SELECT list (most recent first) with pagination
-- ---------------------------------------------------------------------
SELECT BackupHistoryId, BackupType, FileName, Status, StartedAtUtc, CompletedAtUtc
FROM BackupHistories
ORDER BY StartedAtUtc DESC
LIMIT ? OFFSET ?;

-- ---------------------------------------------------------------------
-- UPDATE: mark the run's actual outcome (Status is set by the workflow
-- based on real process completion — never optimistically beforehand).
-- ---------------------------------------------------------------------
UPDATE BackupHistories
SET Status = 'Completed',
    FileSize = ?,
    CompletedAtUtc = UTC_TIMESTAMP(6)
WHERE BackupHistoryId = ?;

UPDATE BackupHistories
SET Status = 'Failed',
    ErrorMessage = ?,
    CompletedAtUtc = UTC_TIMESTAMP(6)
WHERE BackupHistoryId = ?;

-- No DELETE statement — backup history is a permanent audit trail of
-- what was (or was not) actually backed up.
