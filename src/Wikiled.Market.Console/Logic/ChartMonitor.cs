using Autofac.Features.Indexed;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Market.Console.Logic.Charts;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Api.Service;
using Wikiled.Twitter.Communication;

namespace Wikiled.Market.Console.Logic
{
    public class ChartMonitor : IChartMonitor
    {
        private readonly ILogger<ChartMonitor> log;

        private readonly ISentimentTracking twitterAnalysis;

        private readonly ISentimentTracking alpha;

        private readonly IPublisher publisher;

        private readonly Func<string, IDayChartGenerator> chartFactory;

        public ChartMonitor(
            ILogger<ChartMonitor> log,
            IIndex<string, ISentimentTracking> factory,
            IPublisher publisher,
            Func<string, IDayChartGenerator> chartFactory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            this.log = log;
            twitterAnalysis = factory["Twitter"] ?? throw new ArgumentNullException(nameof(twitterAnalysis));
            alpha = factory["Seeking"] ?? throw new ArgumentNullException(nameof(alpha));
            this.publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            this.chartFactory = chartFactory;
        }

        public async Task ProcessMarket(string[] stockItems)
        {
            log.LogDebug("Processing market");
            await CreateChart("Twitter Average Sentiment", twitterAnalysis, stockItems.Select(item => $"${item}").ToArray()).ConfigureAwait(false);
            await CreateChart("SeekingAlpha Articles Average Sentiment", alpha, stockItems, Constant.Article).ConfigureAwait(false);
            await CreateChart("SeekingAlpha Comments Average Sentiment", alpha, stockItems, Constant.Comment).ConfigureAwait(false);
        }

        private async Task CreateChart(string name, ISentimentTracking tracking, string[] stockItems, string type = null)
        {
            int days = 5;
            var data = await tracking
                             .GetTrackingHistory(new SentimentRequest(stockItems) { Hours = new[] { HoursExtension.GetLastDaysHours(days) }, Type = type }, CancellationToken.None)
                             .ConfigureAwait(false);
            var selected = data.Where(item => item.Value.Length > 0).ToArray();

            foreach (var batch in selected.Batch(5))
            {

                var currentBlock = batch.ToArray();
                IDayChartGenerator chart = chartFactory(name);
                foreach (var pair in currentBlock)
                {
                    chart.AddSeriesByDay(pair.Key, pair.Value, days);
                }

                byte[] image = await chart.GenerateGraph().ConfigureAwait(false);
                if (image == null)
                {
                    log.LogWarning("No image to post");
                    return;
                }

                MediaMessage message = new MediaMessage(name, image);
                publisher.PublishMessage(message);
            }
        }
    }
}
