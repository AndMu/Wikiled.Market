using System.Threading.Tasks;

namespace Wikiled.Market.Sentiment
{
    public interface ITwitterAnalysis
    {
        Task<double?> GetSentiment(string text);
    }
}