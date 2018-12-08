using System.Threading.Tasks;

namespace Wikiled.Market.Console.Logic.Charts
{
    public interface ISentimentChartGeneration
    {
        Task AddStocks(params string[] stock);

        Task<byte[]> Generate();
    }
}