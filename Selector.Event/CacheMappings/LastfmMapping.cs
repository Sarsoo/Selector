using System.Text.Json;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Selector.Cache;

namespace Selector.Events
{
    public class LastfmChange
    {
        public string UserId { get; set; }
        public string PreviousUsername { get; set; }
        public string NewUsername { get; set; }
    }

    public partial class FromPubSub
    {
        public class Lastfm : IEventMapping
        {
            private readonly ILogger<Lastfm> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public Lastfm(ILogger<Lastfm> logger,
                ISubscriber subscriber,
                UserEventBus userEvent)
            {
                Logger = logger;
                Subscriber = subscriber;
                UserEvent = userEvent;
            }

            public async Task ConstructMapping()
            {
                Logger.LogDebug("Forming Last.fm username event mapping FROM cache TO event bus");

                (await Subscriber.SubscribeAsync(Key.AllUserLastfm)).OnMessage(message => {

                    try
                    {
                        var userId = Key.Param(message.Channel);

                        var deserialised = JsonSerializer.Deserialize(message.Message, CacheJsonContext.Default.LastfmChange);
                        Logger.LogDebug("Received new Last.fm username event for [{userId}]", deserialised.UserId);

                        if (!userId.Equals(deserialised.UserId))
                        {
                            Logger.LogWarning("Serialised user ID [{}] does not match cache channel [{}]", userId, deserialised.UserId);
                        }

                        UserEvent.OnLastfmCredChange(this, deserialised);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Error parsing Last.fm username event");
                    }
                });
            }
        }
    }

    public partial class ToPubSub
    {
        public class Lastfm : IEventMapping
        {
            private readonly ILogger<Lastfm> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public Lastfm(ILogger<Lastfm> logger,
                ISubscriber subscriber,
                UserEventBus userEvent)
            {
                Logger = logger;
                Subscriber = subscriber;
                UserEvent = userEvent;
            }

            public Task ConstructMapping()
            {
                Logger.LogDebug("Forming Last.fm username event mapping TO cache FROM event bus");

                UserEvent.LastfmCredChange += async (o, e) =>
                {
                    var payload = JsonSerializer.Serialize(e, CacheJsonContext.Default.LastfmChange);
                    await Subscriber.PublishAsync(Key.UserLastfm(e.UserId), payload);
                };

                return Task.CompletedTask;
            }
        }
    }
}