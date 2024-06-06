namespace CustomerLedger.Application.DTOs;

/// <summary>Raw, human-readable feature values before z-score normalization — kept alongside the score so the UI/viva can show *why* a customer scored the way they did.</summary>
public class CustomerRiskFeatures
{
    public int CustomerId { get; init; }
    public double CreditUtilization { get; init; }
    public double UnpaidInvoiceRatio { get; init; }
    public double AverageInvoiceAmount { get; init; }
    public double TotalOutstanding { get; init; }
    public double CustomerAgeDays { get; init; }

    /// <summary>Training-time only: the heuristic ground-truth label (1 = currently shows financial-distress signals). Null when this instance represents a customer being scored, not a training example.</summary>
    public double? Label { get; init; }

    public double[] ToVector() => new[] { CreditUtilization, UnpaidInvoiceRatio, AverageInvoiceAmount, TotalOutstanding, CustomerAgeDays };
}

public class CustomerRiskScore
{
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerCode { get; init; } = string.Empty;
    public double RiskProbability { get; init; }
    public CustomerRiskFeatures Features { get; init; } = new();

    public string RiskBand => RiskProbability switch
    {
        >= 0.7 => "High",
        >= 0.4 => "Medium",
        _ => "Low"
    };
}
