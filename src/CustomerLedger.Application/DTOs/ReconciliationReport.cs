namespace CustomerLedger.Application.DTOs;

public class ReconciliationReport
{
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal PreviousTotalBilled { get; init; }
    public decimal RecalculatedTotalBilled { get; init; }
    public decimal PreviousTotalPaid { get; init; }
    public decimal RecalculatedTotalPaid { get; init; }
    public decimal PreviousCurrentBalance { get; init; }
    public decimal RecalculatedCurrentBalance { get; init; }
    public bool HadMismatch =>
        PreviousTotalBilled != RecalculatedTotalBilled ||
        PreviousTotalPaid != RecalculatedTotalPaid ||
        PreviousCurrentBalance != RecalculatedCurrentBalance;
}
