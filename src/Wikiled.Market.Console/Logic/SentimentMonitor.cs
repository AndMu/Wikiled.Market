using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Wikiled.SeekingAlpha.Api.Request;
using Wikiled.SeekingAlpha.Api.Service;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Twitter.Communication;
using Wikiled.Twitter.Monitor.Api.Service;

namespace Wikiled.Market.Console.Logic
{
    public class SentimentMonitor : ISentimentMonitor
    {
        private readonly ILogger<SentimentMonitor> log;

        private readonly ITwitterAnalysis twitterAnalysis;

        private readonly IAlphaAnalysis alpha;

        private readonly IPublisher publisher;

        private readonly PolicyBuilder<TrackingResults> policy = Policy.HandleResult<TrackingResults>(r => r == null);

        public SentimentMonitor(ILogger<SentimentMonitor> log, ITwitterAnalysis twitterAnalysis, IAlphaAnalysis alpha, IPublisher publisher)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.twitterAnalysis = twitterAnalysis ?? throw new ArgumentNullException(nameof(twitterAnalysis));
            this.alpha = alpha ?? throw new ArgumentNullException(nameof(alpha));
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task ProcessSentimentAll(string[] stockItems)
        {
            Task twitterTask = 
                ProcessSentiment(stockItems, "Twitter", 6, stock => twitterAnalysis.GetTrackingResults($"${stock}", CancellationToken.None));

            Task seekingAlpha =
                ProcessSentiment(stockItems, "SeekingAlpha Editors", 48, stock => alpha.GetTrackingResults(new SentimentRequest(stock, SentimentType.Article) {Steps = new []{48}}, CancellationToken.None));

            Task seekingAlphaComments = 
                ProcessSentiment(stockItems, "SeekingAlpha Comments", 24, stock => alpha.GetTrackingResults(new SentimentRequest(stock, SentimentType.Comment), CancellationToken.None));

            await Task.WhenAll(twitterTask, seekingAlphaComments, seekingAlpha).ConfigureAwait(false);
        }

        private async Task ProcessSentiment(string[] stockItems, string type, int hours, Func<string, Task<TrackingResults>> retrieve)
        {
            log.LogInformation("Retrieving sentiment {0}...", type);
            List<string> messages = new List<string>();
            foreach (string stock in stockItems)
            {
                TrackingResults sentiment = await policy.RetryAsync(3)
                    .ExecuteAsync(() => retrieve(stock))
                    .ConfigureAwait(false);
                if (sentiment != null)
                {
                    if (sentiment.Sentiment.ContainsKey($"{hours}H"))
                    {
                        TrackingResult value = sentiment.Sentiment[$"{hours}H"];
                        if (value.TotalMessages > 0)
                        {
                            messages.Add($"${stock}: {value.GetEmoji()}{value.Average:F2}({value.TotalMessages})");
                        }
                    }
                }
                else
                {
                    log.LogWarning("Not found sentiment for {0}", stock);
                }
            }

            var message = new MultiItemMessage($"Average sentiment (from {type}) ({hours}H):", messages.ToArray());
            publisher.PublishMessage(message);
        }
    }
}
