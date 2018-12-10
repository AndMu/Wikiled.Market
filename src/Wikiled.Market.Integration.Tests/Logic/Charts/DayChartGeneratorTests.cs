using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using Wikiled.Common.Utilities.Modules;
using Wikiled.Market.Console.Commands.Config;
using Wikiled.Market.Console.Logic;
using Wikiled.Market.Console.Logic.Charts;
using Wikiled.Sentiment.Tracking.Api.Request;
using Wikiled.Sentiment.Tracking.Api.Service;

namespace Wikiled.Market.Integration.Tests.Logic.Charts
{
    [TestFixture]
    public class DayChartGeneratorTests
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
            var chartGenerator = container.Resolve<IDayChartGenerator>(new TypedParameter(typeof(string), "Twitter"));
            var tracking = container.ResolveKeyed<ISentimentTracking>("Twitter");
            var result = await tracking.GetTrackingHistory(new SentimentRequest("$AMD", "$INTC", "$BTC", "$TSLA") {Hours = new[] { HoursExtension.GetLastDaysHours(5) } }, CancellationToken.None);
            var average1 = result["$TSLA"].GroupBy(item => item.Date.Date).Select(item => (item.Key, item.Where(y => y.Rating.HasValue).OrderBy(x => x.Date).Average(x => x.Rating.Value)));
            var average2 = result["$AMD"].GroupBy(item => item.Date.Date).Select(item => (item.Key, item.Where(y => y.Rating.HasValue).OrderBy(x => x.Date).Average(x => x.Rating.Value)));
            var average3 = result["$BTC"].GroupBy(item => item.Date.Date).Select(item => (item.Key, item.Where(y => y.Rating.HasValue).OrderBy(x => x.Date).Average(x => x.Rating.Value)));
            var average4 = result["$INTC"].GroupBy(item => item.Date.Date).Select(item => (item.Key, item.Where(y => y.Rating.HasValue).OrderBy(x => x.Date).Average(x => x.Rating.Value)));
            chartGenerator.AddSeriesByDay("AMD", result["$AMD"], 5);
            chartGenerator.AddSeriesByDay("$TSLA", result["$TSLA"], 5);
            chartGenerator.AddSeriesByDay("INTC", result["$INTC"], 5);
            chartGenerator.AddSeriesByDay("$BTC", result["$BTC"], 5);
            var chart = await chartGenerator.GenerateGraph().ConfigureAwait(false);
            await File.WriteAllBytesAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, @"chart.jpg"), chart, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
