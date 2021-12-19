using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using Selector.Web.Hubs;
using Selector.Events;

namespace Selector.Web.Service
{
    public class NowPlayingHubMapping: IEventHubMapping<NowPlayingHub, INowPlayingHubClient>
    {
        private readonly ILogger<NowPlayingHubMapping> Logger;
        private readonly UserEventBus UserEvent;
        private readonly IHubContext<NowPlayingHub, INowPlayingHubClient> Hub;

        public NowPlayingHubMapping(ILogger<NowPlayingHubMapping> logger,
            UserEventBus userEvent,
            IHubContext<NowPlayingHub, INowPlayingHubClient> hub)
        {
            Logger = logger;
            UserEvent = userEvent;
            Hub = hub;
        }

        public Task ConstructMapping()
        {
            Logger.LogDebug("Forming now playing event mapping between event bus and SignalR hub");

            UserEvent.CurrentlyPlaying += async (o, args) =>
            {
                (string id, CurrentlyPlayingDTO e) = args;
                Logger.LogDebug("Passing now playing event to SignalR hub [{userId}]", id);

                await Hub.Clients.User(id).OnNewPlaying(e);
            };

            return Task.CompletedTask;
        }
    }
}
