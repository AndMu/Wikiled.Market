using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using NLog.Extensions.Logging;
using Trady.Importer;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Wikiled.Common.Net.Client;
using Wikiled.Common.Utilities.Config;
using Wikiled.Common.Utilities.Rx;
using Wikiled.Console.Arguments;
using Wikiled.Market.Analysis;
using Wikiled.Market.Console.Config;
using Wikiled.Market.Sentiment;
using Wikiled.Twitter.Monitor.Api.Response;
using Wikiled.Twitter.Security;
using Credentials = Wikiled.Market.Analysis.Credentials;

namespace Wikiled.Market.Console.Commands
{
    /// <summary>
    /// Wikiled.Market.Console.exe bot -Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,AXP,KO,INTC,PM,TIF,WFC,MS,JPM
    /// </summary>
    public class TwitterBotCommand : Command
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private readonly ApplicationConfiguration configuration = new ApplicationConfiguration();

        private readonly Credentials credentials;

        private TwitterAnalysis twitterAnalysis;

        private ITwitterCredentials cred;

        private IDisposable timer = default;

        private IDisposable twitterTimer = null;

        private ILoggerFactory factory;

        public TwitterBotCommand()
        {
            credentials = new Credentials(configuration);
        }

        public override string Name => "Bot";

        [Description("For what stocks generate prediction")]
        public string Stocks { get; set; }

        public bool IsService { get; set; }

        public override Task StopExecution(CancellationToken token)
        {
            timer?.Dispose();
            twitterTimer?.Dispose();
            return base.StopExecution(token);
        }

        protected override Task Execute(CancellationToken token)
        {
            factory = new LoggerFactory();
            factory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

            log.Info("Loading security...");
            if (IsService)
            {
                cred = new EnvironmentAuthentication(configuration).Authenticate();
            }
            else
            {
                var config = LoadConfig();
                log.Info("Authenticating using user account");
                var auth = new PersistedAuthentication(new PinConsoleAuthentication(new TwitterCredentials(config.AccessToken, config.AccessTokenSecret)));
                cred = auth.Authenticate();
            }

            if (string.IsNullOrWhiteSpace(cred.AccessToken) ||
                string.IsNullOrWhiteSpace(cred.AccessTokenSecret))
            {
                throw new ArgumentNullException("Access token not found");
            }

            if (string.IsNullOrWhiteSpace(credentials.QuandlKey))
            {
                throw new ArgumentNullException("QuandlKey not found");
            }

            var logger = factory.CreateLogger<StreamApiClient>();
            twitterAnalysis = new TwitterAnalysis(new StreamApiClient(new HttpClient() { Timeout = TimeSpan.FromMinutes(2) }, new Uri("http://192.168.0.200:7070/api/twiter/"), logger));
            Process();
            return Task.CompletedTask;
        }

        private void Process()
        {
            var instance = new AnalysisManager(new DataSource(new QuandlWikiImporter(credentials.QuandlKey)), new ClassifierFactory());
            var timerCreator = new ObservableTimer(configuration);
            var stockItems = Stocks.Split(",");
            timer = timerCreator.Daily(TimeSpan.FromHours(6)).Select(item => ProcessMarket(instance, stockItems)).Subscribe();
            twitterTimer = Observable.Interval(TimeSpan.FromHours(3)).Select(item => ProcessSentiment(stockItems)).Subscribe();
        }

        private async Task ProcessSentiment(string[] stockItems)
        {
            log.Info("Processing market");
            StringBuilder text = new StringBuilder();
            foreach (var stock in stockItems)
            {
                var sentiment = await twitterAnalysis.GetSentiment($"${stock}");
                if (sentiment != null)
                {
                    text.AppendLine($"{stock} ({sentiment.Total}) with average sentiment:");
                    ExtractResult(sentiment, text);
                }
                else
                {
                    log.Warn("Not found sentiment for {0}", stock);
                }
            }

            PublishMessage(text.ToString());
        }

        private async Task ProcessMarket(AnalysisManager instance, string[] stockItems)
        {
            log.Info("Processing market");
            foreach (var stock in stockItems)
            {
                log.Info("Processing {0}", stock);
                StringBuilder text = new StringBuilder();
                var sentimentTask = twitterAnalysis.GetSentiment($"${stock}");
                var result = await instance.Start(stock);
                var sellAccuracy = result.Performance.PerClassMatrices[0].Accuracy;
                var buyAccuracy = result.Performance.PerClassMatrices[1].Accuracy;
                text.AppendLine($"${stock} trading signals ({sellAccuracy * 100:F0}%/{buyAccuracy * 100:F0}%)");
                var sentiment = await sentimentTask;
                if (sentiment != null)
                {
                    text.AppendLine($"Total twitter messages {sentiment.Total} with average sentiment:");
                    ExtractResult(sentiment, text);
                }
                else
                {
                    log.Warn("Not found sentiment for {0}", stock);
                }

                for (int i = 0; i < result.Predictions.Length || i < 2; i++)
                {
                    var prediction = result.Predictions[result.Predictions.Length - i - 1];
                    log.Info("{2}, Predicted T-{0}: {1}\r\n", i, prediction, stock);
                    text.AppendFormat("T-{0}: {1}\r\n", i, prediction);
                }

                PublishMessage(text.ToString());
            }
        }

        private static void ExtractResult(TrackingResults sentiment, StringBuilder text)
        {
            foreach (var keyValue in sentiment.Sentiment)
            {
                text.AppendLine($" [{keyValue.Key}]: {keyValue.Value.AverageSentiment}({keyValue.Value.TotalMessages} msg.)");
            }
        }

        private void PublishMessage(string text)
        {
            Auth.ExecuteOperationWithCredentials(
                cred,
                () =>
                    {
                        var message = Tweet.PublishTweet(text, new PublishTweetOptionalParameters());
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

        private MonitoringConfig LoadConfig()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!File.Exists(Path.Combine(directory, "service.json")))
            {
                throw new Exception("Configuration file service.json not found");

            }

            return JsonConvert.DeserializeObject<MonitoringConfig>(File.ReadAllText(Path.Combine(directory, "service.json")));
        }
    }
}
