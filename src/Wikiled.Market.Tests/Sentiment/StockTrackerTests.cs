using System;
using Moq;
using NUnit.Framework;
using Wikiled.Common.Utilities.Config;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Tests.Sentiment
{
    [TestFixture]
    public class StockTrackerTests
    {
        private Mock<IApplicationConfiguration> mockApplicationConfiguration;

        private StockTracker instance;

        [SetUp]
        public void SetUp()
        {
            mockApplicationConfiguration = new Mock<IApplicationConfiguration>();
            instance = CreateStockTracker();
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new StockTracker(null, "Test"));
            Assert.Throws<ArgumentNullException>(() => new StockTracker(mockApplicationConfiguration.Object, null));
            Assert.AreEqual("$AAPL", instance.Twitter);
            Assert.AreEqual("AAPL", instance.Stock);
            Assert.AreEqual(0, instance.TotalMessages);
        }

        [Test]
        public void CalculateSentiment()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            var resultSentiment = instance.AverageSentiment();
            var resultCount = instance.TotalWithSentiment();
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);

            instance.AddRating(null);
            resultSentiment = instance.AverageSentiment();
            resultCount = instance.TotalWithSentiment();
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);

            instance.AddRating(1);
            resultSentiment = instance.AverageSentiment();
            resultCount = instance.TotalWithSentiment();
            Assert.AreEqual(1, resultSentiment);
            Assert.AreEqual(1, resultCount);

            instance.AddRating(0);
            resultSentiment = instance.AverageSentiment();
            resultCount = instance.TotalWithSentiment();
            Assert.AreEqual(0.5, resultSentiment);
            Assert.AreEqual(2, resultCount);
        }

        [Test]
        public void CalculateSentimentExpire()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            instance.AddRating(1);
            instance.AddRating(1);
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow.AddDays(2));
            instance.AddRating(0);
            var resultSentiment = instance.AverageSentiment();
            var resultCount = instance.TotalWithSentiment();
            Assert.AreEqual(0, resultSentiment);
            Assert.AreEqual(1, resultCount);
        }

        [Test]
        public void CalculatedHours()
        {
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow);
            instance.AddRating(1);
            instance.AddRating(1);
            mockApplicationConfiguration.Setup(item => item.Now).Returns(DateTime.UtcNow.AddHours(2));
            
            var resultSentiment = instance.AverageSentiment(1);
            var resultCount = instance.TotalWithSentiment(1);
            Assert.IsNull(resultSentiment);
            Assert.AreEqual(0, resultCount);
        }

        private StockTracker CreateStockTracker()
        {
            return new StockTracker(mockApplicationConfiguration.Object, "AAPL");
        }
    }
}