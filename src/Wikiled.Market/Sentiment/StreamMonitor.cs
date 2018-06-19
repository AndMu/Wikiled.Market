using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;
using Tweetinvi.Models.DTO;
using Wikiled.Common.Extensions;
using Wikiled.Text.Analysis.Twitter;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Security;
using Wikiled.Twitter.Streams;

namespace Wikiled.Market.Sentiment
{
    public class StreamMonitor : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private TimingStreamSource streamSource;

        private MonitoringStream stream;

        private FlatFileSerializer serializer;

        private readonly IAuthentication authentication;

        private readonly ISentimentAnalysis sentiment;

        private IDisposable subscription;

        private readonly Extractor extractor = new Extractor();

        private readonly Dictionary<string, IStockTracker> trackersTable = new Dictionary<string, IStockTracker>(StringComparer.OrdinalIgnoreCase);


        public StreamMonitor(IAuthentication authentication, ISentimentAnalysis sentiment, IStockTracker[] trackers)
        {
            this.authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
            trackersTable = trackers.ToDictionary(item => item.Twitter, item => item);
            Trackers = trackers;
        }

        public IStockTracker[] Trackers { get; }

        public void Start(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
            }

            path.EnsureDirectoryExistence();
            streamSource = new TimingStreamSource(path, TimeSpan.FromDays(1));
            serializer = new FlatFileSerializer(streamSource);
            stream = new MonitoringStream(authentication);
            subscription = stream.MessagesReceiving
                .ObserveOn(TaskPoolScheduler.Default)
                .Select(Save)
                .Merge()
                .Subscribe(item =>
                {
                    log.Debug("Processed message: {0}", item.Text);
                });
            Task.Factory.StartNew(async () => await stream.Start(Trackers.Select(item => item.Stock).ToArray(), new string[] { }), TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            subscription?.Dispose();
            streamSource?.Dispose();
            stream?.Dispose();
        }

        private async Task<ITweetDTO> Save(ITweetDTO tweet)
        {
            var saveTask = Task.Run(() => serializer?.Save(tweet));
            var sentimentValue = await sentiment.MeasureSentiment(tweet.Text);
            foreach (var cashTag in extractor.ExtractCashtags(tweet.Text))
            {
                if (trackersTable.TryGetValue(cashTag, out var tracker))
                {
                    tracker.AddRating(sentimentValue);
                }
            }

            await saveTask;
            return tweet;
        }
    }
}
