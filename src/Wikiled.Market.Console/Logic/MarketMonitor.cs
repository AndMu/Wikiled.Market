using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Wikiled.Market.Analysis;
using Wikiled.Market.Console.Commands;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Api.Service;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Communication;

namespace Wikiled.Market.Console.Logic
{
    public class MarketMonitor : IMarketMonitor
    {
        private readonly ILogger<TwitterBotCommand> log;

        private readonly ISentimentTracking twitterAnalysis;

        private readonly IPublisher publisher;

        private readonly Func<IAnalysisManager> instance;

        public MarketMonitor(ILogger<TwitterBotCommand> log, IIndex<string, ISentimentTracking> twitterAnalysis, Func<IAnalysisManager> instance, IPublisher publisher)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.twitterAnalysis = twitterAnalysis?["Twitter"] ?? throw new ArgumentNullException(nameof(twitterAnalysis));
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.publisher = publisher;
        }

        public async Task ProcessMarket(string[] stockItems)
        {
            log.LogDebug("Processing market");
            var sentimentTask = await twitterAnalysis.GetTrackingResults(new SentimentRequest(stockItems.Select(item => $"${item}").ToArray()) {Hours = new[] {24}}, CancellationToken.None)
                                                     .ConfigureAwait(false);
            foreach (string stock in stockItems)
            {
                log.LogInformation("Processing {0}", stock);

                PredictionResult result = await instance().Start(stock).ConfigureAwait(false);
                double sellAccuracy = result.Performance.PerClassMatrices[0].Accuracy;
                double buyAccuracy = result.Performance.PerClassMatrices[1].Accuracy;
                string header = $"${stock} trading signals ({sellAccuracy * 100:F0}%/{buyAccuracy * 100:F0}%)";
                var text = new StringBuilder();

                if (sentimentTask.TryGetValue($"${stock}", out var sentiment))
                {
                    var sentimentValue = sentiment.First();
                    text.AppendFormat(
                        "Average sentiment: {2}{0:F2}({1})\r\n",
                        sentimentValue.Average,
                        sentimentValue.TotalMessages,
                        sentimentValue.GetEmoji());
                }
                else
                {
                    log.LogWarning("Not found sentiment for {0}", stock);
                }

                for (int i = 0; i < result.Predictions.Length || i < 2; i++)
                {
                    MarketDirection prediction = result.Predictions[result.Predictions.Length - i - 1];
                    log.LogInformation("{2}, Predicted T-{0}: {1}\r\n", i, prediction, stock);
                    string icon = prediction == MarketDirection.Buy ? Emoji.CHART_WITH_UPWARDS_TREND.Unicode : Emoji.CHART_WITH_DOWNWARDS_TREND.Unicode;
                    text.AppendFormat("T-{0}: {2}{1}\r\n", i, prediction, icon);
                }

                var message = new MultiItemMessage(header, new[] {text.ToString()});
                publisher.PublishMessage(message);
            }
        }
    }
}
