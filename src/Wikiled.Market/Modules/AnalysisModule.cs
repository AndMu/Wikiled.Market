using Autofac;
using Trady.Core.Infrastructure;
using Trady.Importer.Quandl;
using Wikiled.Market.Analysis;

namespace Wikiled.Market.Modules
{
    public class AnalysisModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AnalysisManager>().As<IAnalysisManager>();
            builder.RegisterType<DataSource>().As<IDataSource>();
            builder.RegisterType<ClassifierFactory>().As<IClassifierFactory>();
            builder.Register(ctx => new QuandlWikiImporter(ctx.Resolve<Credentials>().QuandlKey)).As<IImporter>();
            base.Load(builder);
        }
    }
}
