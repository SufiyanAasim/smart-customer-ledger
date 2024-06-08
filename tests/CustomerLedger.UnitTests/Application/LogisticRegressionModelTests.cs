using CustomerLedger.Application.Services;
using Xunit;

namespace CustomerLedger.UnitTests.Application;

public class LogisticRegressionModelTests
{
    [Fact]
    public void PredictProbability_BeforeTrain_Throws()
    {
        var model = new LogisticRegressionModel();

        Assert.Throws<InvalidOperationException>(() => model.PredictProbability(new double[] { 1.0 }));
    }

    [Fact]
    public void Train_ThrowsOnMismatchedFeatureAndLabelCounts()
    {
        var model = new LogisticRegressionModel();
        var features = new List<double[]> { new[] { 1.0 }, new[] { 2.0 } };
        var labels = new List<double> { 0.0 };

        Assert.Throws<ArgumentException>(() => model.Train(features, labels));
    }

    [Fact]
    public void Train_OnLinearlySeparableData_LearnsToDiscriminate()
    {
        // Single feature: values below 5 are class 0, values at/above 5 are class 1 —
        // a textbook linearly separable dataset that logistic regression should learn
        // essentially perfectly.
        var features = new List<double[]>
        {
            new[] { 0.0 }, new[] { 1.0 }, new[] { 2.0 }, new[] { 3.0 }, new[] { 4.0 },
            new[] { 5.0 }, new[] { 6.0 }, new[] { 7.0 }, new[] { 8.0 }, new[] { 9.0 }
        };
        var labels = new List<double> { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 };

        var model = new LogisticRegressionModel();
        model.Train(features, labels, iterations: 2000, learningRate: 0.5);

        var lowProbability = model.PredictProbability(new[] { 0.0 });
        var highProbability = model.PredictProbability(new[] { 9.0 });

        Assert.True(lowProbability < 0.5, $"Expected a low-class example to score below 0.5, got {lowProbability}");
        Assert.True(highProbability > 0.5, $"Expected a high-class example to score above 0.5, got {highProbability}");
        Assert.True(highProbability > lowProbability);
    }

    [Fact]
    public void Train_MultiFeature_ProducesMonotonicRiskOrdering()
    {
        // Two features; class correlates positively with both. A customer with higher
        // values on both features should score at least as risky as one with lower values.
        var features = new List<double[]>
        {
            new[] { 0.1, 10.0 }, new[] { 0.2, 20.0 }, new[] { 0.15, 15.0 },
            new[] { 0.8, 80.0 }, new[] { 0.9, 90.0 }, new[] { 0.85, 85.0 }
        };
        var labels = new List<double> { 0, 0, 0, 1, 1, 1 };

        var model = new LogisticRegressionModel();
        model.Train(features, labels, iterations: 2000, learningRate: 0.5);

        var lowRisk = model.PredictProbability(new[] { 0.15, 15.0 });
        var highRisk = model.PredictProbability(new[] { 0.85, 85.0 });

        Assert.True(highRisk > lowRisk);
    }
}
