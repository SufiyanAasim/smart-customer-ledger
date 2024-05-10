using System.ComponentModel.DataAnnotations;

namespace CustomerLedger.Web.ViewModels;

public class BranchFormViewModel
{
    public int BranchId { get; set; }

    [Required, StringLength(20)]
    [Display(Name = "Branch Code")]
    public string BranchCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Required, Phone, StringLength(20)]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;
}
