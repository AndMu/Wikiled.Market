using System;
using System.ComponentModel;
using System.IO;
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
using Polly;
using Trady.Importer.Quandl;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Wikiled.Common.Net.Client;
using Wikiled.Common.Utilities.Config;
using Wikiled.Common.Utilities.Rx;
using Wikiled.Console.Arguments;
using Wikiled.MachineLearning.Mathematics.Tracking;
using Wikiled.Market.Analysis;
using Wikiled.Market.Console.Config;
using Wikiled.SeekingAlpha.Api.Request;
using Wikiled.SeekingAlpha.Api.Service;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Monitor.Api.Service;
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

        private ITwitterAnalysis twitterAnalysis;

        private IAlphaAnalysis alpha;

        private ITwitterCredentials cred;

        private IDisposable timer;

        private IDisposable twitterTimer;

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
            var config = LoadConfig();
            if (IsService)
            {
                cred = new EnvironmentAuthentication(configuration).Authenticate();
            }
            else
            {
                log.Info("Authenticating using user account");
                var auth = new PersistedAuthentication(new PinConsoleAuthentication(new TwitterCredentials(config.Twitter.AccessToken, config.Twitter.AccessTokenSecret)));
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

            twitterAnalysis = new TwitterAnalysis(new ApiClientFactory(new HttpClient {Timeout = TimeSpan.FromMinutes(5)}, new Uri(config.Sentiment.Service)));
            alpha = new AlphaAnalysis(new ApiClientFactory(new HttpClient { Timeout = TimeSpan.FromMinutes(5) }, new Uri(config.Sentiment.Alpha)));
            Process();
            return Task.CompletedTask;
        }

        private void Process()
        {
            var instance = new AnalysisManager(new DataSource(new QuandlWikiImporter(credentials.QuandlKey)), new ClassifierFactory());
            var timerCreator = new ObservableTimer(configuration, new NLogLoggerFactory());
            var stockItems = Stocks.Split(",");
            timer = timerCreator.Daily(TimeSpan.FromHours(6)).Select(item => ProcessMarket(instance, stockItems)).Subscribe();
            twitterTimer = Observable.Interval(TimeSpan.FromHours(3)).StartWith(1).Select(item => ProcessSentimentAll(stockItems)).Subscribe();
        }

        private async Task ProcessSentimentAll(string[] stockItems)
        {
            var twitterTask = ProcessSentiment(stockItems,
                                               "Twitter",
                                               stock => twitterAnalysis.GetTrackingResults($"${stock}", CancellationToken.None));
            
            var seekingAlpha = ProcessSentiment(stockItems,
                                               "SeekingAlpha Editors",
                                               stock => alpha.GetTrackingResults(new SentimentRequest(stock, SentimentType.Article), CancellationToken.None));

            var seekingAlphaComments = ProcessSentiment(stockItems,
                                                "SeekingAlpha Comments",
                                                stock => alpha.GetTrackingResults(new SentimentRequest(stock, SentimentType.Comment), CancellationToken.None));

            await Task.WhenAll(twitterTask, seekingAlphaComments, seekingAlpha).ConfigureAwait(false);
        }

        private async Task ProcessSentiment(string[] stockItems, string type, Func<string, Task<TrackingResults>> retrieve)
        {
            log.Info("Retrieving sentiment {0}...", type);
            StringBuilder text = new StringBuilder();
            text.AppendLine($"Average sentiment (from {type}):");
            var policy = Policy.HandleResult<TrackingResults>(r => r == null);

            foreach (var stock in stockItems)
            {
                var sentiment = await policy.RetryAsync(3)
                    .ExecuteAsync(() => retrieve(stock))
                    .ConfigureAwait(false);
                if (sentiment != null)
                {
                    if (sentiment.Sentiment.ContainsKey("6H"))
                    {
                        var value = sentiment.Sentiment["6H"];
                        if (value.TotalMessages > 0)
                        {
                            text.AppendFormat("${2}: {3}{0:F2}({1}) ",
                                              value.Average,
                                              value.TotalMessages,
                                              stock,
                                              GetEmoji(value));
                        }
                    }
                }
                else
                {
                    log.Warn("Not found sentiment for {0}", stock);
                }
            }

            PublishMessage(text.ToString());
        }

        private string GetEmoji(TrackingResult result)
        {
            if (result.Average < 0)
            {
                return Emoji.CHART_WITH_DOWNWARDS_TREND.Unicode;
            }

            return result.Average > 0 ? Emoji.CHART_WITH_UPWARDS_TREND.Unicode : string.Empty;
        }

        private async Task ProcessMarket(AnalysisManager instance, string[] stockItems)
        {
            log.Info("Processing market");
            foreach (var stock in stockItems)
            {
                log.Info("Processing {0}", stock);
                StringBuilder text = new StringBuilder();
                var sentimentTask = twitterAnalysis.GetTrackingResults($"${stock}", CancellationToken.None);
                var result = await instance.Start(stock).ConfigureAwait(false);
                var sellAccuracy = result.Performance.PerClassMatrices[0].Accuracy;
                var buyAccuracy = result.Performance.PerClassMatrices[1].Accuracy;
                text.AppendLine($"${stock} trading signals ({sellAccuracy * 100:F0}%/{buyAccuracy * 100:F0}%)");
                var sentiment = await sentimentTask.ConfigureAwait(false);
                if (sentiment != null)
                {
                    if (sentiment.Sentiment.TryGetValue("24H", out var sentimentValue))
                    {
                        text.AppendFormat("Average sentiment: {2}{0:F2}({1})\r\n",
                            sentimentValue.Average,
                            sentimentValue.TotalMessages,
                            GetEmoji(sentimentValue));
                    }
                }
                else
                {
                    log.Warn("Not found sentiment for {0}", stock);
                }

                for (int i = 0; i < result.Predictions.Length || i < 2; i++)
                {
                    var prediction = result.Predictions[result.Predictions.Length - i - 1];
                    log.Info("{2}, Predicted T-{0}: {1}\r\n", i, prediction, stock);
                    var icon = prediction == MarketDirection.Buy ? Emoji.CHART_WITH_UPWARDS_TREND.Unicode : Emoji.CHART_WITH_DOWNWARDS_TREND.Unicode;
                    text.AppendFormat("T-{0}: {2}{1}\r\n", i, prediction, icon);
                }

                PublishMessage(text.ToString());
            }
        }

        private void PublishMessage(string text)
        {
            log.Info("Publishing message");
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

        private ApplicationConfig LoadConfig()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!File.Exists(Path.Combine(directory, "service.json")))
            {
                throw new Exception("Configuration file service.json not found");

            }

            return JsonConvert.DeserializeObject<ApplicationConfig>(File.ReadAllText(Path.Combine(directory, "service.json")));
        }
    }
}
