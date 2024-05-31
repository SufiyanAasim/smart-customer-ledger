using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Aggregates a report across every active shard. Administrator-only — cross-shard
/// visibility is an organization-wide capability, not a branch-scoped one.
/// </summary>
public interface ICrossShardReportingService
{
    Task<CrossShardReportResult<BranchRevenueSummaryRow>> GetBranchRevenueSummaryAcrossShardsAsync(CancellationToken cancellationToken = default);
}
