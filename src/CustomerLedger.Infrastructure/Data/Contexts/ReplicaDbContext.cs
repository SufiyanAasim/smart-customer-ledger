using Microsoft.EntityFrameworkCore;

namespace CustomerLedger.Infrastructure.Data.Contexts;

/// <summary>
/// A read-only-by-convention context sharing ApplicationDbContext's exact model, pointed at
/// the replica connection string (ConnectionStrings:ReplicaConnection). A distinct class
/// exists purely so both can be registered in DI simultaneously with different connection
/// strings — the standard EF Core pattern for two contexts, one model. Callers must never
/// call SaveChanges on this context; see IReplicaHealthService/IReplicaAwareReportingService
/// for the only intended usage pattern (AsNoTracking report queries with primary fallback).
/// </summary>
public class ReplicaDbContext : ApplicationDbContext
{
    public ReplicaDbContext(DbContextOptions<ReplicaDbContext> options)
        : base(options)
    {
    }
}
