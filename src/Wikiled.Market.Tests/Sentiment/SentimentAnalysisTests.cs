using System;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Tests.Sentiment
{
    [TestFixture]
    public class SentimentAnalysisTests
    {
        private Mock<IStreamApiClient> mockStreamApiClient;

        private SentimentAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            mockStreamApiClient = new Mock<IStreamApiClient>();
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new SentimentAnalysis(null));
        }

        private SentimentAnalysis CreateSentimentAnalysis()
        {
            return new SentimentAnalysis(mockStreamApiClient.Object);
        }
    }
}