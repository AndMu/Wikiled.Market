using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Integration.Tests.Sentiment
{
    [TestFixture]
    public class SentimentAnalysisTests
    {
        private SentimentAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public async Task SimpleTest()
        {
            var result = await instance.MeasureSentiment("Sell and short it");
            Assert.AreEqual(-1, result);
        }

        private SentimentAnalysis CreateSentimentAnalysis()
        {
            return new SentimentAnalysis(new StreamApiClient(new HttpClient(), new Uri("http://sentiment.wikiled.com/api/sentiment/")));
        }
    }
}