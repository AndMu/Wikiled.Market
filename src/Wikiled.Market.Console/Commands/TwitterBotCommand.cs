using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wikiled.Common.Utilities.Rx;
using Wikiled.Console.Arguments;
using Wikiled.Market.Console.Commands.Config;
using Wikiled.Market.Console.Logic;

namespace Wikiled.Market.Console.Commands
{
    /// <summary>
    /// bot -Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,AXP,KO,INTC,PM,TIF,WFC,MS,JPM
    /// </summary>
    public class TwitterBotCommand : Command
    {
        private readonly ILogger<TwitterBotCommand> log;

        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private readonly TwitterBotConfig botConfig;

        private readonly IObservableTimer timerCreator;

        private readonly IMarketMonitor marketMonitor;

        private readonly ISentimentMonitor sentimentMonitor;

        private IChartMonitor chartMonitor;

        public TwitterBotCommand(
            ILogger<TwitterBotCommand> log,
            TwitterBotConfig botConfig,
            IObservableTimer timerCreator,
            IMarketMonitor marketMonitor,
            ISentimentMonitor sentimentMonitor,
            IChartMonitor chartMonitor)
        : base(log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.botConfig = botConfig ?? throw new ArgumentNullException(nameof(botConfig));
            this.timerCreator = timerCreator ?? throw new ArgumentNullException(nameof(timerCreator));
            this.marketMonitor = marketMonitor ?? throw new ArgumentNullException(nameof(marketMonitor));
            this.sentimentMonitor = sentimentMonitor ?? throw new ArgumentNullException(nameof(sentimentMonitor));
            this.chartMonitor = chartMonitor ?? throw new ArgumentNullException(nameof(chartMonitor));
        }

        public override Task StopExecution(CancellationToken token)
        {
            disposable.Dispose();
            return base.StopExecution(token);
        }

        protected override Task Execute(CancellationToken token)
        {
            log.LogInformation("Loading security...");
            Process();
            return Task.CompletedTask;
        }

        private void Process()
        {
            string[] stockItems = botConfig.ApplicationConfig.Stocks;
            if (!string.IsNullOrEmpty(botConfig.Stocks))
            {
                log.LogInformation("Overriding configured stock");
                stockItems = botConfig.Stocks.Split(",");
            }

            var timer = timerCreator.Daily(TimeSpan.FromHours(6)).Select(item => marketMonitor.ProcessMarket(stockItems)).Subscribe();
            disposable.Add(timer);
            timer = timerCreator.Daily(TimeSpan.FromHours(12))
                                       .StartWith(1)
                                       .Select(item => chartMonitor.ProcessMarket(stockItems))
                                       .Subscribe();
            disposable.Add(timer);
            timer = timerCreator.Daily(TimeSpan.FromHours(9), TimeSpan.FromHours(14))
                .StartWith(1)
                .Select(item => sentimentMonitor.ProcessSentimentAll(stockItems))
                .Subscribe();
            disposable.Add(timer);
        }
    }
}
