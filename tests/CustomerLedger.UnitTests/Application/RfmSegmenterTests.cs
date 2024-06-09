using CustomerLedger.Application.DTOs;
using CustomerLedger.Application.Services;
using Xunit;

namespace CustomerLedger.UnitTests.Application;

public class RfmSegmenterTests
{
    [Fact]
    public void Segment_EmptyInput_ReturnsEmpty()
    {
        var result = RfmSegmenter.Segment(Array.Empty<CustomerRfmInput>());

        Assert.Empty(result);
    }

    [Fact]
    public void Segment_MostRecentFrequentAndHighestSpender_ScoresBestOnAllThreeDimensions()
    {
        var customers = new List<CustomerRfmInput>
        {
            new() { CustomerId = 1, CustomerCode = "C1", CustomerName = "Champion", RecencyDays = 1, Frequency = 20, Monetary = 100000 },
            new() { CustomerId = 2, CustomerCode = "C2", CustomerName = "Average", RecencyDays = 30, Frequency = 5, Monetary = 20000 },
            new() { CustomerId = 3, CustomerCode = "C3", CustomerName = "Lapsed", RecencyDays = 200, Frequency = 1, Monetary = 500 },
            new() { CustomerId = 4, CustomerCode = "C4", CustomerName = "Middling", RecencyDays = 60, Frequency = 8, Monetary = 30000 }
        };

        var result = RfmSegmenter.Segment(customers);

        var champion = result.Single(r => r.CustomerId == 1);
        var lapsed = result.Single(r => r.CustomerId == 3);

        Assert.Equal(4, champion.RecencyScore);
        Assert.Equal(4, champion.FrequencyScore);
        Assert.Equal(4, champion.MonetaryScore);
        Assert.Equal("Champions", champion.SegmentName);

        Assert.Equal(1, lapsed.RecencyScore);
        Assert.Equal(1, lapsed.FrequencyScore);
        Assert.Equal(1, lapsed.MonetaryScore);
        Assert.Equal("Lost", lapsed.SegmentName);
    }

    [Fact]
    public void Segment_EveryCustomerGetsExactlyOneSegment()
    {
        var customers = Enumerable.Range(1, 12).Select(i => new CustomerRfmInput
        {
            CustomerId = i,
            CustomerCode = $"C{i}",
            CustomerName = $"Customer {i}",
            RecencyDays = i * 10,
            Frequency = i,
            Monetary = i * 1000
        }).ToList();

        var result = RfmSegmenter.Segment(customers);

        Assert.Equal(12, result.Count);
        Assert.All(result, r => Assert.False(string.IsNullOrEmpty(r.SegmentName)));
    }
}
