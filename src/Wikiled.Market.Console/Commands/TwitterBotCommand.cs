using Microsoft.Extensions.Logging;
using System;
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
    /// Wikiled.Market.Console.exe bot -Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,AXP,KO,INTC,PM,TIF,WFC,MS,JPM
    /// </summary>
    public class TwitterBotCommand : Command
    {
        private readonly ILogger<TwitterBotCommand> log;

        private IDisposable timer;

        private IDisposable twitterTimer;

        private readonly TwitterBotConfig botConfig;

        private readonly IObservableTimer timerCreator;

        private readonly IMarketMonitor marketMonitor;

        private readonly ISentimentMonitor sentimentMonitor;

        public TwitterBotCommand(ILogger<TwitterBotCommand> log,
                                 TwitterBotConfig botConfig,
                                 IObservableTimer timerCreator,
                                 IMarketMonitor marketMonitor,
                                 ISentimentMonitor sentimentMonitor)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.botConfig = botConfig ?? throw new ArgumentNullException(nameof(botConfig));
            this.timerCreator = timerCreator ?? throw new ArgumentNullException(nameof(timerCreator));
            this.marketMonitor = marketMonitor ?? throw new ArgumentNullException(nameof(marketMonitor));
            this.sentimentMonitor = sentimentMonitor ?? throw new ArgumentNullException(nameof(sentimentMonitor));
        }

        public override Task StopExecution(CancellationToken token)
        {
            timer?.Dispose();
            twitterTimer?.Dispose();
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

            timer = timerCreator.Daily(TimeSpan.FromHours(6)).Select(item => marketMonitor.ProcessMarket(stockItems)).Subscribe();
            twitterTimer = timerCreator.Daily(TimeSpan.FromHours(9), TimeSpan.FromHours(14), TimeSpan.FromHours(21))
                .StartWith(1)
                .Select(item => sentimentMonitor.ProcessSentimentAll(stockItems))
                .Subscribe();
        }
    }
}
