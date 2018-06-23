using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Net.Client;

namespace Wikiled.Market.Sentiment
{
    public class TwitterAnalysisFactory : ITwitterAnalysisFactory
    {
        private readonly ILogger<StreamApiClient> logger;

        public TwitterAnalysisFactory(ILogger<StreamApiClient> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ITwitterAnalysis Create()
        {
            return new TwitterAnalysis(
                new StreamApiClient(
                    new HttpClient(), 
                    new Uri("http://192.168.0.200:7020/api/twitter/"),
                logger));
        }
    }
}
