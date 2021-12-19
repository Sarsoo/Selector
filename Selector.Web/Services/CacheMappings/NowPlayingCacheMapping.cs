using System;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Selector.Web.Hubs;
using Selector.Cache;
using Selector.Model.Events;

namespace Selector.Web.Service
{
    public class NowPlayingCacheMapping : ICacheEventMapping
    {
        private readonly ILogger<NowPlayingCacheMapping> Logger;
        private readonly ISubscriber Subscriber;
        private readonly UserEventBus UserEvent;

        public NowPlayingCacheMapping(ILogger<NowPlayingCacheMapping> logger, 
            ISubscriber subscriber,
            UserEventBus userEvent)
        {
            Logger = logger;
            Subscriber = subscriber;
            UserEvent = userEvent;
        }

        public async Task ConstructMapping()
        {
            Logger.LogDebug("Forming now playing event mapping between cache and event bus");

            (await Subscriber.SubscribeAsync(Key.AllCurrentlyPlaying)).OnMessage(message => {
                
                try{
                    var userId = Key.Param(message.Channel);

                    var deserialised = JsonSerializer.Deserialize<CurrentlyPlayingDTO>(message.Message);
                    Logger.LogDebug("Received new currently playing [{username}]", deserialised.Username);

                    UserEvent.OnCurrentlyPlayingChange(this, userId, deserialised);
                }
                catch(Exception e)
                {
                    Logger.LogError(e, $"Error parsing new currently playing [{message}]");
                }
            });
        }
    }
}