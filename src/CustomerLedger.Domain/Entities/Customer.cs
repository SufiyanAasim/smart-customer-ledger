using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

/// <summary>
/// Customers with financial history are never physically deleted — IsDeleted (soft delete)
/// or Status = Inactive is used instead, so invoices/payments retain a valid owner.
/// </summary>
public class Customer
{
    public int CustomerId { get; set; }
    public int BranchId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CNIC { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Branch Branch { get; set; } = null!;
    public CustomerAccount? CustomerAccount { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<CustomerInteraction> Interactions { get; set; } = new List<CustomerInteraction>();
}
