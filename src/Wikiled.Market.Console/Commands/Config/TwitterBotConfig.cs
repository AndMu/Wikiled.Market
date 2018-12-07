using Autofac;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Wikiled.Common.Net.Client;
using Wikiled.Common.Utilities.Modules;
using Wikiled.Console.Arguments;
using Wikiled.Market.Analysis;
using Wikiled.Market.Console.Config;
using Wikiled.Market.Console.Logic;
using Wikiled.Market.Console.Logic.Charts;
using Wikiled.Market.Modules;
using Wikiled.Sentiment.Tracking.Api.Service;
using Wikiled.Twitter.Modules;
using Wikiled.Twitter.Security;

namespace Wikiled.Market.Console.Commands.Config
{
    public class TwitterBotConfig : ICommandConfig
    {
        [Description("For which stocks generate prediction")]
        public string Stocks { get; set; }

        public bool IsService { get; set; }

        public bool IsDev { get; set; }

        public ApplicationConfig ApplicationConfig { get; private set; }

        public void Build(ContainerBuilder builder)
        {
            ApplicationConfig = LoadConfig(IsDev ? "service.dev.json" : "service.json");
            builder.RegisterModule(new CommonModule());
            builder.RegisterModule<AnalysisModule>();
            builder.RegisterModule<TwitterModule>();
            builder.RegisterType<Credentials>();
            builder.RegisterType<TwitterChartGeneration>().As<ISentimentChartGeneration>();
            builder.RegisterType<DayChartGenerator>().As<IDayChartGenerator>();

            builder.RegisterType<ConfigurationValidator>().AsSelf().AutoActivate();

            builder.RegisterType<SentimentMonitor>().As<ISentimentMonitor>();
            builder.RegisterType<MarketMonitor>().As<IMarketMonitor>();

            if (IsService)
            {
                builder.RegisterType<EnvironmentAuthentication>().As<IAuthentication>();
            }
            else
            {
                builder.RegisterModule(new ConsoleAuthModule());
            }

            builder.Register(ctx => new SentimentTracking(ctx.ResolveNamed<IApiClientFactory>("Twitter"))).Keyed<ISentimentTracking>("Twitter");
            builder.Register(ctx => new ApiClientFactory(
                                 new HttpClient { Timeout = TimeSpan.FromMinutes(5) },
                                 new Uri(ApplicationConfig.Sentiment.Service)))
                .Named<IApiClientFactory>("Twitter");

            builder.Register(ctx => new SentimentTracking(ctx.ResolveNamed<IApiClientFactory>("Seeking"))).Keyed<ISentimentTracking>("Seeking");
            builder.Register(ctx => new ApiClientFactory(
                                 new HttpClient { Timeout = TimeSpan.FromMinutes(5) },
                                 new Uri(ApplicationConfig.Sentiment.Alpha)))
                .Named<IApiClientFactory>("Seeking");
        }

        private ApplicationConfig LoadConfig(string file)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            file = Path.Combine(directory, file);
            if (!File.Exists(file))
            {
                throw new Exception("Configuration file service.json not found");

            }

            return JsonConvert.DeserializeObject<ApplicationConfig>(File.ReadAllText(file));
        }
    }
}
