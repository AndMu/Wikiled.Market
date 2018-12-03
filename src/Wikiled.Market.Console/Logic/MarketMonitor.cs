using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Market.Analysis;
using Wikiled.Market.Console.Commands;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Communication;
using Wikiled.Twitter.Monitor.Api.Service;

namespace Wikiled.Market.Console.Logic
{
    public class MarketMonitor : IMarketMonitor
    {
        private readonly ILogger<TwitterBotCommand> log;

        private readonly ITwitterAnalysis twitterAnalysis;

        private readonly IPublisher publisher;

        private readonly Func<IAnalysisManager> instance;

        public MarketMonitor(ILogger<TwitterBotCommand> log, ITwitterAnalysis twitterAnalysis, Func<IAnalysisManager> instance, IPublisher publisher)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.twitterAnalysis = twitterAnalysis ?? throw new ArgumentNullException(nameof(twitterAnalysis));
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.publisher = publisher;
        }

        public async Task ProcessMarket(string[] stockItems)
        {
            log.LogDebug("Processing market");
            foreach (string stock in stockItems)
            {
                log.LogInformation("Processing {0}", stock);
                Task<TrackingResults> sentimentTask = twitterAnalysis.GetTrackingResults($"${stock}", CancellationToken.None);
                PredictionResult result = await instance().Start(stock).ConfigureAwait(false);
                double sellAccuracy = result.Performance.PerClassMatrices[0].Accuracy;
                double buyAccuracy = result.Performance.PerClassMatrices[1].Accuracy;
                string header = $"${stock} trading signals ({sellAccuracy * 100:F0}%/{buyAccuracy * 100:F0}%)";
                StringBuilder text = new StringBuilder();
                TrackingResults sentiment = await sentimentTask.ConfigureAwait(false);
                if (sentiment != null)
                {
                    if (sentiment.Sentiment.TryGetValue("24H", out TrackingResult sentimentValue))
                    {
                        text.AppendFormat("Average sentiment: {2}{0:F2}({1})\r\n",
                                          sentimentValue.Average,
                                          sentimentValue.TotalMessages,
                                          sentimentValue.GetEmoji());
                    }
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

                MultiItemMessage message = new MultiItemMessage(header, new[] { text.ToString() });
                publisher.PublishMessage(message);
            }
        }
    }
}
