using System.Threading.Tasks;
using Wikiled.Console.Arguments;

namespace Wikiled.Market.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            AutoStarter starter = new AutoStarter("Market Utility");
            await starter.Start(args);
        }
    }
}
