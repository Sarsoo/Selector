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
        private readonly IRefreshTokenFactoryProvider TokenFactoryProvider;

        private Dictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public WatcherService(IRefreshTokenFactoryProvider tokenFactoryProvider, ILogger<WatcherService> logger, IOptions<RootOptions> config)
        {
            Logger = logger;
            Config = config.Value;
            TokenFactoryProvider = tokenFactoryProvider;

            TokenFactoryProvider.Initialise(Config.ClientId, Config.ClientSecret);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting up");

            Config.WatcherOptions.Instances.ForEach(i => Logger.LogInformation($"Config: {i.Type}"));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Shutting down");

            foreach((var key, var watcher) in Watchers)
            {
                Logger.LogInformation($"Stopping watcher collection: {key}");
                watcher.Stop();
            }

            return Task.CompletedTask;
        }
    }
}
