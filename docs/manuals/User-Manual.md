# User Manual (Cashier / Staff and Branch Manager)

## Signing in

Navigate to the application URL, sign in with the email/password an Administrator created
for you. There is no self-registration — if you don't have an account, ask your
Administrator.

## Dashboard

Shows your branch's active customer/invoice counts, outstanding balance, overdue
installment count, open interactions, and today's collected amount. Administrators see an
organization-wide (or branch-filtered) view.

## Registering a customer

Customers → **+ New Customer** → fill in the required fields (Code, Name, Phone, Address,
City) → **Register Customer**. A financial account is created automatically with the
credit limit you specify (default 0).

## Creating and activating an invoice

1. From a customer's page, click **New Invoice**.
2. Fill in the invoice number (a suggested one is pre-filled) and date.
3. Add one or more item rows (Description, Quantity, Unit Price, Discount, Tax) using
   **+ Add Item Row**.
4. Click **Create Draft Invoice**.
5. On the invoice page, add/remove items as needed — this is only possible while the
   invoice is **Draft**.
6. Click **Activate Invoice** once ready. After this, items can no longer be changed, and
   the invoice starts contributing to the customer's outstanding balance.

## Recording a payment

From an Active invoice's page, click **Record Payment**, enter the amount (defaults to the
full outstanding balance), method, and optional reference, then **Record Payment**.

## Installment plans

From an Active invoice, click **Create Installment Plan**, set the number of installments,
down payment, frequency, and date range. The plan starts **Pending Approval** — an
Administrator or Branch Manager must approve it before payments can be taken against it.
Once approved, each due row shows a **Pay** button on the plan's detail page.

## Reversing a payment (Administrator / Branch Manager only)

From a payment's detail page, if you have permission, a **Reverse This Payment** form
appears — a reason is required. The original payment is never deleted; a linked reversal
record is created instead.

## Customer interactions

From a customer's page, click **Log Interaction**, choose a type (call, complaint,
reminder, etc.), and fill in the subject/description. Set a follow-up date if one is
needed.

## Exporting data

Customers, Invoices, and Payments list pages each have export buttons (CSV, and JSON for
Customers). A customer's page also offers a combined **Account Statement (CSV)**.

## Importing customers

Customers → **Import CSV** → choose your branch and file → **Preview** (nothing is saved
yet) → review accepted/rejected rows → re-select the same file and **Confirm Import**.
