using System.ComponentModel.DataAnnotations;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Web.ViewModels;

public class PaymentCreateViewModel
{
    [Required]
    public long InvoiceId { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Payment Number")]
    public string PaymentNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Payment Date")]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow.Date;

    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(100)]
    [Display(Name = "Transaction Reference")]
    public string? TransactionReference { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public Invoice? Invoice { get; set; }
}
