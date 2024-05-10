using Microsoft.AspNetCore.Identity;

namespace CustomerLedger.Domain.Entities;

/// <summary>
/// Extends ASP.NET Core Identity's user with branch assignment and employee metadata.
/// BranchId is nullable to allow organization-wide Administrator accounts.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }

    public Branch? Branch { get; set; }
}
