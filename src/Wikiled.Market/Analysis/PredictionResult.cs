using Accord.Statistics.Analysis;
using Wikiled.Common.Arguments;

namespace Wikiled.Market.Analysis
{
    public class PredictionResult
    {
        public PredictionResult(MarketDirection[] predictions, GeneralConfusionMatrix performance)
        {
            Guard.NotNull(() => predictions, predictions);
            Guard.NotNull(() => performance, performance);
            Predictions = predictions;
            Performance = performance;
        }

        public GeneralConfusionMatrix Performance { get; }

        public MarketDirection[] Predictions { get; }
    }
}
