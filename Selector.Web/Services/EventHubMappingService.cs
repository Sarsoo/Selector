using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Selector.Events;

namespace Selector.Web.Service
{
    public class EventHubMappingService: IHostedService
    {
        private readonly ILogger<EventHubMappingService> Logger;
        private readonly IServiceScopeFactory ScopeFactory;

        public EventHubMappingService(
            ILogger<EventHubMappingService> logger,
            IServiceScopeFactory scopeFactory
        )
        {
            Logger = logger;
            ScopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting hub event mapping service");

            using (var scope = ScopeFactory.CreateScope())
            {
                var hubProxy = scope.ServiceProvider.GetRequiredService<EventHubProxy>();
                hubProxy.FormMappings();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}