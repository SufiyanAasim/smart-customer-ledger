namespace CustomerLedger.Domain.Enums;

/// <summary>
/// Used both for Invoice.PaymentStatus (how much of the invoice has been paid) and
/// Payment.PaymentStatus (the lifecycle of an individual payment record).
/// </summary>
public enum PaymentStatus
{
    Unpaid,
    PartiallyPaid,
    Paid,
    Completed,
    Reversed
}
