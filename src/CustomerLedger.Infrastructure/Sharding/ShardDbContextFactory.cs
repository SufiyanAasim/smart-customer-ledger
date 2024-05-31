using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Exceptions;
using CustomerLedger.Application.Interfaces;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CustomerLedger.Infrastructure.Sharding;

public class ShardDbContextFactory : IShardDbContextFactory
{
    private readonly IConfiguration _configuration;
    private readonly IShardResolver _shardResolver;

    public ShardDbContextFactory(IConfiguration configuration, IShardResolver shardResolver)
    {
        _configuration = configuration;
        _shardResolver = shardResolver;
    }

    public ApplicationDbContext CreateForBranch(int branchId)
    {
        var shard = _shardResolver.ResolveForBranch(branchId);
        return CreateForShard(shard);
    }

    public ApplicationDbContext CreateForShard(ShardDescriptor shard)
    {
        var connectionString = _configuration.GetConnectionString(shard.ConnectionStringName)
            ?? throw new BusinessRuleException(
                $"No connection string named '{shard.ConnectionStringName}' is configured for shard '{shard.ShardId}'.");

        var mySqlVersionSetting = _configuration["MySqlServerVersion"] ?? "8.0.36";
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(mySqlVersionSetting)))
            .Options;

        return new ApplicationDbContext(options);
    }
}
