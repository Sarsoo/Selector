using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Selector.Web.Service
{
    public class CacheHubProxyService: IHostedService
    {
        private readonly ILogger<CacheHubProxyService> Logger;
        private readonly CacheHubProxy Proxy;

        public CacheHubProxyService(
            ILogger<CacheHubProxyService> logger,
            CacheHubProxy proxy
        )
        {
            Logger = logger;
            Proxy = proxy;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting cache hub proxy");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping cache hub proxy");
            return Task.CompletedTask;
        }
    }
}