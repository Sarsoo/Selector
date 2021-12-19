using System.Text.Json;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Selector.Cache;

namespace Selector.Events
{
    public class NowPlayingFromCacheMapping : IEventMapping
    {
        private readonly ILogger<NowPlayingFromCacheMapping> Logger;
        private readonly ISubscriber Subscriber;
        private readonly UserEventBus UserEvent;

        public NowPlayingFromCacheMapping(ILogger<NowPlayingFromCacheMapping> logger, 
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