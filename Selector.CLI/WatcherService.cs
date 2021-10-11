using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Selector.CLI
{
    class WatcherService : IHostedService
    {
        private readonly ILogger<WatcherService> Logger;
        private readonly RootOptions Config;

        public WatcherService(ILogger<WatcherService> logger, IOptions<RootOptions> config)
        {
            Logger = logger;
            Config = config.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting up");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Shutting down");
            return Task.CompletedTask;
        }
    }
}
