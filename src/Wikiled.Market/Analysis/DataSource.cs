using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Extension;
using Trady.Core.Infrastructure;
using Wikiled.Arff.Extensions;
using Wikiled.Arff.Logic;

namespace Wikiled.Market.Analysis
{
    public class DataSource : IDataSource
    {
        private readonly IImporter importer;

        private readonly int marketChangeInDays = 5;

        private IReadOnlyList<AnalyzableTick<decimal?>> adx;

        private IReadOnlyList<AnalyzableTick<decimal?>> atr;

        private IReadOnlyList<AnalyzableTick<(decimal? LowerBand, decimal? MiddleBand, decimal? UpperBand)>> bb;

        private IReadOnlyList<AnalyzableTick<(decimal? MacdLine, decimal? SignalLine, decimal? MacdHistogram)>> macd;

        private IReadOnlyList<AnalyzableTick<decimal?>> momentumFive;

        private IReadOnlyList<AnalyzableTick<decimal?>> momentumOne;

        private IReadOnlyList<AnalyzableTick<decimal?>> rsi;

        public DataSource(IImporter importer)
        {
            this.importer = importer ?? throw new ArgumentNullException(nameof(importer));
        }

        public async Task<DataPackage> GetData(string stock, DateTime? from, DateTime? to)
        {
            await LoadData(stock, from, to).ConfigureAwait(false);
            var training = GetTraining();
            var testing = GetTesting();
            var problemData = training.GetData().Where(item => item.Y.HasValue).ToArray();
            var testingData = testing.GetData().Select(item => item.X).ToArray();
            return new DataPackage(problemData.Select(item => item.Y.Value).ToArray(), problemData.Select(item => item.X).ToArray(), testingData);
        }

        private static IArffDataSet CreateArff()
        {
            var training = ArffDataSet.CreateSimple("Market");
            training.Header.RegisterEnumClass<MarketDirection>();
            training.IsSparse = false;
            return training;
        }

        private void AddSignals(IArffDataRow document, int i)
        {
            document.AddRecord("MOM").Value = momentumOne[i].Tick ?? 0;
            document.AddRecord("MOM5").Value = momentumFive[i].Tick ?? 0;
            document.AddRecord("MacdHistogram").Value = macd[i].Tick.MacdHistogram ?? 0;
            document.AddRecord("MacdLine").Value = macd[i].Tick.MacdLine ?? 0;
            document.AddRecord("SignalLine").Value = macd[i].Tick.SignalLine ?? 0;
            document.AddRecord("atr").Value = atr[i].Tick ?? 0;
            document.AddRecord("adx").Value = adx[i].Tick ?? 0;
            document.AddRecord("rsi").Value = rsi[i].Tick ?? 0;
        }

        private IArffDataSet GetTesting()
        {
            var testing = CreateArff();
            for (int i = momentumOne.Count - marketChangeInDays; i < momentumOne.Count; i++)
            {
                var document = testing.AddDocument();
                AddSignals(document, i);
                document.Class.Value = MarketDirection.Buy;
            }

            return testing;
        }

        private IArffDataSet GetTraining()
        {
            var training = CreateArff();
            for (int i = marketChangeInDays; i < momentumOne.Count - marketChangeInDays; i++)
            {
                var document = training.AddDocument();
                AddSignals(document, i);
                document.Class.Value = momentumFive[i + marketChangeInDays].Tick > 0 ? MarketDirection.Buy : MarketDirection.Sell;
            }

            return training;
        }

        private async Task LoadData(string stock, DateTime? from, DateTime? to)
        {
            var data = await importer.ImportAsync(stock, from, to).ConfigureAwait(false);
            momentumOne = data.Mtm(1);
            momentumFive = data.Mtm(5);
            macd = data.Macd(12, 26, 9);
            bb = data.Bb(20, 2);
            atr = data.Atr(12);
            adx = data.Adx(6);
            rsi = data.Rsi(15);
        }
    }
}
