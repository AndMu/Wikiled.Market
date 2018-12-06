using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Twitter.Monitor.Api.Service;

namespace Wikiled.Market.Console.Logic.Charts
{
    public class TwitterChartGeneration : ISentimentChartGeneration
    {
        private readonly ILogger<TwitterChartGeneration> log;

        private readonly ITwitterAnalysis twitterAnalysis;

        private readonly IDayChartGenerator generator;

        public TwitterChartGeneration(ILogger<TwitterChartGeneration> log,
                                      ITwitterAnalysis twitterAnalysis,
                                      Func<string, IDayChartGenerator> generatorFactory)
        {
            if (generatorFactory == null)
            {
                throw new ArgumentNullException(nameof(generatorFactory));
            }

            this.log = log ?? throw new ArgumentNullException(nameof(log));
            generator = generatorFactory("Twitter");
            this.twitterAnalysis = twitterAnalysis ?? throw new ArgumentNullException(nameof(twitterAnalysis));
        }

        public async Task AddStock(string stock)
        {
            if (stock == null)
            {
                throw new ArgumentNullException(nameof(stock));
            }

            log.LogDebug("AddStock {0}", stock);
            var data = await twitterAnalysis.GetTrackingHistory($"${stock}", 24 * 5, CancellationToken.None).ConfigureAwait(false);
            lock (generator)
            {
                generator.AddSeriesByDay(stock, data);
            }
        }

        public Task<byte[]> Generate()
        {
            log.LogDebug("Generate");
            return generator.GenerateGraph();
        }
    }
}
