using CustomerLedger.Domain.Enums;

namespace CustomerLedger.Domain.Entities;

/// <summary>
/// Overdue status is not set automatically by the mere passage of time — a scheduled
/// mechanism (background service or MySQL event) must evaluate DueDate vs. today and
/// transition Pending rows to Overdue. See vw_OverdueInstallments for the computed view
/// used until that scheduler ships.
/// </summary>
public class InstallmentSchedule
{
    public long InstallmentScheduleId { get; set; }
    public long InstallmentPlanId { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public InstallmentPlan InstallmentPlan { get; set; } = null!;
}
