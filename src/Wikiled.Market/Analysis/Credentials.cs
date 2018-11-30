using System;
using Wikiled.Common.Utilities.Config;

namespace Wikiled.Market.Analysis
{
    public class Credentials
    {
        public Credentials(IApplicationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            QuandlKey = configuration.GetEnvironmentVariable("QUANDL_KEY");
        }

        public string QuandlKey { get; }
    }
}
