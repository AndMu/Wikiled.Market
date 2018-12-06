using System.Threading.Tasks;
using Wikiled.Sentiment.Tracking.Logic;

namespace Wikiled.Market.Console.Logic.Charts
{
    public interface IDayChartGenerator
    {
        void AddSeriesByDay(string name, RatingRecord[] records);

        Task<byte[]> GenerateGraph();
    }
}