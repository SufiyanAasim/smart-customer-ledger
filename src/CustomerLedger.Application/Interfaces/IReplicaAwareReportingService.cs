using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Read-only reporting queries that prefer the replica connection and transparently fall
/// back to the primary when the replica is unhealthy — writes never go through this
/// interface (see docs/releases/v5.0.0-Replica.md for the read/write routing rules).
/// </summary>
public interface IReplicaAwareReportingService
{
    Task<ReplicaAwareResult<IReadOnlyList<BranchRevenueSummaryRow>>> GetBranchRevenueSummaryAsync(CancellationToken cancellationToken = default);
}
