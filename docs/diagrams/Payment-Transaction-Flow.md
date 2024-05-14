# Payment Transaction Flow

Matches `PaymentService.RecordPaymentAsync` exactly — see also
`database/transactions/PaymentTransaction.sql` and `ConcurrentPaymentTests`.

```mermaid
sequenceDiagram
    participant U as Staff
    participant PC as PaymentsController
    participant PS as PaymentService
    participant DB as MySQL (transaction)

    U->>PC: Record payment (invoiceId, amount)
    PC->>PS: RecordPaymentAsync(payment)
    PS->>DB: BEGIN TRANSACTION
    PS->>DB: SELECT * FROM Invoices WHERE InvoiceId=? FOR UPDATE
    Note right of DB: Row lock held — any other transaction<br/>touching this same invoice blocks here.
    DB-->>PS: Invoice row (locked)
    PS->>PS: Validate: Active status, Amount>0, Amount<=Outstanding
    alt Validation fails
        PS-->>PC: BusinessRuleException
        PC-->>U: Error message, no data changed
    else Validation passes
        PS->>DB: INSERT Payment (status='Completed')
        PS->>DB: UPDATE Invoice SET PaidAmount+=Amount, OutstandingAmount=Total-Paid,\nPaymentStatus=(Paid|PartiallyPaid), ConcurrencyVersion+=1
        PS->>DB: UPDATE CustomerAccount SET TotalPaid+=Amount, CurrentBalance=Billed-Paid, ConcurrencyVersion+=1
        PS->>DB: INSERT AuditLog (ActionType='Create')
        PS->>DB: COMMIT
        DB-->>PS: OK
        PS-->>PC: Payment
        PC-->>U: "Payment recorded successfully"
    end
```

## Isolation under concurrency

```mermaid
sequenceDiagram
    participant A as Session A
    participant B as Session B
    participant DB as MySQL

    A->>DB: BEGIN; SELECT ... FOR UPDATE (Invoice #1)
    activate DB
    Note over DB: Lock held by A
    B->>DB: BEGIN; SELECT ... FOR UPDATE (Invoice #1)
    Note over B: Blocks — waits for A's lock
    A->>DB: UPDATE Invoice/Account; COMMIT
    deactivate DB
    Note over DB: Lock released
    DB-->>B: Now proceeds, sees A's committed OutstandingAmount
    B->>B: Re-validate payment.Amount <= (now-current) OutstandingAmount
    alt Would overpay
        B-->>B: BusinessRuleException — rejected
    else Still valid
        B->>DB: UPDATE Invoice/Account; COMMIT
    end
```

`ConcurrentPaymentTests.TwoConcurrentPayments_ThatWouldJointlyOverpay_OnlyOneSucceeds` is
the automated test proving this exact sequence.
