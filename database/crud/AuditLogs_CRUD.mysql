-- =====================================================================
-- CustomerLedger — AuditLogs_CRUD.sql
-- Append-oriented: there is no UPDATE of OldValuesJson/NewValuesJson and
-- no hard DELETE. Administrators may only set ReviewStatus/AdminNote or
-- archive a row.
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT
-- ---------------------------------------------------------------------
INSERT INTO AuditLogs (
    UserId, BranchId, TableName, RecordId, ActionType,
    OldValuesJson, NewValuesJson, IpAddress, CorrelationId, CreatedAtUtc, ReviewStatus, IsArchived
) VALUES (
    ?, ?, ?, ?, ?,
    ?, ?, ?, ?, UTC_TIMESTAMP(6), 'Unreviewed', 0
);

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT AuditLogId, UserId, BranchId, TableName, RecordId, ActionType,
       OldValuesJson, NewValuesJson, CreatedAtUtc, ReviewStatus, AdminNote, IsArchived
FROM AuditLogs
WHERE AuditLogId = ?;

-- ---------------------------------------------------------------------
-- SELECT list with search/filter/pagination (Administrator only)
-- ---------------------------------------------------------------------
SELECT AuditLogId, TableName, RecordId, ActionType, BranchId, CreatedAtUtc, ReviewStatus
FROM AuditLogs
WHERE IsArchived = 0
  AND (? IS NULL OR BranchId = ?)     -- @branchId
  AND (? = '' OR TableName = ?)       -- @tableName
ORDER BY CreatedAtUtc DESC
LIMIT ? OFFSET ?;

-- ---------------------------------------------------------------------
-- Audit trail for one record (JOIN-free lookup, most common query shape).
-- ---------------------------------------------------------------------
SELECT CreatedAtUtc, ActionType, UserId, OldValuesJson, NewValuesJson
FROM AuditLogs
WHERE TableName = ? AND RecordId = ?
ORDER BY CreatedAtUtc;

-- ---------------------------------------------------------------------
-- UPDATE: review status and admin note ONLY — never the audit payload.
-- ---------------------------------------------------------------------
UPDATE AuditLogs
SET ReviewStatus = ?,
    AdminNote = ?
WHERE AuditLogId = ?;

-- ---------------------------------------------------------------------
-- Archive instead of destructive delete.
-- ---------------------------------------------------------------------
UPDATE AuditLogs
SET IsArchived = 1
WHERE AuditLogId = ?;
