namespace CustomerLedger.Application.DTOs;

/// <summary>
/// Aggregate counters shown on the landing dashboard. Scoped to one branch for
/// Branch Manager/Staff, or organization-wide when requested by an Administrator.
/// </summary>
public class DashboardSummary
{
    public int TotalActiveCustomers { get; init; }
    public int TotalActiveInvoices { get; init; }
    public decimal TotalOutstandingBalance { get; init; }
    public int OverdueInstallmentCount { get; init; }
    public int OpenInteractionCount { get; init; }
    public decimal TodaysCollectedAmount { get; init; }
}
