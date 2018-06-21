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
        private TwitterAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public async Task SimpleTest()
        {
            var result = await instance.GetSentiment("Sell and short it");
            Assert.AreEqual(-1, result);
        }

        private TwitterAnalysis CreateSentimentAnalysis()
        {
            return new TwitterAnalysis(new StreamApiClient(new HttpClient(), new Uri("http://sentiment.wikiled.com/api/sentiment/")));
        }
    }
}