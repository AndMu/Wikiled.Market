using System.Threading.Tasks;
using NUnit.Framework;
using Trady.Importer;
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
            instance = new AnalysisManager(new QuandlWikiImporter(Credentials.QuandlKey));
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public async Task Construct()
        {
            await instance.Start().ConfigureAwait(false);
        }
    }
}