using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Net.Client;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Tests.Sentiment
{
    [TestFixture]
    public class TwitterAnalysisTests
    {
        private Mock<IApiClient> mockStreamApiClient;

        private TwitterAnalysis instance;

        private readonly ILogger<TwitterAnalysis> logger = new Logger<TwitterAnalysis>(new NullLoggerFactory());

        [SetUp]
        public void SetUp()
        {
            mockStreamApiClient = new Mock<IApiClient>();
            instance = CreateSentimentAnalysis();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new TwitterAnalysis(null, logger));
            Assert.Throws<ArgumentNullException>(() => new TwitterAnalysis(mockStreamApiClient.Object, null));
        }

        private TwitterAnalysis CreateSentimentAnalysis()
        {
            return new TwitterAnalysis(mockStreamApiClient.Object, logger);
        }
    }
}