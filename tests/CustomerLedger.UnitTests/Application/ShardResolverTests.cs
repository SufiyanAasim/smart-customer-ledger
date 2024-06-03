using CustomerLedger.Application.Exceptions;
using CustomerLedger.Infrastructure.Sharding;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CustomerLedger.UnitTests.Application;

public class ShardResolverTests
{
    private static ShardResolver CreateResolver(params (string ShardId, bool IsActive)[] shards)
    {
        var data = new Dictionary<string, string?>();
        for (var i = 0; i < shards.Length; i++)
        {
            data[$"ShardSettings:Shards:{i}:ShardId"] = shards[i].ShardId;
            data[$"ShardSettings:Shards:{i}:Name"] = shards[i].ShardId;
            data[$"ShardSettings:Shards:{i}:ConnectionStringName"] = $"{shards[i].ShardId}Connection";
            data[$"ShardSettings:Shards:{i}:IsActive"] = shards[i].IsActive.ToString();
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(data).Build();
        return new ShardResolver(configuration);
    }

    [Fact]
    public void ResolveForBranch_SameBranchId_AlwaysResolvesToSameShard()
    {
        var resolver = CreateResolver(("shard-01", true), ("shard-02", true));

        var first = resolver.ResolveForBranch(42);
        var second = resolver.ResolveForBranch(42);

        Assert.Equal(first.ShardId, second.ShardId);
    }

    [Fact]
    public void ResolveForBranch_DifferentBranches_CanResolveToDifferentShards()
    {
        var resolver = CreateResolver(("shard-01", true), ("shard-02", true));

        var shardForEven = resolver.ResolveForBranch(2);
        var shardForOdd = resolver.ResolveForBranch(1);

        Assert.NotEqual(shardForEven.ShardId, shardForOdd.ShardId);
    }

    [Fact]
    public void ResolveForBranch_IgnoresInactiveShards()
    {
        var resolver = CreateResolver(("shard-01", true), ("shard-02", false));

        var resolved = resolver.ResolveForBranch(1);

        Assert.Equal("shard-01", resolved.ShardId);
    }

    [Fact]
    public void ResolveForBranch_NoActiveShards_ThrowsBusinessRuleException()
    {
        var resolver = CreateResolver(("shard-01", false));

        Assert.Throws<BusinessRuleException>(() => resolver.ResolveForBranch(1));
    }

    [Fact]
    public void GetAllShards_ReturnsBothActiveAndInactive()
    {
        var resolver = CreateResolver(("shard-01", true), ("shard-02", false));

        var all = resolver.GetAllShards();

        Assert.Equal(2, all.Count);
    }
}
