namespace Wikiled.Market.Sentiment
{
    public interface IStockTracker
    {
        string Stock { get; }
        int TotalMessages { get; }
        string Twitter { get; }

        void AddRating(double? rating);
        double? AverageSentiment(int lastHours = 24);
        int TotalWithSentiment(int lastHours = 24);
    }
}