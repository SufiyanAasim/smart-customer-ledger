namespace CustomerLedger.Application.DTOs;

public class CustomerImportRowResult
{
    public int RowNumber { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool Accepted { get; init; }
    public string? RejectionReason { get; init; }
}

public class CustomerImportResult
{
    public IReadOnlyList<CustomerImportRowResult> Rows { get; init; } = Array.Empty<CustomerImportRowResult>();
    public int AcceptedCount => Rows.Count(r => r.Accepted);
    public int RejectedCount => Rows.Count(r => !r.Accepted);

    /// <summary>True once ImportService.ImportAsync has actually written the accepted rows — a preview-only call leaves this false so the caller knows nothing was persisted yet.</summary>
    public bool WasCommitted { get; init; }
}
