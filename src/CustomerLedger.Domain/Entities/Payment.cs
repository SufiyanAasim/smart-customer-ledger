using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

/// <summary>
/// Completed payments are never physically deleted. A reversal is represented by a second
/// Payment row linked back via ReversedPaymentId, keeping the original traceable — see the
/// Payment Reversal Transaction workflow introduced in the Balance release.
/// </summary>
public class Payment
{
    public long PaymentId { get; set; }
    public long InvoiceId { get; set; }
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Completed;
    public string ReceivedByUserId { get; set; } = string.Empty;
    public long? ReversedPaymentId { get; set; }
    public string? ReversalReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ApplicationUser ReceivedByUser { get; set; } = null!;
    public Payment? ReversedPayment { get; set; }
}
