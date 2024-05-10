using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>branchId null means organization-wide — only honored for Administrators; the implementation re-derives scope from ICurrentUserContext regardless of what is passed.</summary>
    Task<DashboardSummary> GetSummaryAsync(int? branchId, CancellationToken cancellationToken = default);
}
