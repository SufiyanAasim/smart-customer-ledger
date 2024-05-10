using System.ComponentModel.DataAnnotations;

namespace CustomerLedger.Web.ViewModels;

public class InvoiceItemInputModel
{
    [Required, StringLength(300)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Discount")]
    public decimal DiscountAmount { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Tax")]
    public decimal TaxAmount { get; set; }
}
