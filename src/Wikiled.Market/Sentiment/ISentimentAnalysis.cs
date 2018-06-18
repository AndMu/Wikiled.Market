using System.Threading.Tasks;

namespace Wikiled.Market.Sentiment
{
    public interface ISentimentAnalysis
    {
        Task<double> MeasureSentiment(string text);
    }
}