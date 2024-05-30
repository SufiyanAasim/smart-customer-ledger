using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Interfaces;

/// <summary>
/// Resolves which logical shard owns a given branch's data. All shard-selection logic lives
/// behind this one interface — no controller or service computes a modulus or picks a
/// connection string itself, so the routing rule changes in exactly one place if it ever
/// needs to (see docs/releases/v6.0.0-Shard.md for the rebalancing implications of that).
/// </summary>
public interface IShardResolver
{
    /// <summary>Deterministic: the same BranchId always resolves to the same shard for a given, unchanged shard registry.</summary>
    ShardDescriptor ResolveForBranch(int branchId);

    IReadOnlyCollection<ShardDescriptor> GetAllShards();
}
