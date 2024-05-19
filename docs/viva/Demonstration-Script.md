# Demonstration Script

A suggested 15–20 minute walkthrough order for a live demo/viva.

## 1. Architecture (2 min)

Show `src/` folder structure — Domain/Application/Infrastructure/Web — and explain the
dependency direction (see [System-Architecture.md](../diagrams/System-Architecture.md)).

## 2. Database schema (3 min)

Open MySQL Workbench, show the tables, run
`database/verification/VerifySchema.sql`. Show the ER diagram
([ER-Diagram.md](../diagrams/ER-Diagram.md)) alongside the real schema.

## 3. Sign in and dashboard (1 min)

Sign in as Administrator. Point out the dashboard tiles and the branch badge in the navbar.

## 4. Customer → Invoice → Payment happy path (5 min)

1. Register a customer.
2. Create a Draft invoice with 2 line items, show the total recalculate live.
3. Activate the invoice.
4. Record a partial payment, show `PartiallyPaid` status.
5. Record the remaining payment, show `Paid` status.
6. Open the customer's page, show the account balance updated.

## 5. Branch isolation (2 min)

Sign in as a Staff user of a different branch. Attempt to open the invoice from step 4
directly by URL. Show the 403.

## 6. Payment reversal (2 min)

Sign back in as Administrator. Reverse one of the payments from step 4 with a reason. Show
the invoice balance restored and the original payment marked Reversed (not deleted).

## 7. Installment plan (2 min)

Create a new invoice, activate it, create an installment plan, approve it, pay one
installment row, show the schedule update.

## 8. Views and triggers (2 min)

In MySQL Workbench: `SELECT * FROM vw_CustomerAccountSummary;`. Then attempt
`DELETE FROM Payments WHERE PaymentId = <id>;` and show the trigger rejecting it.

## 9. ACID / concurrency (2 min)

Run `dotnet test --filter FullyQualifiedName~ConcurrentPaymentTests` and explain what it
proves (see [ACID-Transaction-Lab.md](../labs/ACID-Transaction-Lab.md)).

## 10. Backup/export (2 min)

Admin → Backup History → Run Backup Now → show the Completed row and file size. Customers →
Export CSV → open in Excel.

## 11. Wrap-up (1 min)

Show `docs/releases/` — the six-release roadmap and what's actually shipped so far, and the
honest "what was/wasn't verified in this environment" sections in each release document.
