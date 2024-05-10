using System.ComponentModel.DataAnnotations;
using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Web.ViewModels;

public class UserFormViewModel
{
    public string? Id { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [Display(Name = "Employee Code")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Display(Name = "Branch")]
    public int? BranchId { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Password (leave blank to keep unchanged)")]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<Branch>? AvailableBranches { get; set; }
    public IEnumerable<string>? AvailableRoles { get; set; }
}
