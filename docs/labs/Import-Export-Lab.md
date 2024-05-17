# Lab: Import and Export

**Goal**: export data in both formats, then import a CSV back in, observing validation.

## Export

1. Navigate to Customers, click **Export CSV**. Open the downloaded file in a text editor
   (not Excel yet) and confirm it starts with a UTF-8 BOM and a header row.
2. Open the same file in Excel/Google Sheets and confirm it renders correctly (no encoding
   mojibake).
3. Click **Export JSON** and confirm valid, indented JSON.
4. From a Customer's Details page, click **Account Statement (CSV)** and confirm it lists
   both invoices and payments for that one customer.
5. Create a test customer whose `FullName` is exactly `=1+1` (a formula-injection attempt),
   export Customers CSV again, and open it in Excel. Confirm the cell displays the literal
   text `'=1+1`, not a computed formula result — `CsvUtilities.EscapeField`'s
   neutralization at work.

## Import

1. Navigate to Customers → Import CSV. Create a CSV file with header
   `CustomerCode,FullName,PhoneNumber,Address,City` and 3 rows, one of which duplicates an
   existing `CustomerCode`.
2. Select a branch, upload the file, click **Preview**. Confirm 2 rows show **Accepted** and
   1 shows **Rejected** with the reason "CustomerCode '...' already exists in this branch."
3. Re-upload the same file and click **Confirm Import**. Confirm only the 2 accepted rows
   now exist as real Customers (with linked CustomerAccounts), and the rejected row was
   never written.
4. Attempt to upload a CSV missing the `PhoneNumber` column entirely — confirm the whole
   import is rejected with "Missing required column(s): PhoneNumber."
5. Attempt to upload a file larger than 2 MB — confirm it is rejected before any parsing.

## Expected outcomes

- Formula injection is neutralized on export (step 5 of Export).
- Nothing is written to the database until the explicit **Confirm Import** step (step 3 of
  Import) — Preview alone never persists anything.
- Missing-column and oversized-file uploads fail with clear, specific messages.

## Automated coverage

`CsvUtilitiesTests` (unit, no DB) covers the formula-injection neutralization and CSV
parsing directly. `ImportService`'s validation logic is exercised manually per this lab —
add a `DatabaseTests`/`IntegrationTests` case here if broader automated coverage is desired
in a future release.
