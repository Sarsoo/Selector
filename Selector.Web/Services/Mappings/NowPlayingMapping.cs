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
        private readonly ILogger<NowPlayingMapping> Logger;
        private readonly string UserId;
        private readonly string Username;

        public NowPlayingMapping(ILogger<NowPlayingMapping> logger, string userId, string username)
        {
            Logger = logger;
            UserId = userId;
            Username = username;
        }

        public async Task ConstructMapping(ISubscriber subscriber, IHubContext<NowPlayingHub, INowPlayingHubClient> hub)
        {
            var key = Key.CurrentlyPlaying(Username);
            (await subscriber.SubscribeAsync(key)).OnMessage(async message => {
                
                try{
                    var trimmedMessage = message.ToString().Substring(key.Length + 1);
                    var deserialised = JsonSerializer.Deserialize<CurrentlyPlayingDTO>(trimmedMessage);
                    Logger.LogDebug($"Received new currently playing [{deserialised.Username}] [{deserialised.Username}]");
                    await hub.Clients.User(UserId).OnNewPlaying(deserialised);
                }
                catch(Exception e)
                {
                    Logger.LogError(e, $"Error parsing new currently playing [{message}]");
                }
            });
        }
    }
}