# Lab: Backup and Restore

**Goal**: run a real backup, inspect its output, and restore it.

## Prerequisites

`mysqldump` and `mysql` client binaries on the machine running the app (same host as the
MySQL server is not required — just network access to it), and an Administrator account.

## Steps

1. Sign in as Administrator, navigate to Admin → Backup History.
2. Select **Full** and click **Run Backup Now**. Confirm a new row appears with `Status =
   Completed` and a non-zero `FileSize`.
3. Locate the actual file on disk (`BackupSettings:Directory`, default
   `App_Data/Backups/`) and open it in a text editor — confirm it is a plain-text
   `mysqldump` SQL file (`-- MySQL dump ...` header, `CREATE TABLE`/`INSERT` statements).
4. Make a small, reversible change in the app (e.g. edit a branch's phone number).
5. Back on Admin → Backup History, expand the Completed backup row's Restore form, type the
   literal word `RESTORE`, and submit.
6. Confirm the branch's phone number reverted to what it was before step 4.
7. Repeat step 2 with **SchemaOnly** and confirm the resulting file contains `CREATE TABLE`
   statements but no `INSERT` statements (open it and check).
8. Deliberately misconfigure `BackupSettings:MysqldumpPath` (e.g. to `not-a-real-binary`)
   and run a backup again — confirm the row is recorded `Failed` with a captured
   `ErrorMessage`, never a false `Completed`.

## Expected outcomes

- Step 2 and step 7 produce real, inspectable SQL dump files.
- Step 5's restore is blocked unless the confirmation text is typed exactly.
- Step 8 never fabricates success.

## Automated coverage

`BackupServiceTests.CreateBackupAsync_WithMissingMysqldumpBinary_RecordsFailedNotCompleted`
automates step 8's exact scenario.
