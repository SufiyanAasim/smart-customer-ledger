-- =====================================================================
-- CustomerLedger — Shard02Schema.sql
-- Identical procedure to Shard01Schema.sql, targeting the second shard.
-- =====================================================================

CREATE DATABASE IF NOT EXISTS customerledger_shard_02
    CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

-- Run against customerledger_shard_02:
--   database/schema/02_CreateTables.sql
--   database/schema/03_AlterTables.sql
--   database/constraints/CreateConstraints.sql
--   database/indexes/CreateIndexes.sql
--   database/views/CreateViews.sql
--   database/triggers/CreateTriggers.sql

-- Only branches where branchId % activeShardCount == 1 belong here
-- (e.g. branches 1 and 3 for a 2-shard registry).

GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON customerledger_shard_02.* TO 'customerledger_app'@'%';
FLUSH PRIVILEGES;
