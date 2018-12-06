using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Wikiled.Common.Utilities.Modules;
using Wikiled.Market.Console.Commands.Config;
using Wikiled.Market.Console.Logic.Charts;

namespace Wikiled.Market.Integration.Tests.Logic.Charts
{
    [TestFixture]
    public class SentimentChartGenerationTests
    {
        [Test]
        public async Task Generate()
        {
            ContainerBuilder builder = new ContainerBuilder();
            TwitterBotConfig config = new TwitterBotConfig();
            config.IsDev = true;
            config.IsService = true;
            builder.RegisterModule(new LoggingModule(new NullLoggerFactory()));
            config.Build(builder);
            var container = builder.Build();
            var generation = container.Resolve<ISentimentChartGeneration>();
            await generation.AddStock("AMD");
            await generation.AddStock("TSLA");
            await generation.AddStock("AMZN");
            await generation.AddStock("FB");
            var chart = await generation.Generate().ConfigureAwait(false);
            await File.WriteAllBytesAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, @"chart.jpg"), chart, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
