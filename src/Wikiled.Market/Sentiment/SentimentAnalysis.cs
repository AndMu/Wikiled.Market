using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Wikiled.Common.Net.Client;
using Wikiled.Sentiment.Api.Request;
using Wikiled.Sentiment.Text.Data.Review;
using Wikiled.Sentiment.Text.Sentiment;
using Wikiled.Text.Analysis.Structure;

namespace Wikiled.Market.Sentiment
{
    public class SentimentAnalysis : ISentimentAnalysis
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IStreamApiClient client;

        public SentimentAnalysis(IStreamApiClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<double?> MeasureSentiment(string text)
        {
            log.Debug("MeasureSentiment");
            WorkRequest request = new WorkRequest();
            request.CleanText = true;
            request.Documents = new[] { new SingleProcessingData(text) };
            request.Domain = "TwitterMarket";
            var result = await client.PostRequest<WorkRequest, Document>("parsestream", request, CancellationToken.None).LastOrDefaultAsync();
            log.Debug("MeasureSentiment Calculated: {0}", result.Stars);
            return result.Stars.HasValue ? RatingCalculator.ConvertToRaw(result.Stars.Value) : result.Stars;
        }
    }
}
