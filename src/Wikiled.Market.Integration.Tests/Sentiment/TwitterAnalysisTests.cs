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
        private ITwitterAnalysis instance;

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

        private ITwitterAnalysis CreateSentimentAnalysis()
        {
            return new TwitterAnalysisFactory(new Logger<StreamApiClient>(new NullLoggerFactory())).Create();
        }
    }
}