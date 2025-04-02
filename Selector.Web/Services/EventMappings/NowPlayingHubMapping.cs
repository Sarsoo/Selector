using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Selector.Events;
using Selector.SignalR;
using Selector.Web.Hubs;

namespace Selector.Web.Service
{
    public class NowPlayingHubMapping : IEventHubMapping<NowPlayingHub, INowPlayingHubClient>
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

            UserEvent.CurrentlyPlayingSpotify += async (o, args) =>
            {
                Logger.LogDebug("Passing Spotify now playing event to SignalR hub [{userId}]", args.UserId);

                await Hub.Clients.User(args.UserId).OnNewPlayingSpotify(args);
            };

            UserEvent.CurrentlyPlayingApple += async (o, args) =>
            {
                Logger.LogDebug("Passing Apple now playing event to SignalR hub {userId}]", args.UserId);

                await Hub.Clients.User(args.UserId).OnNewPlayingApple(args);
            };

            return Task.CompletedTask;
        }
    }
}