using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

public class InstallmentPlan
{
    public long InstallmentPlanId { get; set; }
    public long InvoiceId { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal TotalInstallmentAmount { get; set; }
    public decimal DownPayment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public InstallmentFrequency Frequency { get; set; }
    public InstallmentPlanStatus Status { get; set; } = InstallmentPlanStatus.PendingApproval;
    public string? ApprovedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public ApplicationUser? ApprovedByUser { get; set; }
    public ICollection<InstallmentSchedule> Schedules { get; set; } = new List<InstallmentSchedule>();
}
