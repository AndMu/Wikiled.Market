using System.Threading.Tasks;

namespace Wikiled.Market.Console.Logic
{
    public interface ISentimentMonitor
    {
        Task ProcessSentimentAll(string[] stockItems);
    }
}