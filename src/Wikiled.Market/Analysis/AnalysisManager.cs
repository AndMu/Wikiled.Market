using System.Threading.Tasks;
using Trady.Core.Infrastructure;
using Wikiled.Common.Arguments;

namespace Wikiled.Market.Analysis
{
    public class AnalysisManager
    {
        private readonly IImporter importer;

        public AnalysisManager(IImporter importer)
        {
            Guard.NotNull(() => importer, importer);
            this.importer = importer;
        }

        public async Task Start()
        {
            var data = await importer.ImportAsync("AAPL").ConfigureAwait(false);
        }
    }
}
