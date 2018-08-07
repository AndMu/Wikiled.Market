using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;
using Wikiled.Twitter.Monitor.Api.Response;

namespace Wikiled.Market.Sentiment
{
    public class TwitterAnalysis : ITwitterAnalysis
    {
        private readonly ILogger<TwitterAnalysis> logger;

        private readonly IApiClient client;

        public TwitterAnalysis(IApiClient client, ILogger<TwitterAnalysis> logger)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TrackingResults> GetSentiment(string keyword)
        {
            try
            {
                logger.LogDebug("GetSentiment: {0}", keyword);
                var result = await client.GetRequest<RawResponse<TrackingResults>>($"sentiment/{keyword}", CancellationToken.None).ConfigureAwait(false);
                logger.LogDebug("Result {0}", result);
                if (!result.IsSuccess)
                {
                    logger.LogError("Request failed with: {0}", result.HttpResponseMessage);
                    return null;
                }

                return result.Result.Value;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve sentiment");
            }

            return null;
        }
    }
}
