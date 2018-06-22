using System.Threading.Tasks;
using NUnit.Framework;
using Trady.Importer;
using Wikiled.Common.Utilities.Config;
using Wikiled.Market.Analysis;

namespace Wikiled.Market.Integration.Tests.Analysis
{
    [TestFixture]
    public class AnalysisManagerTests
    {
        private AnalysisManager instance;

        [SetUp]
        public void SetUp()
        {
            instance = new AnalysisManager(new DataSource(new QuandlWikiImporter(new Credentials(new ApplicationConfiguration()).QuandlKey)), new ClassifierFactory());
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task Construct()
        {
            await instance.Start("AAPL").ConfigureAwait(false);
        }
    }
}