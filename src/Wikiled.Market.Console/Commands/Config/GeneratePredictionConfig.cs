using System;
using System.ComponentModel;
using Autofac;
using Wikiled.Common.Utilities.Modules;
using Wikiled.Console.Arguments;
using Wikiled.Market.Analysis;
using Wikiled.Market.Modules;

namespace Wikiled.Market.Console.Commands.Config
{
    public class GeneratePredictionConfig : ICommandConfig
    {
        [Description("For what stocks generate prediction")]
        public string Stocks { get; set; }

        public void Build(ContainerBuilder builder)
        {
            builder.RegisterModule(new CommonModule());
            builder.RegisterModule<AnalysisModule>();
            builder.RegisterType<Credentials>();
        }
    }
}
