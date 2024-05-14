# Invoice Transaction Flow

Matches `InvoiceService.CreateDraftAsync` / `ActivateAsync` exactly.

```mermaid
sequenceDiagram
    participant U as Staff/Manager/Admin
    participant IC as InvoicesController
    participant IS as InvoiceService
    participant Calc as InvoiceCalculationService
    participant DB as MySQL (transaction)

    U->>IC: Create invoice (customer, items)
    IC->>IS: CreateDraftAsync(invoice, items)
    IS->>IS: Check branch access, customer active, unique InvoiceNumber
    IS->>Calc: RecalculateInvoiceTotals(invoice)
    Calc-->>IS: Subtotal/Discount/Tax/Total/Outstanding
    IS->>DB: INSERT Invoice (status = Draft)
    DB-->>IS: InvoiceId
    IS-->>IC: Invoice (Draft)
    IC-->>U: Redirect to invoice detail

    Note over U,IC: Staff may still Add/Remove items while Draft

    U->>IC: Activate invoice
    IC->>IS: ActivateAsync(invoiceId)
    IS->>DB: BEGIN TRANSACTION
    IS->>DB: UPDATE Invoice SET InvoiceStatus='Active', ConcurrencyVersion+=1
    IS->>DB: UPDATE CustomerAccount SET TotalBilled+=Total, CurrentBalance=TotalBilled-TotalPaid, ConcurrencyVersion+=1
    IS->>DB: INSERT AuditLog (ActionType='Activate')
    IS->>DB: COMMIT
    DB-->>IS: OK
    IS-->>IC: done
    IC-->>U: "Invoice activated — it can now accept payments"
```

`TotalBilled` is deliberately recognized at **Activate**, not at Draft creation — a Draft
invoice can still be freely edited or cancelled with zero financial consequence, so it must
not count toward the customer's outstanding debt until it becomes real (Active). See
`InvoiceService.CancelAsync` for the symmetric undo when an Active invoice is cancelled.
