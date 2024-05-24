using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CustomerLedger.Infrastructure.Services;

/// <summary>
/// Demonstrates the read/write separation pattern: non-critical reporting reads prefer the
/// replica connection, checked fresh on every call via IReplicaHealthService, and fall back
/// to the primary connection — logged, never silent — when the replica is unavailable. This
/// project ships a clearly-labeled *simulated* replica (see docs/releases/v5.0.0-Replica.md)
/// rather than requiring native MySQL replication just to exercise this pattern.
/// </summary>
public class ReplicaAwareReportingService : IReplicaAwareReportingService
{
    private readonly IConfiguration _configuration;
    private readonly IReplicaHealthService _replicaHealth;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<ReplicaAwareReportingService> _logger;

    public ReplicaAwareReportingService(
        IConfiguration configuration,
        IReplicaHealthService replicaHealth,
        ICurrentUserContext currentUser,
        ILogger<ReplicaAwareReportingService> logger)
    {
        _configuration = configuration;
        _replicaHealth = replicaHealth;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReplicaAwareResult<IReadOnlyList<BranchRevenueSummaryRow>>> GetBranchRevenueSummaryAsync(CancellationToken cancellationToken = default)
    {
        var replicaHealthy = await _replicaHealth.IsReplicaHealthyAsync(cancellationToken);

        var connectionString = replicaHealthy
            ? _configuration.GetConnectionString("ReplicaConnection") ?? _configuration.GetConnectionString("DefaultConnection")!
            : _configuration.GetConnectionString("DefaultConnection")!;

        if (!replicaHealthy)
        {
            _logger.LogWarning("Replica unavailable — vw_BranchRevenueSummary read falling back to the primary connection.");
        }

        var rows = new List<BranchRevenueSummaryRow>();

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT BranchId, BranchCode, BranchName, TotalCustomers, TotalInvoices,
                   TotalBilled, TotalCollected, TotalOutstanding,
                   PartiallyPaidInvoiceCount, UnpaidInvoiceCount
            FROM vw_BranchRevenueSummary
            WHERE (@branchId IS NULL OR BranchId = @branchId)
            ORDER BY BranchName;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.Add("@branchId", MySqlDbType.Int32).Value =
            (object?)(_currentUser.IsAdministrator ? null : _currentUser.BranchId) ?? DBNull.Value;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new BranchRevenueSummaryRow
            {
                BranchId = reader.GetInt32("BranchId"),
                BranchCode = reader.GetString("BranchCode"),
                BranchName = reader.GetString("BranchName"),
                TotalCustomers = reader.GetInt32("TotalCustomers"),
                TotalInvoices = reader.GetInt32("TotalInvoices"),
                TotalBilled = reader.GetDecimal("TotalBilled"),
                TotalCollected = reader.GetDecimal("TotalCollected"),
                TotalOutstanding = reader.GetDecimal("TotalOutstanding"),
                PartiallyPaidInvoiceCount = reader.GetInt32("PartiallyPaidInvoiceCount"),
                UnpaidInvoiceCount = reader.GetInt32("UnpaidInvoiceCount")
            });
        }

        return new ReplicaAwareResult<IReadOnlyList<BranchRevenueSummaryRow>>
        {
            Data = rows,
            ServedFromReplica = replicaHealthy
        };
    }
}
