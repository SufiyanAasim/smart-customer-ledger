-- =====================================================================
-- CustomerLedger — Shard01Schema.sql
-- Each shard is a full, independent CustomerLedger database — sharding
-- splits DATA (which branches live where), not the SCHEMA (every shard
-- has the identical table/view/trigger structure). Rather than
-- duplicating 02_CreateTables.sql's ~250 lines a second time here, this
-- script documents exactly which existing scripts to run and against
-- which database name.
-- =====================================================================

-- 1. Create the shard's database:
CREATE DATABASE IF NOT EXISTS customerledger_shard_01
    CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

-- 2. Run the standard schema scripts against it (substitute the -D value,
--    or `USE customerledger_shard_01;` before each script):
--      mysql -u root -p customerledger_shard_01 < database/schema/02_CreateTables.sql
--      mysql -u root -p customerledger_shard_01 < database/schema/03_AlterTables.sql
--      mysql -u root -p customerledger_shard_01 < database/constraints/CreateConstraints.sql
--      mysql -u root -p customerledger_shard_01 < database/indexes/CreateIndexes.sql
--      mysql -u root -p customerledger_shard_01 < database/views/CreateViews.sql
--      mysql -u root -p customerledger_shard_01 < database/triggers/CreateTriggers.sql

-- 3. Only insert branches assigned to this shard by IShardResolver
--    (branchId % activeShardCount == 0 for a 2-shard registry — see
--    ShardResolver.cs). Example, assuming branches 2 and 4 route here:
-- INSERT INTO Branches (BranchId, BranchCode, Name, PhoneNumber, Address, City, IsActive, CreatedAtUtc)
-- VALUES (2, 'SHARD1-A', 'Shard 1 Branch A', '0', 'n/a', 'Karachi', 1, UTC_TIMESTAMP(6));

-- 4. Grant the application user access to this shard's database:
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON customerledger_shard_01.* TO 'customerledger_app'@'%';
FLUSH PRIVILEGES;
