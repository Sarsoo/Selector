using System;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Selector.Web.Hubs;
using Selector.Cache;

namespace Selector.Web.Service
{
    public class NowPlayingMapping : ICacheHubMapping<NowPlayingHub, INowPlayingHubClient>
    {
        private readonly string UserId;

        public NowPlayingMapping(ILogger<NowPlayingMapping> logger, string userId)
        {
            UserId = userId;
        }

        public async Task ConstructMapping(ISubscriber subscriber, IHubContext<NowPlayingHub, INowPlayingHubClient> hub)
        {
            (await subscriber.SubscribeAsync(Key.CurrentlyPlaying(UserId))).OnMessage(async message => {
                
                var deserialised = JsonSerializer.Deserialize<CurrentlyPlayingDTO>(message.ToString());
                await hub.Clients.User(UserId).OnNewPlaying(deserialised);
            });
        }
    }
}