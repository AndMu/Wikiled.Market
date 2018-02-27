using System.Threading;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Core.Infrastructure;
using Wikiled.Arff.Normalization;
using Wikiled.Arff.Persistence;
using Wikiled.Common.Arguments;
using Wikiled.MachineLearning.Svm.Clients;
using Wikiled.MachineLearning.Svm.Logic;

namespace Wikiled.Market.Analysis
{
    public class AnalysisManager
    {
        private readonly IImporter importer;

        private int marketChangeInDays = 5;

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
            var arff = ArffDataSet.CreateSimple("Market");
            arff.Header.RegisterEnumClass<MarketDirection>();
            for (int i = 0; i < (momentumOne.Count - marketChangeInDays); i++)
            {
                var document = arff.AddDocument();
                document.AddRecord("MOM").Value = momentumOne[i].Tick ?? 0;
                document.AddRecord("MOM5").Value = momentumFive[i].Tick ?? 0;
                document.Class.Value = momentumFive[i + marketChangeInDays].Tick > 0 ? MarketDirection.Bullish : MarketDirection.Bearish;
            }

            TrainingHeader header = new TrainingHeader();
            header.GridSelection = true;
            header.Kernel = KernelType.RBF;
            header.SvmType = SvmType.C_SVC;
            header.Normalization = NormalizationType.L2;
            arff.Normalize(NormalizationType.L2);
            SvmTrainClient train = new SvmTrainClient(arff);
            var model = await train.Train(header, CancellationToken.None).ConfigureAwait(false);
            SvmTestClient testClient = new SvmTestClient(model.DataSet, model.Model);
            var result = testClient.Test(model.DataSet);
        }
    }
}
