using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Selector.Cache;
using StackExchange.Redis;
using Trace = Selector.Event.Trace;

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
                    using var span = Trace.Tracer.StartActivity();
                    try
                    {
                        var userId = Key.Param(message.Channel);
                        span?.AddBaggage(TraceConst.Username, userId);

                        var deserialised = JsonSerializer.Deserialize(message.Message,
                            CacheJsonContext.Default.AppleMusicLinkChange);
                        Logger.LogDebug("Received new Apple Music link event for [{userId}]", deserialised.UserId);

                        if (!userId.Equals(deserialised.UserId))
                        {
                            Logger.LogWarning("Serialised user ID [{}] does not match cache channel [{}]", userId,
                                deserialised.UserId);
                        }

                        UserEvent.OnAppleMusicLinkChange(this, deserialised);
                        span?.SetStatus(ActivityStatusCode.Ok);
                    }
                    catch (TaskCanceledException e)
                    {
                        span?.AddException(e);
                        Logger.LogDebug("Task Cancelled");
                    }
                    catch (Exception e)
                    {
                        span?.AddException(e);
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
                    var payload = JsonSerializer.Serialize(e, CacheJsonContext.Default.AppleMusicLinkChange);
                    await Subscriber.PublishAsync(RedisChannel.Literal(Key.UserAppleMusic(e.UserId)), payload);
                };

                return Task.CompletedTask;
            }
        }
    }
}