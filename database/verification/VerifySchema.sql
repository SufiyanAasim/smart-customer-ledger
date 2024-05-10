-- =====================================================================
-- CustomerLedger — VerifySchema.sql
-- Confirms every expected table exists with InnoDB + utf8mb4, and lists
-- column counts as a quick sanity check against 02_CreateTables.sql.
-- =====================================================================

USE customerledger;

SELECT table_name, engine, table_collation
FROM information_schema.tables
WHERE table_schema = DATABASE()
ORDER BY table_name;

SELECT table_name, COUNT(*) AS column_count
FROM information_schema.columns
WHERE table_schema = DATABASE()
GROUP BY table_name
ORDER BY table_name;

-- Expect exactly these business tables to be present (11) alongside the
-- 7 ASP.NET Core Identity tables (18 total).
SELECT
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE()) AS total_tables,
    18 AS expected_minimum_tables;
