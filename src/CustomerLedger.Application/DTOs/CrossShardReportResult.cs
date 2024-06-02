namespace CustomerLedger.Application.DTOs;

public class ShardFailure
{
    public string ShardId { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}

/// <summary>
/// The result of querying every active shard independently. Never pretends a distributed
/// join happened — each shard is queried on its own, successes are aggregated, and any
/// shard that failed is reported explicitly rather than silently dropped from the total.
/// </summary>
public class CrossShardReportResult<T>
{
    public required IReadOnlyList<T> Data { get; init; }
    public required IReadOnlyList<string> ShardsQueried { get; init; }
    public required IReadOnlyList<ShardFailure> ShardFailures { get; init; }
    public bool IsComplete => ShardFailures.Count == 0;
}
