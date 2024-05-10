using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

public class Invoice
{
    public long InvoiceId { get; set; }
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public InvoiceStatus InvoiceStatus { get; set; } = InvoiceStatus.Draft;
    public string CreatedByUserId { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public uint ConcurrencyVersion { get; set; }

    public Customer Customer { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public InstallmentPlan? InstallmentPlan { get; set; }
}
