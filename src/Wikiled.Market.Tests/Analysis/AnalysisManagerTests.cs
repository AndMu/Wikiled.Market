using System;
using Moq;
using NUnit.Framework;
using Trady.Core.Infrastructure;
using Wikiled.Market.Analysis;

namespace Wikiled.Market.Tests.Analysis
{
    [TestFixture]
    public class AnalysisManagerTests
    {
        private Mock<IImporter> importer;

        private AnalysisManager instance;

        [SetUp]
        public void SetUp()
        {
            instance = CreateManager();
            importer = new Mock<IImporter>();
        }
      
        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new AnalysisManager(importer.Object));
        }

        private AnalysisManager CreateManager()
        {
            return new AnalysisManager(importer.Object);
        }
    }
}