namespace CustomerLedger.Application.Services;

/// <summary>
/// A minimal, dependency-free binary logistic regression classifier trained by batch
/// gradient descent — the same core algorithm behind any "ML.NET LogisticRegression" or
/// "scikit-learn LogisticRegression" call, implemented directly so this project's one ML
/// use case doesn't need a multi-hundred-megabyte ML framework dependency. Given a fixed
/// feature vector, it learns a weight per feature (plus a bias term) that best separates
/// the two classes in the training data, then scores new customers with the same weights.
///
/// This is real supervised learning (weights are *learned* from data, not hand-tuned), but
/// it is deliberately simple: see docs/releases/v7.0.0-Capital.md for the honest limitations
/// (small/synthetic training set, a heuristic label rather than real historical default
/// outcomes) and what a production system would do differently.
/// </summary>
public class LogisticRegressionModel
{
    private double[] _weights = Array.Empty<double>();
    private double _bias;

    public bool IsTrained { get; private set; }

    /// <summary>
    /// Trains via batch gradient descent on the binary cross-entropy loss. Each row of
    /// <paramref name="features"/> is one training example; <paramref name="labels"/> is
    /// 0.0 or 1.0 for that example's class.
    /// </summary>
    public void Train(IReadOnlyList<double[]> features, IReadOnlyList<double> labels, int iterations = 1000, double learningRate = 0.1)
    {
        if (features.Count == 0 || features.Count != labels.Count)
        {
            throw new ArgumentException("Features and labels must be non-empty and of equal length.");
        }

        var featureCount = features[0].Length;
        var normalized = Normalize(features, out var means, out var stdDevs);

        _weights = new double[featureCount];
        _bias = 0;
        _means = means;
        _stdDevs = stdDevs;

        var sampleCount = normalized.Count;

        for (var iteration = 0; iteration < iterations; iteration++)
        {
            var weightGradients = new double[featureCount];
            var biasGradient = 0.0;

            for (var i = 0; i < sampleCount; i++)
            {
                var predicted = Sigmoid(Dot(normalized[i], _weights) + _bias);
                var error = predicted - labels[i];

                for (var f = 0; f < featureCount; f++)
                {
                    weightGradients[f] += error * normalized[i][f];
                }
                biasGradient += error;
            }

            for (var f = 0; f < featureCount; f++)
            {
                _weights[f] -= learningRate * weightGradients[f] / sampleCount;
            }
            _bias -= learningRate * biasGradient / sampleCount;
        }

        IsTrained = true;
    }

    /// <summary>Returns the predicted probability (0.0-1.0) of the positive class for one feature vector.</summary>
    public double PredictProbability(double[] featureVector)
    {
        if (!IsTrained)
        {
            throw new InvalidOperationException("Call Train() before PredictProbability().");
        }

        var normalized = new double[featureVector.Length];
        for (var f = 0; f < featureVector.Length; f++)
        {
            normalized[f] = _stdDevs[f] == 0 ? 0 : (featureVector[f] - _means[f]) / _stdDevs[f];
        }

        return Sigmoid(Dot(normalized, _weights) + _bias);
    }

    private double[] _means = Array.Empty<double>();
    private double[] _stdDevs = Array.Empty<double>();

    /// <summary>
    /// Z-score normalization (mean 0, std-dev 1 per feature) — without it, a feature on a
    /// scale of thousands (e.g. invoice amount) would dominate gradient descent over a
    /// feature on a scale of 0-1 (e.g. credit utilization ratio), regardless of which one
    /// actually predicts risk better.
    /// </summary>
    private static List<double[]> Normalize(IReadOnlyList<double[]> features, out double[] means, out double[] stdDevs)
    {
        var featureCount = features[0].Length;
        means = new double[featureCount];
        stdDevs = new double[featureCount];

        for (var f = 0; f < featureCount; f++)
        {
            var mean = features.Average(row => row[f]);
            var variance = features.Average(row => Math.Pow(row[f] - mean, 2));
            means[f] = mean;
            stdDevs[f] = Math.Sqrt(variance);
        }

        var normalized = new List<double[]>(features.Count);
        foreach (var row in features)
        {
            var normalizedRow = new double[featureCount];
            for (var f = 0; f < featureCount; f++)
            {
                normalizedRow[f] = stdDevs[f] == 0 ? 0 : (row[f] - means[f]) / stdDevs[f];
            }
            normalized.Add(normalizedRow);
        }

        return normalized;
    }

    private static double Sigmoid(double z) => 1.0 / (1.0 + Math.Exp(-z));

    private static double Dot(double[] a, double[] b)
    {
        var sum = 0.0;
        for (var i = 0; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }
        return sum;
    }
}
