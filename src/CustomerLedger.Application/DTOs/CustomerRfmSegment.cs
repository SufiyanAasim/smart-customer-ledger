namespace CustomerLedger.Application.DTOs;

/// <summary>Raw RFM inputs for one customer before scoring.</summary>
public class CustomerRfmInput
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public double RecencyDays { get; init; }
    public int Frequency { get; init; }
    public double Monetary { get; init; }
}

public class CustomerRfmSegment
{
    public int CustomerId { get; init; }
    public string CustomerCode { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public double RecencyDays { get; init; }
    public int Frequency { get; init; }
    public double Monetary { get; init; }

    /// <summary>1 (worst) to 4 (best) per dimension.</summary>
    public int RecencyScore { get; init; }
    public int FrequencyScore { get; init; }
    public int MonetaryScore { get; init; }

    public string SegmentName { get; init; } = string.Empty;
}
