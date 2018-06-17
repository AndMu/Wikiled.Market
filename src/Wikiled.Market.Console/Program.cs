using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wikiled.Console.Arguments;
using Wikiled.Market.Console.Logic;

namespace Wikiled.Market.Console
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IAutoStarter>(serviceProvider => new AutoStarter("Market Utility", args));
                        services.AddScoped<IHostedService, HostedService>();
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
