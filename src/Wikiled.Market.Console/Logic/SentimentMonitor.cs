using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Microsoft.Extensions.Logging;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Api.Service;
using Wikiled.Sentiment.Tracking.Logic;
using Wikiled.Twitter.Communication;

namespace Wikiled.Market.Console.Logic
{
    public class SentimentMonitor : ISentimentMonitor
    {
        private readonly ILogger<SentimentMonitor> log;

        private readonly ISentimentTracking twitterAnalysis;

        private readonly ISentimentTracking alpha;

        private readonly IPublisher publisher;

        public SentimentMonitor(ILogger<SentimentMonitor> log, IIndex<string, ISentimentTracking> factory, IPublisher publisher)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            this.log = log ?? throw new ArgumentNullException(nameof(log));
            log.LogDebug("SentimentMonitor");
            twitterAnalysis = factory["Twitter"] ?? throw new ArgumentNullException(nameof(twitterAnalysis));
            alpha = factory["Seeking"] ?? throw new ArgumentNullException(nameof(alpha));
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task ProcessSentimentAll(string[] stockItems)
        {
            PublishSentiment(await Get(twitterAnalysis, stockItems.Select(item => $"${item}").ToArray(), 6).ConfigureAwait(false), "Twitter 6H");
            await Task.Delay(TimeSpan.FromMinutes(15)).ConfigureAwait(false);
            PublishSentiment(await Get(alpha, stockItems, 48, "Article").ConfigureAwait(false), "SeekingAlpha Editors");
            await Task.Delay(TimeSpan.FromMinutes(15)).ConfigureAwait(false);
            PublishSentiment(await Get(alpha, stockItems, 24, "Comment").ConfigureAwait(false), "SeekingAlpha Comments");
        }

        private Task<IDictionary<string, TrackingResult[]>> Get(ISentimentTracking tracker, string[] keywords, int hours, string type = null)
        {
            try
            {
                return tracker.GetTrackingResults(new SentimentRequest(keywords) {Hours = new[] {hours}, Type = type}, CancellationToken.None);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error");
            }

            return null;
        }

        private void PublishSentiment(IDictionary<string, TrackingResult[]> retrieve, string type)
        {
            log.LogInformation("Retrieving sentiment {0}...", type);
            if (retrieve == null ||
                retrieve.Count == 0)
            {
                log.LogWarning("Nothing to process - {0}", type);
                return;
            }

            List<string> messages = new List<string>();
            foreach (var stock in retrieve)
            {
                if (stock.Value != null)
                {
                    foreach (var record in stock.Value)
                    {
                        if (record.TotalMessages > 0)
                        {
                            messages.Add($"{record.GetEmoji()} {stock.Key}: {record.Average:F2}({record.TotalMessages})");
                        }
                    }
                }
                else
                {
                    log.LogWarning("Not found sentiment for {0}", stock);
                }
            }

            var message = new MultiItemMessage($"Average sentiment ({type}):", messages.ToArray());
            publisher.PublishMessage(message);
        }
    }
}
