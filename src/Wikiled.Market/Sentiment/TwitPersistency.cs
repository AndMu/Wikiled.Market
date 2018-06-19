using System.IO;
using System.Text;
using CsvHelper;
using Tweetinvi.Models.DTO;
using Wikiled.Twitter.Persistency;

namespace Wikiled.Market.Sentiment
{
    public class TwitPersistency
    {
        private readonly IStreamSource streamSource;

        private readonly object syncRoot = new object();

        public TwitPersistency(IStreamSource streamSource)
        {
            this.streamSource = streamSource;
        }

        public void Save(ITweetDTO message, double? sentiment)
        {
            var text = message.Text.Replace("\r\n", " ");
            lock (syncRoot)
            {
                var stream = streamSource.GetStream();
                using (var streamOut = new StreamWriter(stream, Encoding.UTF8, 4096, true))
                using (var csvDataTarget = new CsvWriter(streamOut))
                {
                    csvDataTarget.Configuration.Delimiter = "\t";
                    csvDataTarget.WriteField(message.CreatedAt);
                    csvDataTarget.WriteField(message.Id);
                    csvDataTarget.WriteField(message.CreatedBy.Id);
                    csvDataTarget.WriteField(sentiment);
                    csvDataTarget.WriteField(text);
                    csvDataTarget.NextRecord();
                    streamOut.Flush();
                }

                stream.Flush();
            }
        }
    }
}
