using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Text.Analysis.Twitter;

namespace Wikiled.Market.Console.Logic
{
    public static class TrackingResultExtension
    {
        public static string GetEmoji(this TrackingResult result)
        {
            if (result.Average < 0)
            {
                return Emoji.CHART_WITH_DOWNWARDS_TREND.Unicode;
            }

            return result.Average > 0 ? Emoji.CHART_WITH_UPWARDS_TREND.Unicode : string.Empty;
        }
    }
}
