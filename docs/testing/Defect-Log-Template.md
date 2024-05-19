# Defect Log Template

Copy the row template below for each new defect found during manual testing or grading.
Keep this file as a running log — do not delete resolved entries, mark them Closed instead.

| ID | Date Found | Found By | Severity | Area | Description | Steps to Reproduce | Status | Resolution |
|---|---|---|---|---|---|---|---|---|
| DEF-001 | *(example)* 2026-08-01 | *(name)* | Medium | Payments | *(what went wrong)* | *(exact steps)* | Open / In Progress / Closed | *(what fixed it, or "won't fix" + why)* |

## Severity guide

- **Critical**: data corruption, security vulnerability, or a crash blocking core workflow.
- **High**: a core workflow (invoice, payment, installment) produces an incorrect result.
- **Medium**: a secondary feature (export, audit log, reconciliation) misbehaves.
- **Low**: cosmetic/UI issue with no functional impact.

## Real defects found and fixed during this project

These are documented here for traceability, not left implicit in commit history alone:

| ID | Date | Severity | Area | Description | Resolution |
|---|---|---|---|---|---|
| DEF-000 | v2.0.0 development | High | CustomerAccount sync | `TotalBilled`/`TotalPaid`/`CurrentBalance` were never updated by any Index-era code path — optimistic concurrency (`ConcurrencyVersion`) was configured but never incremented, so it could never detect a conflict either. | Fixed in Balance: `InvoiceService.ActivateAsync`/`CancelAsync` and `PaymentService.RecordPaymentAsync`/`ReverseAsync` now sync the account and increment `ConcurrencyVersion` on every mutation. See [docs/releases/v2.0.0-Balance.md § Fixed](../releases/v2.0.0-Balance.md). |
