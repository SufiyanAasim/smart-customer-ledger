# Lab: Frontend CRUD

**Goal**: exercise the full customer/invoice/payment CRUD workflow through the browser and
confirm branch isolation.

## Prerequisites

- Application running (`dotnet run --project src/CustomerLedger.Web`) against a live MySQL
  instance with the migration applied.
- An Administrator account (from `SeedAdmin:*`) and at least one Branch Manager/Staff
  account created under Admin → Users, assigned to two different branches.

## Steps

1. **Sign in as Administrator.** Navigate to Admin → Branches, create a second branch.
2. **Create a user** under Admin → Users assigned to Branch A, role Staff. Create a second
   user assigned to Branch B.
3. **Sign out, sign in as the Branch A user.** Navigate to Customers → + New Customer.
   Confirm the Branch dropdown only offers Branch A (non-administrators cannot register a
   customer into a different branch — see `CustomersController.Create`).
4. **Create an invoice** for that customer (Invoices → + New Invoice), add at least one
   item, and Activate it.
5. **Record a payment** against the invoice; confirm `PaymentStatus` updates and the
   customer's account balance (Customers → Details) reflects it.
6. **Branch isolation check**: note the invoice's URL (`/Invoices/Details/{id}`). Sign out,
   sign in as the Branch B user, and navigate to that same URL directly. Expect a
   **403 Forbidden** (`BranchAccessDeniedException` → `Forbid()`), not the invoice.
7. **Log a customer interaction** (Customer Details → Log Interaction) and confirm it
   appears on the Customer Interactions list.
8. **Deactivate the customer** and confirm the status badge updates and the customer no
   longer appears in the default (Active-only) filter.

## Expected outcomes

- Every list screen supports search/filter/sort/pagination.
- Non-administrators never see or reach another branch's data, even via a guessed URL.
- Validation errors (e.g. duplicate CustomerCode) surface as a user-readable message, never
  a raw exception page.

## Automated coverage

`tests/CustomerLedger.IntegrationTests/Services/CustomerServiceTests.cs` and
`InvoiceServiceTests.cs` automate the branch-isolation and validation checks above at the
service layer.
