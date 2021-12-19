using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace Selector.Events
{
    public class EventMappingService: IHostedService
    {
        private readonly ILogger<EventMappingService> Logger;
        private readonly IServiceScopeFactory ScopeFactory;

        private readonly IEnumerable<IEventMapping> CacheEvents;

        public EventMappingService(
            ILogger<EventMappingService> logger,
            IEnumerable<IEventMapping> mappings,
            IServiceScopeFactory scopeFactory
        )
        {
            Logger = logger;
            ScopeFactory = scopeFactory;

            CacheEvents = mappings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting event mapping service");

            foreach (var mapping in CacheEvents)
            {
                mapping.ConstructMapping();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}