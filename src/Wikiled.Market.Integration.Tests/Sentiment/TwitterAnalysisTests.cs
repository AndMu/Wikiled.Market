using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Integration.Tests.Sentiment
{
    [TestFixture]
    public class TwitterAnalysisTests
    {
        private TwitterAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public async Task SimpleTest()
        {
            var result = await instance.GetSentiment("$AMD");
            Assert.GreaterOrEqual(result.Total, 0);
        }

        private TwitterAnalysis CreateSentimentAnalysis()
        {
            return new TwitterAnalysis(new StreamApiClient(new HttpClient(),
                new Uri("http://192.168.0.200:7020/api/twitter/"),
                new Logger<StreamApiClient>(new NullLoggerFactory())));
        }
    }
}