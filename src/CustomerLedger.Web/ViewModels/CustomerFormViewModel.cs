using System.ComponentModel.DataAnnotations;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Web.ViewModels;

public class CustomerFormViewModel
{
    public int CustomerId { get; set; }

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Customer Code")]
    public string CustomerCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Required, Phone, StringLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(20)]
    public string? CNIC { get; set; }

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Display(Name = "Initial Credit Limit")]
    public decimal InitialCreditLimit { get; set; }

    public IEnumerable<Branch>? AvailableBranches { get; set; }
}
