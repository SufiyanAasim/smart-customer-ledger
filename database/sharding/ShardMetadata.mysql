-- =====================================================================
-- CustomerLedger — ShardMetadata.sql
-- This project's actual shard registry lives in application
-- configuration (ShardSettings:Shards in appsettings — see
-- IShardResolver / ShardResolver.cs), NOT in a database table, to avoid
-- the chicken-and-egg problem of "which shard holds the map of which
-- shard holds what" (the registry itself would need to live somewhere
-- queryable before you know which shard to query).
--
-- This script instead documents what a database-table-backed shard
-- registry WOULD look like, for a production system that outgrows a
-- static configuration file (e.g. wanting to reassign a branch to a
-- different shard without redeploying the app). It is illustrative —
-- not applied to this project's actual schema.
-- =====================================================================

-- A hypothetical *separate, unsharded* registry database that every
-- application instance can reach regardless of which shard it's
-- currently routing a request to:
CREATE DATABASE IF NOT EXISTS customerledger_shard_registry
    CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

USE customerledger_shard_registry;

CREATE TABLE IF NOT EXISTS Shards (
    ShardId VARCHAR(20) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    ConnectionStringName VARCHAR(100) NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (ShardId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS BranchShardAssignments (
    BranchId INT NOT NULL,
    ShardId VARCHAR(20) NOT NULL,
    AssignedAtUtc DATETIME(6) NOT NULL,
    PRIMARY KEY (BranchId),
    CONSTRAINT FK_BranchShardAssignments_Shards FOREIGN KEY (ShardId) REFERENCES Shards (ShardId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- An explicit per-branch assignment table like this (rather than a
-- computed `branchId % shardCount`) is exactly what would let a
-- rebalancing operation move ONE branch to a new shard without
-- reshuffling every other branch — see the Rebalancing section of
-- docs/releases/v6.0.0-Shard.md.
