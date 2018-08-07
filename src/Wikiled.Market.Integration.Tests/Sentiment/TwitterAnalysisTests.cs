using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Wikiled.Market.Analysis;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Integration.Tests.Sentiment
{
    [TestFixture]
    public class TwitterAnalysisTests
    {
        private ITwitterAnalysis instance;

        private SentimentConfig config;

        [SetUp]
        public void SetUp()
        {
            config = new SentimentConfig();
            config.Service = "192.168.0.70:7020";
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public async Task SimpleTest()
        {
            var result = await instance.GetSentiment("$AMD").ConfigureAwait(false);
            Assert.GreaterOrEqual(result.Total, 0);
        }

        private ITwitterAnalysis CreateSentimentAnalysis()
        {
            return new TwitterAnalysisFactory(new NullLoggerFactory(), config).Create();
        }
    }
}