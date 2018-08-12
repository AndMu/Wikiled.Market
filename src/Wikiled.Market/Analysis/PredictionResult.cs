using System;
using Accord.Statistics.Analysis;

namespace Wikiled.Market.Analysis
{
    public class PredictionResult
    {
        public PredictionResult(MarketDirection[] predictions, GeneralConfusionMatrix performance)
        {
            Predictions = predictions ?? throw new ArgumentNullException(nameof(predictions));
            Performance = performance ?? throw new ArgumentNullException(nameof(performance));
        }

        public GeneralConfusionMatrix Performance { get; }

        public MarketDirection[] Predictions { get; }
    }
}
