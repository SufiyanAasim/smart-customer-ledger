-- =====================================================================
-- CustomerLedger — CrossShardReporting.sql
-- MySQL has no native cross-server JOIN/UNION — you cannot write one SQL
-- statement spanning customerledger_shard_01 and customerledger_shard_02
-- from a single connection to either server. CrossShardReportingService
-- (C#) queries each shard's own connection independently and aggregates
-- in application code; this script shows the manual MySQL Workbench
-- equivalent using two separate connection tabs.
-- =====================================================================

-- Tab 1 — connected to customerledger_shard_01:
SELECT * FROM vw_BranchRevenueSummary;

-- Tab 2 — connected to customerledger_shard_02:
SELECT * FROM vw_BranchRevenueSummary;

-- Combine the two result sets manually (copy/paste, or export both to
-- CSV and concatenate) — this manual step is exactly what
-- CrossShardReportingService.GetBranchRevenueSummaryAcrossShardsAsync
-- automates, with the added behavior that if one shard's connection
-- fails, the other shard's rows are still returned (partial result),
-- and the failure is reported explicitly rather than the whole report
-- silently coming back short.

-- If both servers happen to be reachable via federated/dblink-style
-- tooling (not used by this project — see Known Limitations in
-- docs/releases/v6.0.0-Shard.md), a single UNION ALL across a FEDERATED
-- table pointing at each shard would look like:
--
--   SELECT * FROM vw_BranchRevenueSummary_shard01
--   UNION ALL
--   SELECT * FROM vw_BranchRevenueSummary_shard02;
--
-- This project does not set up FEDERATED tables — the C# aggregation
-- approach is more portable and doesn't require a MySQL storage engine
-- most managed hosts disable.
