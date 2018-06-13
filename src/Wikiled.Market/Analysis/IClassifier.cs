using System.Threading;
using Accord.Statistics.Analysis;

namespace Wikiled.Market.Analysis
{
    public interface IClassifier
    {
        GeneralConfusionMatrix TestSetPerformance { get; }

        void Train(DataPackage data, CancellationToken token);

        int[] Classify(double[][] x);
    }
}