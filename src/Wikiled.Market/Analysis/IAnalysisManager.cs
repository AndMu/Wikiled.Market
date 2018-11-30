using System.Threading.Tasks;

namespace Wikiled.Market.Analysis
{
    public interface IAnalysisManager
    {
        Task<PredictionResult> Start(string stock);
    }
}