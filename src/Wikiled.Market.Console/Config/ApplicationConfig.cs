using Wikiled.Market.Analysis;

namespace Wikiled.Market.Console.Config
{
    public class ApplicationConfig
    {
        public SentimentConfig Sentiment { get; set; }

        public string[] Stocks { get; set; }
    }
}
