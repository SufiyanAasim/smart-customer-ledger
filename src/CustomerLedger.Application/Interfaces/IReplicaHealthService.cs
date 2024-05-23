namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Checks whether the replica connection is actually reachable right now. Used by
/// IReplicaAwareReportingService to decide, per call, whether to read from the replica or
/// fall back to the primary — never assumed healthy without checking.
/// </summary>
public interface IReplicaHealthService
{
    Task<bool> IsReplicaHealthyAsync(CancellationToken cancellationToken = default);
}
