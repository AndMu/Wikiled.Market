using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using System.Threading.Tasks;
using Wikiled.Common.Logging;
using Wikiled.Console.Arguments;
using Wikiled.Market.Console.Commands;
using Wikiled.Market.Console.Commands.Config;

namespace Wikiled.Market.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("NLog.config");
            var starter = new AutoStarter(ApplicationLogging.LoggerFactory, "Market Utility", args);
            starter.LoggerFactory.AddNLog();
            starter.RegisterCommand<TwitterBotCommand, TwitterBotConfig>("bot");
            starter.RegisterCommand<GeneratePredictionCommand, GeneratePredictionConfig>("generate");

            var hostBuilder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IHostedService>(serviceProvider => starter);
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
