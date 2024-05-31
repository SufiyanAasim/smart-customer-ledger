using CustomerLedger.Application.DTOs;
using CustomerLedger.Infrastructure.Data.Contexts;

namespace CustomerLedger.Infrastructure.Sharding;

/// <summary>
/// Creates an ApplicationDbContext instance pointed at a specific shard's connection
/// string. Unlike ReplicaDbContext (registered once, pooled, for the whole app lifetime),
/// shard contexts are created per-call because which shard is needed depends on runtime
/// data (a branch id) rather than being known at startup. Lives in Infrastructure (not
/// Application) because it exposes ApplicationDbContext directly, an EF Core/Infrastructure
/// concern — Application only ever sees ICrossShardReportingService's plain DTOs.
/// </summary>
public interface IShardDbContextFactory
{
    ApplicationDbContext CreateForBranch(int branchId);
    ApplicationDbContext CreateForShard(ShardDescriptor shard);
}
