-- =====================================================================
-- CustomerLedger — ReplicaSetup.sql
-- Configures a MySQL server as a native replica of the primary. Requires
-- network connectivity from the replica host to the primary and the
-- replication user created by PrimarySetup.sql.
-- =====================================================================

-- my.cnf / my.ini, [mysqld] section, on the REPLICA server:
--   server-id = 2          -- must differ from the primary's server-id
--   relay_log = mysql-relay
--   read_only = ON          -- prevents accidental writes hitting the replica directly
--   gtid_mode = ON
--   enforce_gtid_consistency = ON

-- On the replica, point it at the primary (GTID-based auto-positioning):
CHANGE REPLICATION SOURCE TO
    SOURCE_HOST = '<primary-host>',
    SOURCE_PORT = 3306,
    SOURCE_USER = 'customerledger_replicator',
    SOURCE_PASSWORD = 'CHANGE_ME',
    SOURCE_AUTO_POSITION = 1;

START REPLICA;

-- Confirm both IO and SQL replication threads are running, and note
-- Seconds_Behind_Source (replica lag):
SHOW REPLICA STATUS\G
