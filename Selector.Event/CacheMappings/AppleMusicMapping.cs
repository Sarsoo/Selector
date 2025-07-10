using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Selector.Cache;
using Selector.Cache.Extensions;
using StackExchange.Redis;

namespace Selector.Events
{
    public class AppleMusicLinkChange
    {
        public string UserId { get; set; }
        public bool PreviousLinkState { get; set; }
        public bool NewLinkState { get; set; }
    }

    public partial class FromPubSub
    {
        public class AppleMusicLink : IEventMapping
        {
            private readonly ILogger<AppleMusicLink> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public AppleMusicLink(ILogger<AppleMusicLink> logger,
                ISubscriber subscriber,
                UserEventBus userEvent)
            {
                Logger = logger;
                Subscriber = subscriber;
                UserEvent = userEvent;
            }

            public async Task ConstructMapping()
            {
                Logger.LogDebug("Forming Apple Music link event mapping FROM cache TO event bus");

                (await Subscriber.SubscribeAsync(RedisChannel.Pattern(Key.AllUserAppleMusic))).OnMessage(message =>
                {
                    try
                    {
                        var userId = Key.Param(message.Channel);

                        using var msg =
                            message.DeserialiseBaggageWrapped(CacheJsonContext.Default.AppleMusicLinkChange);
                        var deserialised = msg.Object;
                        Activity.Current?.AddBaggage(TraceConst.UserId, userId);

                        Logger.LogDebug("Received new Apple Music link event for [{userId}]", deserialised.UserId);

                        if (!userId.Equals(deserialised.UserId))
                        {
                            Logger.LogWarning("Serialised user ID [{}] does not match cache channel [{}]", userId,
                                deserialised.UserId);
                        }

                        UserEvent.OnAppleMusicLinkChange(this, deserialised);
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
                        Logger.LogError(e, "Error parsing new Apple Music link event");
                    }
                });
            }
        }
    }

    public partial class ToPubSub
    {
        public class AppleMusicLink : IEventMapping
        {
            private readonly ILogger<AppleMusicLink> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public AppleMusicLink(ILogger<AppleMusicLink> logger,
                ISubscriber subscriber,
                UserEventBus userEvent)
            {
                Logger = logger;
                Subscriber = subscriber;
                UserEvent = userEvent;
            }

            public Task ConstructMapping()
            {
                Logger.LogDebug("Forming Apple Music link event mapping TO cache FROM event bus");

                UserEvent.AppleLinkChange += async (o, e) =>
                {
                    var payload = e.SerialiseBaggageWrapped(CacheJsonContext.Default.AppleMusicLinkChange);
                    await Subscriber.PublishAsync(RedisChannel.Literal(Key.UserAppleMusic(e.UserId)), payload);
                };

                return Task.CompletedTask;
            }
        }
    }
}