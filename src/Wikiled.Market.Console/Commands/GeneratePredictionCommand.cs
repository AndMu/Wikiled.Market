using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Console.Arguments;
using Wikiled.Market.Analysis;
using Wikiled.Market.Console.Commands.Config;

namespace Wikiled.Market.Console.Commands
{
    /// <summary>
    /// generate -Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,AXP,KO,INTC,PM,TIF,WFC,MS,JPM
    /// </summary>
    [Description("Generate Command")]
    public class GeneratePredictionCommand : Command
    {
        private readonly ILogger<GeneratePredictionCommand> log;

        private readonly IAnalysisManager instance;

        private readonly GeneratePredictionConfig config;

        public GeneratePredictionCommand(ILogger<GeneratePredictionCommand> log, GeneratePredictionConfig config, IAnalysisManager instance)
            : base(log)
        {
            this.log = log;
            this.config = config;
            this.instance = instance;
        }

        protected override Task Execute(CancellationToken token)
        {
            var stocks = config.Stocks.Split(',');
            foreach (var stock in stocks)
            {
                log.LogInformation("Calculating {0}", stock);
                var result = instance.Start(stock).Result;
                for (int i = 0; i < result.Predictions.Length; i++)
                {
                    log.LogInformation("{2}, Predicted T-{0}: {1}", i, result.Predictions[result.Predictions.Length - i - 1], stock);
                }
            }

            return Task.CompletedTask;
        }
    }
}
