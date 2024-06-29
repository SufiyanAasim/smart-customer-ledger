-- =====================================================================
-- CustomerLedger — PrimarySetup.sql
-- Configures the primary MySQL server for replication. This is real,
-- standard MySQL replication configuration — what actually runs against
-- it (native replication vs. this project's simulated mode) is a
-- separate decision documented in docs/releases/v5.0.0-Replica.md.
-- =====================================================================

-- my.cnf / my.ini, [mysqld] section, on the PRIMARY server:
--   server-id = 1
--   log_bin = mysql-bin
--   binlog_format = ROW
--   gtid_mode = ON
--   enforce_gtid_consistency = ON

-- After restarting MySQL with the above config, create a dedicated
-- replication user (never reuse the application's own credentials):
CREATE USER IF NOT EXISTS 'customerledger_replicator'@'%' IDENTIFIED WITH mysql_native_password BY 'CHANGE_ME';
GRANT REPLICATION SLAVE ON *.* TO 'customerledger_replicator'@'%';
FLUSH PRIVILEGES;

-- Confirm binary logging is active and note the current position — this
-- is what a replica's CHANGE REPLICATION SOURCE TO would reference in a
-- real (non-GTID) native replication setup:
SHOW MASTER STATUS;
