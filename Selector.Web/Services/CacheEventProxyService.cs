using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Selector.Web.Service
{
    public class CacheEventProxyService: IHostedService
    {
        private readonly ILogger<CacheEventProxyService> Logger;
        private readonly IServiceScopeFactory ScopeFactory;

        private readonly IEnumerable<ICacheEventMapping> CacheEvents;

        public CacheEventProxyService(
            ILogger<CacheEventProxyService> logger,
            IEnumerable<ICacheEventMapping> mappings,
            IServiceScopeFactory scopeFactory
        )
        {
            Logger = logger;
            ScopeFactory = scopeFactory;

            CacheEvents = mappings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting cache event proxy");

            foreach (var mapping in CacheEvents)
            {
                mapping.ConstructMapping();
            }

            using (var scope = ScopeFactory.CreateScope())
            {
                var hubProxy = scope.ServiceProvider.GetRequiredService<EventHubProxy>();
                hubProxy.FormMappings();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Stopping cache hub proxy");
            return Task.CompletedTask;
        }
    }
}