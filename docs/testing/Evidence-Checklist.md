# Evidence Checklist

Checklist of evidence to collect before final submission/grading. Screenshots and command
output are **placeholders** here — do not consider this document complete until each item
has been actually captured against a real running instance with a real MySQL database; a
checkbox in this file records intent to collect evidence, not evidence itself.

## Build and test evidence

- [ ] `dotnet build` output showing 0 errors/warnings (captured in every release document
      already, from the actual sandbox build — re-capture against the grading machine too)
- [ ] `dotnet test` output with `CUSTOMERLEDGER_TEST_CONNECTION` set, showing the 28
      currently-skipped tests actually passing
- [ ] `dotnet ef database update` output showing successful migration application

## Screenshots (placeholder — capture manually)

- [ ] Login page
- [ ] Dashboard (Administrator view and Staff view, to show the difference)
- [ ] Customer list with search/filter/pagination in use
- [ ] Invoice detail page showing a Draft invoice with line items
- [ ] Invoice detail page showing an Active, partially-paid invoice
- [ ] Payment reversal form and its result
- [ ] Installment plan detail page with a mixed Paid/Pending/Overdue schedule
- [ ] Admin → Reconciliation results page showing a corrected account
- [ ] Admin → Backup History showing a Completed backup with file size
- [ ] Customer import Preview screen showing both accepted and rejected rows
- [ ] MySQL Workbench: `EXPLAIN` output for the invoice list query showing index usage
- [ ] MySQL Workbench: all six views listed via `information_schema.views`
- [ ] MySQL Workbench: all eight triggers listed via `information_schema.triggers`

## SQL command output (placeholder — capture manually)

- [ ] `database/verification/VerifySchema.sql` output
- [ ] `database/verification/VerifyConstraints.sql` output (including the negative test)
- [ ] `database/verification/VerifyViews.sql` output
- [ ] `database/verification/VerifyTriggers.sql` output
- [ ] Two-session isolation demonstration transcript (`ACID-Demonstrations.sql`)

## Do not fabricate

If an item above cannot be captured (e.g. no MySQL server available), record that honestly
in the relevant release document's Verification section rather than inventing a screenshot
or output that was never actually produced.
