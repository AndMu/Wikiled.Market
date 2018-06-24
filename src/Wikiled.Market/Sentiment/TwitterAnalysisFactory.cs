using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;

namespace Wikiled.Market.Sentiment
{
    public class TwitterAnalysisFactory : ITwitterAnalysisFactory
    {
        private readonly ILoggerFactory logger;

        public TwitterAnalysisFactory(ILoggerFactory logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ITwitterAnalysis Create()
        {
            return new TwitterAnalysis(
                new ApiClientFactory(new HttpClient(), new Uri("http://192.168.0.200:7020/api/twitter/")).GetClient(), 
                logger.CreateLogger<TwitterAnalysis>());
        }
    }
}
