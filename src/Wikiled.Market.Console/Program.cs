using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using Wikiled.Console.Arguments;
using Wikiled.Market.Console.Commands;
using Wikiled.Market.Console.Commands.Config;

namespace Wikiled.Market.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var starter = new AutoStarter("Market Utility", args);
            starter.Factory.AddNLog(new NLogProviderOptions {CaptureMessageProperties = true, IncludeScopes = true});
            starter.RegisterCommand<TwitterBotCommand, TwitterBotConfig>("bot");
            starter.RegisterCommand<GeneratePredictionCommand, GeneratePredictionConfig>("Generate");

            var hostBuilder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IHostedService>(serviceProvider => starter);
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
