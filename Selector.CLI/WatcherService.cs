using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Selector.CLI
{
    class WatcherService : IHostedService
    {
        private readonly ILogger<WatcherService> Logger;
        private readonly IConfiguration Config;

        public WatcherService(ILogger<WatcherService> logger, IConfiguration config)
        {
            Logger = logger;
            Config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting up");

            foreach ((var key, var pair) in Config.AsEnumerable())
            //foreach ((var key, var pair) in Config.GetSection("Selector").AsEnumerable())
            {
                Logger.LogInformation($"{key} => {pair}");
            }

            using(Logger.BeginScope("A New Scope!"))
            {
                Logger.LogError("From the scope!");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Shutting down");
            return Task.CompletedTask;
        }
    }
}
