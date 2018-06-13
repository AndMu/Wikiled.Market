using System.ComponentModel;
using System.Threading.Tasks;
using NLog;
using Trady.Importer;
using Wikiled.Console.Arguments;
using Wikiled.Market.Analysis;

namespace Wikiled.Market.Console.Commands
{
    [Description("Generate Command")]
    public class GeneratePredictionCommand : Command
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public override string Name => "Generate";

        [Description("For what stocks generate prediction")]
        public string Stocks { get; set; }

        public override Task Execute()
        {
            var instance = new AnalysisManager(new DataSource(new QuandlWikiImporter(Credentials.QuandlKey)), new ClassifierFactory());
            var stocks = Stocks.Split(',');
            foreach (var stock in stocks)
            {
                log.Info("Calculating {0}", stock);
                var result = instance.Start(stock).Result;
                for (int i = 0; i < result.Predictions.Length; i++)
                {
                    log.Info("{2}, Predicted T-{0}: {1}", i, result.Predictions[result.Predictions.Length - i - 1], stock);
                }
            }

            return Task.CompletedTask;
        }
    }
}
