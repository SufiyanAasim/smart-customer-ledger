-- =====================================================================
-- CustomerLedger — ShardRoutingExamples.sql
-- Illustrates, in raw SQL, the exact routing rule ShardResolver.cs
-- implements in C# — useful for explaining the algorithm without
-- reading code, e.g. during a viva.
-- =====================================================================

-- With 2 active shards (shard-01, shard-02, ordered by ShardId):
--   activeShards[0] = shard-01
--   activeShards[1] = shard-02
--   shardIndex = ((branchId % 2) + 2) % 2   -- the extra +N, %N guards against negative branchId

-- Worked examples:
SELECT
    branch_id,
    ((branch_id % 2) + 2) % 2 AS shard_index,
    CASE ((branch_id % 2) + 2) % 2
        WHEN 0 THEN 'shard-01'
        WHEN 1 THEN 'shard-02'
    END AS resolved_shard
FROM (
    SELECT 1 AS branch_id UNION ALL
    SELECT 2 UNION ALL
    SELECT 3 UNION ALL
    SELECT 4 UNION ALL
    SELECT 5
) AS branches;

-- Expected output:
--   branch 1 -> shard_index 1 -> shard-02
--   branch 2 -> shard_index 0 -> shard-01
--   branch 3 -> shard_index 1 -> shard-02
--   branch 4 -> shard_index 0 -> shard-01
--   branch 5 -> shard_index 1 -> shard-02

-- What happens if a third shard is added (activeShardCount becomes 3):
SELECT
    branch_id,
    branch_id % 3 AS new_shard_index_with_3_shards,
    branch_id % 2 AS old_shard_index_with_2_shards
FROM (
    SELECT 1 AS branch_id UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
) AS branches;
-- Notice branch 3 and 4 land on a DIFFERENT shard index once shard count
-- changes from 2 to 3 — this is the exact rebalancing problem plain
-- modulus routing has, discussed in docs/releases/v6.0.0-Shard.md.
