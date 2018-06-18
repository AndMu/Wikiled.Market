using System;
using System.Linq;
using Tweetinvi.Models.DTO;
using Wikiled.Twitter.Persistency;
using Wikiled.Twitter.Security;
using Wikiled.Twitter.Streams;

namespace Wikiled.Market.Sentiment
{
    public class StreamMonitor : IDisposable, IPersistency
    {
        private TimingStreamSource streamSource;

        private MonitoringStream stream;

        private FlatFileSerializer serializer;

        private readonly IAuthentication authentication;

        private readonly ISentimentAnalysis sentiment;

        public StreamMonitor(IAuthentication authentication, ISentimentAnalysis sentiment)
        {
            this.authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            this.sentiment = sentiment ?? throw new ArgumentNullException(nameof(sentiment));
        }

        public void Start(string path, IStockTracker[] trackers)
        {
            streamSource = new TimingStreamSource(path, TimeSpan.FromDays(1));
            serializer = new FlatFileSerializer(streamSource);
            stream = new MonitoringStream(this, authentication);
            stream.Start(trackers.Select(item => item.Stock).ToArray(), new string[]{});
        }

        public void Dispose()
        {
            streamSource?.Dispose();
            stream?.Dispose();
        }

        public void Save(ITweetDTO tweet)
        {
            serializer?.Save(tweet);
        }
    }
}
