using CustomerLedger.Application.DTOs;

namespace CustomerLedger.Application.Services;

/// <summary>
/// Classic RFM (Recency, Frequency, Monetary) customer segmentation — a standard,
/// widely-taught data-mining technique for grouping customers by behavior rather than
/// predicting an outcome (contrast with LogisticRegressionModel, which predicts). Each
/// dimension is bucketed into quartiles (1 = worst, 4 = best) computed from the customer
/// set actually passed in, then combined via a small rule table into a named segment.
/// </summary>
public static class RfmSegmenter
{
    public static IReadOnlyList<CustomerRfmSegment> Segment(IReadOnlyList<CustomerRfmInput> customers)
    {
        if (customers.Count == 0)
        {
            return Array.Empty<CustomerRfmSegment>();
        }

        // Recency: FEWER days since last activity is better, so its quartile scoring is
        // inverted relative to Frequency/Monetary (where a HIGHER raw value is better).
        var recencyThresholds = Quartiles(customers.Select(c => c.RecencyDays).ToList());
        var frequencyThresholds = Quartiles(customers.Select(c => (double)c.Frequency).ToList());
        var monetaryThresholds = Quartiles(customers.Select(c => c.Monetary).ToList());

        return customers.Select(c =>
        {
            var recencyScore = ScoreInverted(c.RecencyDays, recencyThresholds);
            var frequencyScore = Score(c.Frequency, frequencyThresholds);
            var monetaryScore = Score(c.Monetary, monetaryThresholds);

            return new CustomerRfmSegment
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                RecencyDays = c.RecencyDays,
                Frequency = c.Frequency,
                Monetary = c.Monetary,
                RecencyScore = recencyScore,
                FrequencyScore = frequencyScore,
                MonetaryScore = monetaryScore,
                SegmentName = ClassifySegment(recencyScore, frequencyScore, monetaryScore)
            };
        }).ToList();
    }

    private static (double Q1, double Q2, double Q3) Quartiles(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        return (Percentile(sorted, 0.25), Percentile(sorted, 0.50), Percentile(sorted, 0.75));
    }

    private static double Percentile(List<double> sorted, double percentile)
    {
        if (sorted.Count == 1)
        {
            return sorted[0];
        }

        var position = percentile * (sorted.Count - 1);
        var lowerIndex = (int)Math.Floor(position);
        var upperIndex = (int)Math.Ceiling(position);
        if (lowerIndex == upperIndex)
        {
            return sorted[lowerIndex];
        }

        var fraction = position - lowerIndex;
        return sorted[lowerIndex] + (sorted[upperIndex] - sorted[lowerIndex]) * fraction;
    }

    /// <summary>Higher raw value → higher score (used for Frequency and Monetary).</summary>
    private static int Score(double value, (double Q1, double Q2, double Q3) thresholds) => value switch
    {
        _ when value <= thresholds.Q1 => 1,
        _ when value <= thresholds.Q2 => 2,
        _ when value <= thresholds.Q3 => 3,
        _ => 4
    };

    /// <summary>Lower raw value → higher score (used for Recency — fewer days since last activity is better).</summary>
    private static int ScoreInverted(double value, (double Q1, double Q2, double Q3) thresholds) => value switch
    {
        _ when value <= thresholds.Q1 => 4,
        _ when value <= thresholds.Q2 => 3,
        _ when value <= thresholds.Q3 => 2,
        _ => 1
    };

    private static string ClassifySegment(int recency, int frequency, int monetary)
    {
        var total = recency + frequency + monetary;

        return (recency, frequency, monetary) switch
        {
            _ when recency >= 4 && frequency >= 3 && monetary >= 3 => "Champions",
            _ when recency >= 3 && frequency <= 2 && monetary <= 2 => "New Customers",
            _ when recency <= 2 && frequency >= 3 && monetary >= 3 => "At Risk",
            _ when recency == 1 && frequency <= 2 && monetary <= 2 => "Lost",
            _ when total >= 9 => "Loyal Customers",
            _ => "Regular Customers"
        };
    }
}
