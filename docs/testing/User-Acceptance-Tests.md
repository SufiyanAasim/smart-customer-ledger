# User Acceptance Tests

Written from the perspective of each role, in plain business language — for a
non-technical stakeholder (or the course instructor during a demo) to run through.

## As an Administrator

- [ ] I can create a new branch and see it appear in every branch dropdown.
- [ ] I can create a staff account, assign them a role and branch, and they can immediately
      log in with the password I set.
- [ ] I can deactivate a staff account and they can no longer log in.
- [ ] I can see customers, invoices, and payments across **all** branches, not just one.
- [ ] I can review the audit log and mark an entry as reviewed.
- [ ] I can run a backup and see it complete successfully with a file size shown.
- [ ] I can run reconciliation for a branch and see a before/after report.

## As a Branch Manager

- [ ] I can register a new customer in my own branch.
- [ ] I cannot register a customer in a different branch (the option isn't offered).
- [ ] I can approve or cancel an installment plan request from my staff.
- [ ] I can see my branch's customers, invoices, and payments, but not another branch's.
- [ ] I can reverse a payment (with a reason) and see the invoice balance update.

## As a Cashier / Staff

- [ ] I can register a customer and immediately create an invoice for them.
- [ ] I can add multiple line items to a Draft invoice and see the total recalculate live.
- [ ] Once I activate an invoice, I can no longer edit its items.
- [ ] I can record a full or partial payment and see the invoice status update
      (Unpaid → PartiallyPaid → Paid).
- [ ] I can create an installment plan and see a schedule generated automatically.
- [ ] I can pay one installment and see just that row (not the whole plan) update.
- [ ] I can log a customer interaction (e.g. a complaint) and see it in their history.
- [ ] I cannot reverse a payment or approve an installment plan (no such button appears).
- [ ] I cannot see another branch's data even if I know or guess a record's URL.

## Cross-cutting

- [ ] Every list screen (Customers, Invoices, Payments, Interactions) supports search and
      pagination, and an empty result shows a friendly message, not a blank page or error.
- [ ] Every error (validation failure, business rule violation, unauthorized action) shows a
      readable message — never a raw exception, stack trace, or SQL error.
- [ ] Exporting Customers to CSV opens correctly in Excel with no character encoding issues.
