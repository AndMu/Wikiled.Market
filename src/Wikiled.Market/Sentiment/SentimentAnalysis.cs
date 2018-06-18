using System;
using System.Threading.Tasks;

namespace Wikiled.Market.Sentiment
{
    public class SentimentAnalysis : ISentimentAnalysis
    {
        public Task<double> MeasureSentiment(string text)
        {
            throw new NotImplementedException();
        }
    }
}
