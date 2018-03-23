using Wikiled.Console.Arguments;

namespace Wikiled.Market.Console
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            AutoStarter starter = new AutoStarter("Market Utility");
            starter.Start(args);
        }
    }
}
