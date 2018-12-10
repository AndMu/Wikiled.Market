using System.Threading.Tasks;

namespace Wikiled.Market.Console.Logic
{
    public interface IChartMonitor
    {
        Task ProcessMarket(string[] stockItems);
    }
}