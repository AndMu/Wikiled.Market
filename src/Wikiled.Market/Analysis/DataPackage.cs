using Wikiled.Common.Arguments;

namespace Wikiled.Market.Analysis
{
    public class DataPackage
    {
        public DataPackage(int[] y, double[][] x, double[][] prediction)
        {
            Guard.NotNull(() => y, y);
            Guard.NotNull(() => x, x);
            Guard.NotNull(() => prediction, prediction);
            Y = y;
            X = x;
            Prediction = prediction;
        }

        public int[] Y { get; }

        public double[][] X { get; }

        public double[][] Prediction { get; }
    }
}
