using System.Threading.Tasks;

namespace Wikiled.Market.Console.Logic.Charts
{
    public interface ISentimentChartGeneration
    {
        Task<byte[]> Generate();
    }
}