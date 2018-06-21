using Wikiled.Common.Utilities.Config;

namespace Wikiled.Market.Analysis
{
    public class Credentials
    {
        private IApplicationConfiguration configuration;

        public Credentials(IApplicationConfiguration configuration)
        {
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
            QuandlKey = configuration.GetEnvironmentVariable("QUANDL_KEY");
        }

        public string QuandlKey { get; }
    }
}
