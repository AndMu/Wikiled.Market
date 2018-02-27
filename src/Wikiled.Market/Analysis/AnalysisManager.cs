using System.Threading.Tasks;
using Trady.Analysis;
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

        public async Task Start(string stock)
        {
            var data = await importer.ImportAsync(stock).ConfigureAwait(false);
            var momentumOne = data.CloseDiff(1);
            var momentumFive = data.CloseDiff(5);
        }
    }
}
