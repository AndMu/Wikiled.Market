using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Microsoft.Extensions.Logging;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Api.Service;

namespace Wikiled.Market.Console.Logic.Charts
{
    public class TwitterChartGeneration : ISentimentChartGeneration
    {
        private readonly ILogger<TwitterChartGeneration> log;

        private readonly ISentimentTracking twitterAnalysis;

        private readonly IDayChartGenerator generator;

        public TwitterChartGeneration(ILogger<TwitterChartGeneration> log, IIndex<string, ISentimentTracking> twitterAnalysis, Func<string, IDayChartGenerator> generatorFactory)
        {
            if (generatorFactory == null)
            {
                throw new ArgumentNullException(nameof(generatorFactory));
            }

            this.log = log ?? throw new ArgumentNullException(nameof(log));
            generator = generatorFactory("Twitter");
            this.twitterAnalysis = twitterAnalysis?["Twitter"] ?? throw new ArgumentNullException(nameof(twitterAnalysis));
        }

        public async Task AddStocks(string[] stocks)
        {
            if (stocks == null)
            {
                throw new ArgumentNullException(nameof(stocks));
            }

            if (stocks.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(stocks));
            }

            log.LogDebug("AddStock {0}", stocks.Length);
            var items = stocks.Select(item => $"${item}").ToArray();
            var data = await twitterAnalysis
                .GetTrackingHistory(new SentimentRequest(items) { Hour = 24 * 5 }, CancellationToken.None)
                .ConfigureAwait(false);
            lock (generator)
            {
                foreach (var ratingRecords in data)
                {
                    generator.AddSeriesByDay(ratingRecords.Key, ratingRecords.Value);
                }
            }
        }

        public Task<byte[]> Generate()
        {
            log.LogDebug("Generate");
            return generator.GenerateGraph();
        }
    }
}
