using System.Threading;

namespace Wikiled.Market.Analysis
{
    public interface IClassifier
    {
        void Train(DataPackage data, CancellationToken token);

        int[] Classify(double[][] x);
    }
}