using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Selector.Web.Service
{
    public class CacheHubProxyService: IHostedService
    {
        private readonly ILogger<CacheHubProxyService> Logger;
        private readonly CacheHubProxy Proxy;
        private readonly IServiceScopeFactory ScopeFactory;

        public CacheHubProxyService(
            ILogger<CacheHubProxyService> logger,
            CacheHubProxy proxy,
            IServiceScopeFactory scopeFactory
        )
        {
            Logger = logger;
            Proxy = proxy;
            ScopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting cache hub proxy");

            using(var scope = ScopeFactory.CreateScope())
            {   
                foreach(var mapping in scope.ServiceProvider.GetServices<IUserMapping>())
                {
                    mapping.FormAll();
                }          
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