using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Selector.Cache;
using Selector.Cache.Extensions;
using StackExchange.Redis;

namespace Selector.Events
{
    public class SpotifyLinkChange
    {
        public string UserId { get; set; }
        public bool PreviousLinkState { get; set; }
        public bool NewLinkState { get; set; }
    }

    public partial class FromPubSub
    {
        public class SpotifyLink : IEventMapping
        {
            private readonly ILogger<SpotifyLink> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public SpotifyLink(ILogger<SpotifyLink> logger,
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

                (await Subscriber.SubscribeAsync(RedisChannel.Pattern(Key.AllUserSpotify))).OnMessage(message =>
                {
                    try
                    {
                        var userId = Key.Param(message.Channel);

                        using var msg = message.DeserialiseBaggageWrapped(CacheJsonContext.Default.SpotifyLinkChange);
                        var deserialised = msg.Object;
                        Activity.Current?.AddBaggage(TraceConst.UserId, userId);

                        Logger.LogDebug("Received new Spotify link event for [{userId}]", deserialised.UserId);

                        if (!userId.Equals(deserialised.UserId))
                        {
                            Logger.LogWarning("Serialised user ID [{}] does not match cache channel [{}]", userId,
                                deserialised.UserId);
                        }

                        UserEvent.OnSpotifyLinkChange(this, deserialised);
                        Activity.Current?.SetStatus(ActivityStatusCode.Ok);
                    }
                    catch (TaskCanceledException e)
                    {
                        Activity.Current?.AddException(e);
                        Logger.LogDebug("Task Cancelled");
                    }
                    catch (Exception e)
                    {
                        Activity.Current?.AddException(e);
                        Logger.LogError(e, "Error parsing new Spotify link event");
                    }
                });
            }
        }
    }

    public partial class ToPubSub
    {
        public class SpotifyLink : IEventMapping
        {
            private readonly ILogger<SpotifyLink> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public SpotifyLink(ILogger<SpotifyLink> logger,
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
                    var payload = e.SerialiseBaggageWrapped(CacheJsonContext.Default.SpotifyLinkChange);
                    await Subscriber.PublishAsync(RedisChannel.Literal(Key.UserSpotify(e.UserId)), payload);
                };

                return Task.CompletedTask;
            }
        }
    }
}