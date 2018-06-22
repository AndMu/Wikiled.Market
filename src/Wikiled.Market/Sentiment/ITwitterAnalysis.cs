using System.Threading.Tasks;
using Wikiled.Twitter.Monitor.Api.Response;

namespace Wikiled.Market.Sentiment
{
    public interface ITwitterAnalysis
    {
        Task<TrackingResults> GetSentiment(string text);
    }
}