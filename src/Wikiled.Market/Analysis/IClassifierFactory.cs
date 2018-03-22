namespace Wikiled.Market.Analysis
{
    public interface IClassifierFactory
    {
        IClassifier Construct();
    }
}