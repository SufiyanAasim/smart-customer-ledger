using System.ComponentModel.DataAnnotations;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Web.ViewModels;

public class InvoiceCreateViewModel
{
    [Required]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required, StringLength(30)]
    [Display(Name = "Invoice Number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Invoice Date")]
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow.Date;

    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    public List<InvoiceItemInputModel> Items { get; set; } = new() { new InvoiceItemInputModel() };

    public Customer? Customer { get; set; }
}
