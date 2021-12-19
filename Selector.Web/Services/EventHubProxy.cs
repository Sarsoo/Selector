using Microsoft.Extensions.Logging;

namespace Selector.Web.Service
{
    public class EventHubProxy
    {
        private readonly ILogger<EventHubProxy> Logger;

        private readonly NowPlayingHubMapping NowPlayingMapping;

        public EventHubProxy(ILogger<EventHubProxy> logger,
            NowPlayingHubMapping nowPlayingMapping
        )
        {
            Logger = logger;
            NowPlayingMapping = nowPlayingMapping;
        }

        public void FormMappings()
        {
            Logger.LogDebug("Forming event mappings between event bus and SignalR hubs");

            NowPlayingMapping.ConstructMapping();
        }
    }
}