using System;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Tests.Sentiment
{
    [TestFixture]
    public class TwitterAnalysisTests
    {
        private Mock<IStreamApiClient> mockStreamApiClient;

        private TwitterAnalysis instance;

        [SetUp]
        public void SetUp()
        {
            mockStreamApiClient = new Mock<IStreamApiClient>();
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new TwitterAnalysis(null));
        }

        private TwitterAnalysis CreateSentimentAnalysis()
        {
            return new TwitterAnalysis(mockStreamApiClient.Object);
        }
    }
}