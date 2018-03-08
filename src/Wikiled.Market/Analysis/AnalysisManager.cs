using System;
using System.Threading;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Core.Infrastructure;
using Wikiled.Arff.Normalization;
using Wikiled.Arff.Persistence;
using Wikiled.Common.Arguments;
using Wikiled.MachineLearning.Svm.Clients;
using Wikiled.MachineLearning.Svm.Data;
using Wikiled.MachineLearning.Svm.Logic;
using Wikiled.MachineLearning.Svm.Parameters;

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
            var macd = data.Macd(12, 26, 9);
            var bb = data.Bb(20, 2);
            var atr = data.Atr(12);
            var adx = data.Adx(6);
            var rsi = data.Rsi(15);
            var arff = ArffDataSet.CreateSimple("Market");
            arff.Header.RegisterEnumClass<MarketDirection>();
            arff.IsSparse = false;
            for (int i = marketChangeInDays; i < (momentumOne.Count - marketChangeInDays); i++)
            {
                var document = arff.AddDocument();
                document.AddRecord("MOM").Value = momentumOne[i].Tick ?? 0;
                document.AddRecord("MOM5").Value = momentumFive[i].Tick ?? 0;
                document.AddRecord("MacdHistogram").Value = macd[i].Tick.MacdHistogram ?? 0;
                document.AddRecord("MacdLine").Value = macd[i].Tick.MacdLine ?? 0;
                document.AddRecord("SignalLine").Value = macd[i].Tick.SignalLine ?? 0;
                document.AddRecord("atr").Value = atr[i].Tick ?? 0;
                document.AddRecord("adx").Value = adx[i].Tick ?? 0;
                document.AddRecord("rsi").Value = rsi[i].Tick ?? 0;
                document.Class.Value = momentumFive[i + marketChangeInDays].Tick > 0 ? MarketDirection.Bullish : MarketDirection.Bearish;
            }

            TrainingHeader header = new TrainingHeader();
            header.GridSelection = true;
            header.Kernel = KernelType.RBF;
            header.SvmType = SvmType.C_SVC;
            arff.SaveCsv(@"c:\1\problem.csv");
            arff.Save(@"c:\1\problem.arff");
            IProblemFactory problemFactory = new ProblemFactory(arff);

            NewSvm newSvm = new NewSvm();
            newSvm.Classify(problemFactory.Construct(arff).GetProblem());

            problemFactory = problemFactory.WithGaussianScaling();

            SvmTraining train = new SvmTraining(problemFactory, arff);
            var parameters = (GridParameterSelection)train.SelectParameters(header, CancellationToken.None);
            parameters.SearchParameters.C = new[] { 0.001, 0.01, 0.1, 1, 10 };
            parameters.SearchParameters.Gamma = new[] { 0.001, 0.01, 0.1, 1 };
            var model = await train.Train(parameters).ConfigureAwait(false);
            model.Save(@"c:\1\model.dat");
            SvmTesting testClient = new SvmTesting(model.Model, problemFactory);
            var result = testClient.Classify(model.DataSet);
            var posF1 = result.Statistics.F1(1);
            var negF1 = result.Statistics.F1(0);
        }
    }
}
