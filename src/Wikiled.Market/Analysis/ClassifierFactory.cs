namespace Wikiled.Market.Analysis
{
    public class ClassifierFactory : IClassifierFactory
    {
        public IClassifier Construct()
        {
            return new Classifier();
        }
    }
}
