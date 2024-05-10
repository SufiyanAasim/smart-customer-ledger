namespace CustomerLedger.Domain.Entities;

/// <summary>
/// LineTotal = (Quantity * UnitPrice) - DiscountAmount + TaxAmount. Calculated by
/// InvoiceCalculationService, never entered directly — see Application/Services.
/// </summary>
public class InvoiceItem
{
    public long InvoiceItemId { get; set; }
    public long InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
