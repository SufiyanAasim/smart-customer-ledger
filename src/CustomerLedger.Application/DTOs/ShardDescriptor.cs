namespace CustomerLedger.Application.DTOs;

/// <summary>One logical shard: a named MySQL database reachable via a named connection string in configuration.</summary>
public sealed record ShardDescriptor(
    string ShardId,
    string Name,
    string ConnectionStringName,
    bool IsActive);
