using Wikiled.Common.Arguments;

namespace Wikiled.Market.Analysis
{
    public class PredictionResult
    {
        public PredictionResult(MarketDirection[] predictions)
        {
            Guard.NotNull(() => predictions, predictions);
            Predictions = predictions;
        }

        public MarketDirection[] Predictions { get; }
    }
}
