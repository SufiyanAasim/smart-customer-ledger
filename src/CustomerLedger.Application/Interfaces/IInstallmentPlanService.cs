using CustomerLedger.Domain.Entities;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Index-level installment foundation: creating a plan and generating its schedule rows.
/// Approval workflow enforcement and payment-driven schedule updates are Balance-release scope.
/// </summary>
public interface IInstallmentPlanService
{
    Task<InstallmentPlan?> GetByInvoiceIdAsync(long invoiceId, CancellationToken cancellationToken = default);
    Task<InstallmentPlan?> GetByIdAsync(long installmentPlanId, CancellationToken cancellationToken = default);
    Task<InstallmentPlan> CreateAsync(InstallmentPlan plan, CancellationToken cancellationToken = default);
    Task ApproveAsync(long installmentPlanId, string approvedByUserId, CancellationToken cancellationToken = default);
    Task CancelAsync(long installmentPlanId, CancellationToken cancellationToken = default);
}
