using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Twitter.Monitor.Api.Service;

namespace Wikiled.Market.Console.Logic.Charts
{
    public class SentimentChartGeneration : ISentimentChartGeneration
    {
        private readonly ILogger<SentimentChartGeneration> log;

        private readonly ITwitterAnalysis twitterAnalysis;

        private readonly Func<string, IDayChartGenerator> generatorFactory;

        public SentimentChartGeneration(ILogger<SentimentChartGeneration> log, ITwitterAnalysis twitterAnalysis, Func<string, IDayChartGenerator> generatorFactory)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.generatorFactory = generatorFactory ?? throw new ArgumentNullException(nameof(generatorFactory));
            this.twitterAnalysis = twitterAnalysis ?? throw new ArgumentNullException(nameof(twitterAnalysis));
        }

        public async Task<byte[]> Generate()
        {
            var data = await twitterAnalysis.GetTrackingHistory("$AMD", 24 * 5, CancellationToken.None).ConfigureAwait(false);
            var generator = generatorFactory("Twitter");
            generator.AddSeriesByDay("AMD", data);
            return await generator.GenerateGraph().ConfigureAwait(false);
        }
    }
}
