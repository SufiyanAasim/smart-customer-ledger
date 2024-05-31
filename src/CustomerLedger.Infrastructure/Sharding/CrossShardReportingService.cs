using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CustomerLedger.Infrastructure.Sharding;

/// <summary>
/// Queries vw_BranchRevenueSummary against every active shard's own connection
/// independently (MySQL has no native cross-server JOIN for this) and aggregates the rows.
/// A shard that times out or errors is recorded as a ShardFailure and excluded from Data —
/// it never silently vanishes from the result with no trace, and it never aborts the other
/// shards' queries (each shard's query is wrapped in its own try/catch).
/// </summary>
public class CrossShardReportingService : ICrossShardReportingService
{
    private static readonly TimeSpan PerShardTimeout = TimeSpan.FromSeconds(10);

    private readonly IConfiguration _configuration;
    private readonly IShardResolver _shardResolver;
    private readonly ILogger<CrossShardReportingService> _logger;

    public CrossShardReportingService(IConfiguration configuration, IShardResolver shardResolver, ILogger<CrossShardReportingService> logger)
    {
        _configuration = configuration;
        _shardResolver = shardResolver;
        _logger = logger;
    }

    public async Task<CrossShardReportResult<BranchRevenueSummaryRow>> GetBranchRevenueSummaryAcrossShardsAsync(CancellationToken cancellationToken = default)
    {
        var shards = _shardResolver.GetAllShards().Where(s => s.IsActive).ToList();

        var data = new List<BranchRevenueSummaryRow>();
        var failures = new List<ShardFailure>();
        var queried = new List<string>();

        foreach (var shard in shards)
        {
            queried.Add(shard.ShardId);

            try
            {
                var connectionString = _configuration.GetConnectionString(shard.ConnectionStringName)
                    ?? throw new InvalidOperationException($"No connection string named '{shard.ConnectionStringName}' configured.");

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(PerShardTimeout);

                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync(timeoutCts.Token);

                const string sql = """
                    SELECT BranchId, BranchCode, BranchName, TotalCustomers, TotalInvoices,
                           TotalBilled, TotalCollected, TotalOutstanding,
                           PartiallyPaidInvoiceCount, UnpaidInvoiceCount
                    FROM vw_BranchRevenueSummary;
                    """;

                await using var command = new MySqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(timeoutCts.Token);

                while (await reader.ReadAsync(timeoutCts.Token))
                {
                    data.Add(new BranchRevenueSummaryRow
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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cross-shard report: shard {ShardId} failed and was excluded from the aggregate.", shard.ShardId);
                failures.Add(new ShardFailure { ShardId = shard.ShardId, ErrorMessage = ex.Message });
            }
        }

        return new CrossShardReportResult<BranchRevenueSummaryRow>
        {
            Data = data,
            ShardsQueried = queried,
            ShardFailures = failures
        };
    }
}
