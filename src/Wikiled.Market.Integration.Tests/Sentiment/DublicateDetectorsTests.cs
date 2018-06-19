using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using Wikiled.Market.Sentiment;

namespace Wikiled.Market.Integration.Tests.Sentiment
{
    [TestFixture]
    public class DublicateDetectorsTests
    {
        private DublicateDetectors instance;

        private MemoryCache cache;

        [SetUp]
        public void SetUp()
        {
            cache = new MemoryCache(new MemoryCacheOptions());
            instance = CreateDublicateDetectors();
        }

        [TearDown]
        public void TestCleanup()
        {
            cache.Dispose();
        }

        [Test]
        public void Construct()
        {
            var result = instance.HasReceived(@"Test Message http://dadas.com");
            Assert.IsFalse(result);
            result = instance.HasReceived(@"Test Message http://dadaczxczxs.com");
            Assert.IsTrue(result);
            result = instance.HasReceived(@"Test Message 3");
            Assert.IsFalse(result);
        }

        private DublicateDetectors CreateDublicateDetectors()
        {
            return new DublicateDetectors(cache);
        }
    }
}