using CustomerLedger.Application.Interfaces;
using CustomerLedger.Infrastructure.Data.Contexts;
using Microsoft.Extensions.Logging;

namespace CustomerLedger.Infrastructure.Services;

public class ReplicaHealthService : IReplicaHealthService
{
    private readonly ReplicaDbContext _replicaDb;
    private readonly ILogger<ReplicaHealthService> _logger;

    public ReplicaHealthService(ReplicaDbContext replicaDb, ILogger<ReplicaHealthService> logger)
    {
        _replicaDb = replicaDb;
        _logger = logger;
    }

    public async Task<bool> IsReplicaHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));
            return await _replicaDb.Database.CanConnectAsync(timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Replica health check failed — reads will fall back to the primary connection.");
            return false;
        }
    }
}
