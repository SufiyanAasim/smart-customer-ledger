namespace CustomerLedger.Application.DTOs;

/// <summary>Maps 1:1 to vw_BranchRevenueSummary's columns.</summary>
public class BranchRevenueSummaryRow
{
    public int BranchId { get; init; }
    public string BranchCode { get; init; } = string.Empty;
    public string BranchName { get; init; } = string.Empty;
    public int TotalCustomers { get; init; }
    public int TotalInvoices { get; init; }
    public decimal TotalBilled { get; init; }
    public decimal TotalCollected { get; init; }
    public decimal TotalOutstanding { get; init; }
    public int PartiallyPaidInvoiceCount { get; init; }
    public int UnpaidInvoiceCount { get; init; }
}

/// <summary>Wraps any replica-aware read result with which connection actually served it, so the UI/logs can show the fallback transparently rather than silently.</summary>
public class ReplicaAwareResult<T>
{
    public required T Data { get; init; }
    public required bool ServedFromReplica { get; init; }
}
