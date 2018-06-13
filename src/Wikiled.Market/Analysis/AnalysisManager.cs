using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Common.Arguments;

namespace Wikiled.Market.Analysis
{
    public class AnalysisManager
    {
        private readonly IDataSource dataSource;

        private readonly IClassifierFactory classifierFactory;

        public AnalysisManager(IDataSource dataSource, IClassifierFactory classifierFactory)
        {
            Guard.NotNull(() => dataSource, dataSource);
            Guard.NotNull(() => classifierFactory, classifierFactory);
            this.dataSource = dataSource;
            this.classifierFactory = classifierFactory;
        }

        public async Task<PredictionResult> Start(string stock)
        {
            var classifier = classifierFactory.Construct();
            var data = await dataSource.GetData(stock, DateTime.Today.AddYears(-20), DateTime.Today).ConfigureAwait(false);
            await Task.Run(() => classifier.Train(data, CancellationToken.None)).ConfigureAwait(false);
            var result = classifier.Classify(data.Prediction);
            return new PredictionResult(result.Select(item => (MarketDirection)item).ToArray(), classifier.TestSetPerformance);
        }
    }
}
