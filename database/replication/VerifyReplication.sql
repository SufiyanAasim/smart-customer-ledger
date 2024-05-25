-- =====================================================================
-- CustomerLedger — VerifyReplication.sql
-- Verification for whichever mode is actually configured.
-- =====================================================================

-- ---------------------------------------------------------------------
-- Native replication mode (PrimarySetup.sql + ReplicaSetup.sql)
-- ---------------------------------------------------------------------
-- On the primary:
SHOW MASTER STATUS;
SHOW REPLICAS;

-- On the replica:
SHOW REPLICA STATUS\G
-- Check specifically:
--   Replica_IO_Running: Yes
--   Replica_SQL_Running: Yes
--   Seconds_Behind_Source: (a number, ideally 0 or very small)
--   Last_IO_Error / Last_SQL_Error: (empty)

-- ---------------------------------------------------------------------
-- Simulated replication mode (SimulatedReplicaSync.sql)
-- ---------------------------------------------------------------------
-- Compare row counts between primary and simulated replica — they
-- should match immediately after a sync and may differ afterward
-- (that difference IS the simulated "lag"):
SELECT
    (SELECT COUNT(*) FROM customerledger.Customers) AS primary_customer_count,
    (SELECT COUNT(*) FROM customerledger_replica.Customers) AS replica_customer_count;

-- ---------------------------------------------------------------------
-- Application-level health check
-- ---------------------------------------------------------------------
-- IReplicaHealthService.IsReplicaHealthyAsync() calls
-- ReplicaDbContext.Database.CanConnectAsync() with a 3-second timeout.
-- Confirm it reports unhealthy correctly by pointing
-- ConnectionStrings:ReplicaConnection at an unreachable host/port and
-- observing the "Replica unavailable — falling back to the primary
-- connection" warning logged by ReplicaAwareReportingService.
