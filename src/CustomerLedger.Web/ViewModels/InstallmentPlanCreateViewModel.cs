using System.ComponentModel.DataAnnotations;
using CustomerLedger.Domain.Entities;
using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Web.ViewModels;

public class InstallmentPlanCreateViewModel
{
    [Required]
    public long InvoiceId { get; set; }

    [Range(1, 60, ErrorMessage = "Number of installments must be between 1 and 60.")]
    [Display(Name = "Number of Installments")]
    public int NumberOfInstallments { get; set; } = 3;

    [Range(0, double.MaxValue)]
    [Display(Name = "Down Payment")]
    public decimal DownPayment { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [Required, DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddMonths(3);

    [Required]
    public InstallmentFrequency Frequency { get; set; } = InstallmentFrequency.Monthly;

    public Invoice? Invoice { get; set; }
}
