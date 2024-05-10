-- =====================================================================
-- CustomerLedger — 01_CreateDatabase.sql
-- Creates the application database with a utf8mb4 default charset/collation
-- so the schema matches what EF Core's Pomelo provider generates.
--
-- Source of truth: this script documents the schema for MySQL Workbench
-- demonstration and manual grading review. The database actually used by
-- the application is created/updated via EF Core migrations
-- (dotnet ef database update) — see docs/database/Database-Dictionary.md
-- for which source governs which object.
-- =====================================================================

CREATE DATABASE IF NOT EXISTS customerledger
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

-- Dedicated application login — do not use the MySQL root account from the
-- application connection string. Replace CHANGE_ME with a strong secret
-- generated for your environment, never a hardcoded value committed to git.
CREATE USER IF NOT EXISTS 'customerledger_app'@'%' IDENTIFIED BY 'CHANGE_ME';
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON customerledger.* TO 'customerledger_app'@'%';
FLUSH PRIVILEGES;

USE customerledger;
