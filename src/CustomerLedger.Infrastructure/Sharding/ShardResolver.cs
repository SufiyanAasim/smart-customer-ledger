using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CustomerLedger.Infrastructure.Sharding;

/// <summary>
/// Reads the shard registry from configuration (ShardSettings:Shards) and routes a branch
/// to a shard via `branchId % activeShardCount` — the "simple academic routing example"
/// named in the project specification. Shards are ordered by ShardId before the modulus is
/// applied, so routing is stable across process restarts as long as the registry itself
/// doesn't change.
///
/// Known limitation, documented rather than hidden: adding or removing a shard changes
/// activeShardCount, which reassigns most existing branches to a different shard under
/// plain modulus routing — a real production system would use consistent hashing or an
/// explicit, persisted branch-to-shard map to avoid that. See
/// docs/releases/v6.0.0-Shard.md's Rebalancing section.
/// </summary>
public class ShardResolver : IShardResolver
{
    private readonly IReadOnlyList<ShardDescriptor> _allShards;
    private readonly IReadOnlyList<ShardDescriptor> _activeShards;

    public ShardResolver(IConfiguration configuration)
    {
        _allShards = configuration.GetSection("ShardSettings:Shards")
            .Get<List<ShardDescriptor>>() ?? new List<ShardDescriptor>();

        _activeShards = _allShards
            .Where(s => s.IsActive)
            .OrderBy(s => s.ShardId, StringComparer.Ordinal)
            .ToList();
    }

    public ShardDescriptor ResolveForBranch(int branchId)
    {
        if (_activeShards.Count == 0)
        {
            throw new BusinessRuleException("No active shards are configured (ShardSettings:Shards).");
        }

        var index = ((branchId % _activeShards.Count) + _activeShards.Count) % _activeShards.Count;
        return _activeShards[index];
    }

    public IReadOnlyCollection<ShardDescriptor> GetAllShards() => _allShards;
}
