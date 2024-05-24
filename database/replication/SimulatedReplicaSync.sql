-- =====================================================================
-- CustomerLedger — SimulatedReplicaSync.sql
-- This project's default mode: a SIMULATED replica, not native MySQL
-- replication (see docs/releases/v5.0.0-Replica.md for why). "Sync" here
-- means periodically re-copying the primary's data into a second
-- database via mysqldump — a batch snapshot, not continuous binlog
-- streaming. Lag is therefore however long ago the last sync ran, not a
-- sub-second replication delay.
--
-- Do not present this as native replication in a viva — be explicit
-- that it is a documented simulation of the read/write separation
-- pattern, chosen because this environment cannot guarantee a second
-- MySQL server is available to configure as a true replica.
-- =====================================================================

-- One-time: create the simulated replica database.
CREATE DATABASE IF NOT EXISTS customerledger_replica
    CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

-- Re-run this shell command on whatever interval you want to simulate
-- ("sync" the replica) — e.g. every few minutes via cron/Task Scheduler:
--
--   mysqldump -u root -p customerledger | mysql -u root -p customerledger_replica
--
-- Configure ConnectionStrings:ReplicaConnection to point at
-- customerledger_replica (same server, different schema name) or a
-- genuinely separate MySQL instance if one is available.

-- Demonstrate staleness: write to the primary, then immediately query
-- the (not-yet-resynced) replica and observe the old value.
-- 1. On the primary:
UPDATE customerledger.Branches SET Name = 'Renamed For Lag Demo' WHERE BranchId = 1;
-- 2. On the replica (before running the mysqldump sync above again):
SELECT Name FROM customerledger_replica.Branches WHERE BranchId = 1;
-- Expected: still shows the OLD name — this is the "lag" in this simulated mode.
-- 3. Run the mysqldump sync command above, then re-run step 2 — now it matches.
