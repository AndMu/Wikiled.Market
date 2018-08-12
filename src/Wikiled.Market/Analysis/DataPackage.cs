using System;

namespace Wikiled.Market.Analysis
{
    public class DataPackage
    {
        public DataPackage(int[] y, double[][] x, double[][] prediction)
        {
            Y = y ?? throw new ArgumentNullException(nameof(y));
            X = x ?? throw new ArgumentNullException(nameof(x));
            Prediction = prediction ?? throw new ArgumentNullException(nameof(prediction));
        }

        public int[] Y { get; }

        public double[][] X { get; }

        public double[][] Prediction { get; }
    }
}
