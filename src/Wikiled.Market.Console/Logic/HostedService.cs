using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Wikiled.Console.Arguments;

namespace Wikiled.Market.Console.Logic
{
    public class HostedService : IHostedService
    {
        private readonly IAutoStarter autoStarter;

        public HostedService(IAutoStarter autoStarter)
        {
            this.autoStarter = autoStarter ?? throw new ArgumentNullException(nameof(autoStarter));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return autoStarter.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return autoStarter.Stop(cancellationToken);
        }
    }
}
