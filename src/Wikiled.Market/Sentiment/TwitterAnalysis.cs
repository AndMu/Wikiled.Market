using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Wikiled.Common.Net.Client;
using Wikiled.Twitter.Monitor.Api.Response;

namespace Wikiled.Market.Sentiment
{
    public class TwitterAnalysis : ITwitterAnalysis
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IStreamApiClient client;

        public TwitterAnalysis(IStreamApiClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<TrackingResults> GetSentiment(string keyword)
        {
            log.Debug("GetSentiment: {0}", keyword);
            var result = await client.GetRequest<TrackingResults>($"sentiment/{keyword}", CancellationToken.None).LastOrDefaultAsync();
            log.Debug("Result {0}", result);
            return result;
        }
    }
}
