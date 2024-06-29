-- =====================================================================
-- CustomerLedger — CustomerInteractions_CRUD.sql
-- =====================================================================

USE customerledger;

-- ---------------------------------------------------------------------
-- INSERT
-- ---------------------------------------------------------------------
INSERT INTO CustomerInteractions (
    CustomerId, BranchId, InteractionType, Subject, Description,
    InteractionDate, FollowUpDate, Status, RecordedByUserId, CreatedAtUtc
) VALUES (
    ?, ?, ?, ?, ?,
    ?, ?, ?, ?, UTC_TIMESTAMP(6)
);

-- ---------------------------------------------------------------------
-- SELECT by primary key
-- ---------------------------------------------------------------------
SELECT CustomerInteractionId, CustomerId, BranchId, InteractionType, Subject, Description,
       InteractionDate, FollowUpDate, Status, RecordedByUserId
FROM CustomerInteractions
WHERE CustomerInteractionId = ?;

-- ---------------------------------------------------------------------
-- SELECT list with search/filter/pagination
-- ---------------------------------------------------------------------
SELECT CustomerInteractionId, CustomerId, InteractionType, Subject, InteractionDate, FollowUpDate, Status
FROM CustomerInteractions
WHERE (? IS NULL OR BranchId = ?)      -- @branchId
  AND (? IS NULL OR CustomerId = ?)    -- @customerId
ORDER BY InteractionDate DESC
LIMIT ? OFFSET ?;

-- ---------------------------------------------------------------------
-- Upcoming follow-ups for a branch (staff worklist).
-- ---------------------------------------------------------------------
SELECT ci.CustomerInteractionId, c.FullName, ci.Subject, ci.FollowUpDate
FROM CustomerInteractions ci
JOIN Customers c ON c.CustomerId = ci.CustomerId
WHERE ci.BranchId = ? AND ci.FollowUpDate IS NOT NULL AND ci.FollowUpDate >= UTC_TIMESTAMP()
  AND ci.Status <> 'Closed'
ORDER BY ci.FollowUpDate;

-- ---------------------------------------------------------------------
-- UPDATE
-- ---------------------------------------------------------------------
UPDATE CustomerInteractions
SET InteractionType = ?,
    Subject = ?,
    Description = ?,
    InteractionDate = ?,
    FollowUpDate = ?,
    Status = ?,
    UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE CustomerInteractionId = ?;

-- ---------------------------------------------------------------------
-- Archive / close (soft state change — no destructive delete).
-- ---------------------------------------------------------------------
UPDATE CustomerInteractions
SET Status = 'Closed', UpdatedAtUtc = UTC_TIMESTAMP(6)
WHERE CustomerInteractionId = ?;

-- ---------------------------------------------------------------------
-- JOIN example: full interaction history for one customer, with staff name.
-- ---------------------------------------------------------------------
SELECT ci.InteractionDate, ci.InteractionType, ci.Subject, ci.Status, u.FullName AS RecordedBy
FROM CustomerInteractions ci
JOIN AspNetUsers u ON u.Id = ci.RecordedByUserId
WHERE ci.CustomerId = ?
ORDER BY ci.InteractionDate DESC;
