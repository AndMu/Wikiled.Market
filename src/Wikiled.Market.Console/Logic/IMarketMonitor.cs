using System.Threading.Tasks;

namespace Wikiled.Market.Console.Logic
{
    public interface IMarketMonitor
    {
        Task ProcessMarket(string[] stockItems);
    }
}