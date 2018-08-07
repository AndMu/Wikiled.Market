using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;
using Wikiled.Market.Analysis;

namespace Wikiled.Market.Sentiment
{
    public class TwitterAnalysisFactory : ITwitterAnalysisFactory
    {
        private readonly ILoggerFactory logger;

        private readonly SentimentConfig config;

        public TwitterAnalysisFactory(ILoggerFactory logger, SentimentConfig config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ITwitterAnalysis Create()
        {
            return new TwitterAnalysis(
                new ApiClientFactory(new HttpClient(), new Uri($"http://{config.Service}/api/twitter/")).GetClient(), 
                logger.CreateLogger<TwitterAnalysis>());
        }
    }
}
