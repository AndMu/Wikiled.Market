using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Trady.Importer;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Wikiled.Common.Net.Client;
using Wikiled.Common.Utilities.Config;
using Wikiled.Common.Utilities.Rx;
using Wikiled.Console.Arguments;
using Wikiled.Market.Analysis;
using Wikiled.Market.Sentiment;
using Wikiled.Twitter.Security;

namespace Wikiled.Market.Console.Commands
{
    /// <summary>
    /// Wikiled.Market.Console.exe bot -Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,AXP,KO,INTC,PM,TIF,WFC,MS,JPM
    /// </summary>
    public class TwitterBotCommand : Command
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly ApplicationConfiguration configuration = new ApplicationConfiguration();

        private ITwitterCredentials cred;

        private IDisposable timer = default;

        private IDisposable twitterTimer = null;

        private StreamMonitor monitor;

        private IStockTracker[] trackers;

        public override string Name => "Bot";

        [Description("For what stocks generate prediction")]
        public string Stocks { get; set; }

        public bool IsKeyAuth { get; set; }

        public override Task StopExecution(CancellationToken token)
        {
            monitor?.Dispose();
            timer?.Dispose();
            twitterTimer?.Dispose();
            return base.StopExecution(token);
        }

        protected override Task Execute(CancellationToken token)
        {
            log.Info("Loading security...");
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            if (IsKeyAuth)
            {
                log.Info("Using key auth");
                cred = Analysis.Credentials.TwitterCredentials;
                if (string.IsNullOrWhiteSpace(cred.ConsumerKey) ||
                   string.IsNullOrWhiteSpace(cred.ConsumerSecret))
                {
                    throw new ArgumentNullException("Consumer key not found");
                }
            }
            else
            {
                var auth = new PersistedAuthentication(new PinConsoleAuthentication());
                cred = auth.Authenticate(Analysis.Credentials.TwitterCredentials);
            }

            if (string.IsNullOrWhiteSpace(cred.AccessToken) ||
                string.IsNullOrWhiteSpace(cred.AccessTokenSecret))
            {
                throw new ArgumentNullException("Access token not found");
            }

            if (string.IsNullOrWhiteSpace(Analysis.Credentials.QuandlKey))
            {
                throw new ArgumentNullException("QuandlKey not found");
            }

            trackers = Stocks.Split(',').Select(item => new StockTracker(configuration, item)).ToArray();
            Process();
            monitor = new StreamMonitor(new BasicAuthentication(cred), new SentimentAnalysis(new StreamApiClient(new HttpClient(), new Uri("http://sentiment.wikiled.com/api/sentiment/"))), trackers);
            monitor.Start(@"c:\logging\market\twitter");
            return Task.CompletedTask;
        }

        private void Process()
        {
            var instance = new AnalysisManager(new DataSource(new QuandlWikiImporter(Analysis.Credentials.QuandlKey)), new ClassifierFactory());
            var timerCreator = new ObservableTimer(configuration);
            timer = timerCreator.Daily(TimeSpan.FromHours(6)).Subscribe(item => ProcessMarket(instance));
            twitterTimer = Observable.Interval(TimeSpan.FromHours(3)).Subscribe(item => ProcessSentiment());
        }

        private void ProcessSentiment()
        {
            log.Info("Processing market");
            StringBuilder text = new StringBuilder();
            foreach (var stock in trackers)
            {
                text.AppendLine($"{stock.Twitter} ({stock.TotalMessages}) with average sentiment - {stock.AverageSentiment()}(24H) {stock.AverageSentiment(1)}(1H)");
            }

            PublishMessage(text.ToString());
        }

        private void ProcessMarket(AnalysisManager instance)
        {
            log.Info("Processing market");
            foreach (var stock in trackers)
            {
                log.Info("Processing {0}", stock);
                StringBuilder text = new StringBuilder();
                var result = instance.Start(stock.Stock).Result;
                var sellAccuracy = result.Performance.PerClassMatrices[0].Accuracy;
                var buyAccuracy = result.Performance.PerClassMatrices[1].Accuracy;
                text.AppendLine($"${stock} trading signals ({sellAccuracy * 100:F0}%/{buyAccuracy * 100:F0}%)");
                text.AppendLine($"Total twitter messages {stock.TotalMessages} with average sentiment - {stock.AverageSentiment()}(24H) {stock.AverageSentiment(1)}(1H)");
                for (int i = 0; i < result.Predictions.Length || i < 2; i++)
                {
                    var prediction = result.Predictions[result.Predictions.Length - i - 1];
                    log.Info("{2}, Predicted T-{0}: {1}\r\n", i, prediction, stock);
                    text.AppendFormat("T-{0}: {1}\r\n", i, prediction);
                }

                PublishMessage(text.ToString());
            }
        }

        private void PublishMessage(string text)
        {
            Auth.ExecuteOperationWithCredentials(
                cred,
                () =>
                    {
                        var message = Tweet.PublishTweet(text, new PublishTweetOptionalParameters { });
                        if (message == null)
                        {
                            var exception = ExceptionHandler.GetLastException();
                            if (exception != null)
                            {
                                foreach (var exceptionTwitterExceptionInfo in exception.TwitterExceptionInfos)
                                {
                                    log.Error(exceptionTwitterExceptionInfo.Message);
                                }
                            }
                        }
                    });
        }
    }
}
