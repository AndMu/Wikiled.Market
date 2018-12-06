using System.Threading.Tasks;

namespace Wikiled.Market.Console.Logic.Charts
{
    public interface ISentimentChartGeneration
    {
        Task AddStock(string stock);

        Task<byte[]> Generate();
    }
}