using System;
using Moq;
using NUnit.Framework;
using Wikiled.Market.Analysis;

namespace Wikiled.Market.Tests.Analysis
{
    [TestFixture]
    public class AnalysisManagerTests
    {
        private Mock<IDataSource> dataSource;

        private Mock<IClassifierFactory> classifierMock;

        private AnalysisManager instance;

        [SetUp]
        public void SetUp()
        {
            dataSource = new Mock<IDataSource>();
            classifierMock = new Mock<IClassifierFactory>();
            instance = CreateManager();
        }
      
        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new AnalysisManager(null, classifierMock.Object));
            Assert.Throws<ArgumentNullException>(() => new AnalysisManager(dataSource.Object, null));
        }

        private AnalysisManager CreateManager()
        {
            return new AnalysisManager(dataSource.Object, classifierMock.Object);
        }
    }
}