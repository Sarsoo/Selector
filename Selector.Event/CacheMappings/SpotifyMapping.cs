using System.Text.Json;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Selector.Cache;

namespace Selector.Events
{
    public class SpotifyLinkChange
    {
        public string UserId { get; set; }
        public bool PreviousLinkState { get; set; }
        public bool NewLinkState { get; set; }
    }

    public class SpotifyLinkFromCacheMapping : IEventMapping
    {
        private readonly ILogger<SpotifyLinkFromCacheMapping> Logger;
        private readonly ISubscriber Subscriber;
        private readonly UserEventBus UserEvent;

        public SpotifyLinkFromCacheMapping(ILogger<SpotifyLinkFromCacheMapping> logger, 
            ISubscriber subscriber,
            UserEventBus userEvent)
        {
            Logger = logger;
            Subscriber = subscriber;
            UserEvent = userEvent;
        }

        public async Task ConstructMapping()
        {
            Logger.LogDebug("Forming Spotify link event mapping FROM cache TO event bus");

            (await Subscriber.SubscribeAsync(Key.AllUserSpotify)).OnMessage(message => {
                
                try{
                    var userId = Key.Param(message.Channel);

                    var deserialised = JsonSerializer.Deserialize<SpotifyLinkChange>(message.Message);
                    Logger.LogDebug("Received new Spotify link event for [{userId}]", deserialised.UserId);

                    if (!userId.Equals(deserialised.UserId))
                    {
                        Logger.LogWarning("Serialised user ID [{}] does not match cache channel [{}]", userId, deserialised.UserId);
                    }

                    UserEvent.OnSpotifyLinkChange(this, deserialised);
                }
                catch(TaskCanceledException)
                {
                    Logger.LogDebug("Task Cancelled");
                }
                catch(Exception e)
                {
                    Logger.LogError(e, "Error parsing new Spotify link event");
                }
            });
        }
    }

    public class SpotifyLinkToCacheMapping : IEventMapping
    {
        private readonly ILogger<SpotifyLinkToCacheMapping> Logger;
        private readonly ISubscriber Subscriber;
        private readonly UserEventBus UserEvent;

        public SpotifyLinkToCacheMapping(ILogger<SpotifyLinkToCacheMapping> logger,
            ISubscriber subscriber,
            UserEventBus userEvent)
        {
            Logger = logger;
            Subscriber = subscriber;
            UserEvent = userEvent;
        }

        public Task ConstructMapping()
        {
            Logger.LogDebug("Forming Spotify link event mapping TO cache FROM event bus");

            UserEvent.SpotifyLinkChange += async (o, e) =>
            {
                var payload = JsonSerializer.Serialize(e);
                await Subscriber.PublishAsync(Key.UserSpotify(e.UserId), payload);
            };

            return Task.CompletedTask;
        }
    }
}