# Payment Reversal Flow

Matches `PaymentService.ReverseAsync` exactly — see also
`database/transactions/PaymentReversal.sql`.

```mermaid
sequenceDiagram
    participant U as Admin/Manager
    participant PC as PaymentsController
    participant PS as PaymentService
    participant DB as MySQL (transaction)

    U->>PC: Reverse payment (paymentId, reason)
    PC->>PS: ReverseAsync(paymentId, reason)
    PS->>PS: Require non-empty reason
    PS->>DB: BEGIN TRANSACTION
    PS->>DB: SELECT Payment WHERE PaymentId=?
    PS->>PS: Validate: status=Completed, not already reversed
    alt Invalid
        PS-->>PC: BusinessRuleException
    else Valid
        PS->>DB: SELECT * FROM Invoices WHERE InvoiceId=? FOR UPDATE
        PS->>DB: UPDATE original Payment SET Status='Reversed', ReversalReason=?
        PS->>DB: INSERT new Payment (Status='Reversed', ReversedPaymentId=original.Id)
        PS->>DB: UPDATE Invoice SET PaidAmount-=Amount, OutstandingAmount recalculated,\nPaymentStatus recalculated, ConcurrencyVersion+=1
        PS->>DB: UPDATE CustomerAccount SET TotalPaid-=Amount, CurrentBalance recalculated
        PS->>DB: INSERT AuditLog (ActionType='Reverse', OldValuesJson=original status)
        PS->>DB: COMMIT
        PS-->>PC: reversal Payment
        PC-->>U: "Payment reversed successfully"
    end
```

The original payment row is **never deleted** — it is marked `Reversed` and a second,
linked row records the reversal, so the full transaction history remains traceable end to
end. `PaymentReversalTests.ReverseAsync_CalledTwice_ThrowsBusinessRuleException` proves a
payment cannot be reversed more than once.
