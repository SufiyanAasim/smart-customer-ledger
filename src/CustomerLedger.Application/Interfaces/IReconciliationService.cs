using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Recalculates a customer account's totals from source rows (active invoices, completed
/// non-reversed payments) and corrects any drift — the authorized recovery path if
/// TotalBilled/TotalPaid/CurrentBalance ever fall out of step with reality.
/// </summary>
public interface IReconciliationService
{
    Task<ReconciliationReport> ReconcileCustomerAccountAsync(int customerId, CancellationToken cancellationToken = default);

    /// <summary>Reconciles every customer account in a branch (Administrator/Branch Manager use) and returns one report per account.</summary>
    Task<IReadOnlyList<ReconciliationReport>> ReconcileBranchAsync(int branchId, CancellationToken cancellationToken = default);
}
