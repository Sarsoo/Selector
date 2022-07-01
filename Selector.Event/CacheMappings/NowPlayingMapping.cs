using System.Text.Json;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Selector.Cache;

namespace Selector.Events
{
    public partial class FromPubSub
    {
        public class NowPlaying : IEventMapping
        {
            private readonly ILogger<NowPlaying> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public NowPlaying(ILogger<NowPlaying> logger,
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

                    try
                    {
                        var userId = Key.Param(message.Channel);

                        var deserialised = JsonSerializer.Deserialize(message.Message, JsonContext.Default.CurrentlyPlayingDTO);
                        Logger.LogDebug("Received new currently playing [{username}]", deserialised.Username);

                        UserEvent.OnCurrentlyPlayingChange(this, deserialised);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Error parsing new currently playing [{message}]", message);
                    }
                });
            }
        }
    }

    public partial class ToPubSub
    {
        public class NowPlaying : IEventMapping
        {
            private readonly ILogger<NowPlaying> Logger;
            private readonly ISubscriber Subscriber;
            private readonly UserEventBus UserEvent;

            public NowPlaying(ILogger<NowPlaying> logger,
                ISubscriber subscriber,
                UserEventBus userEvent)
            {
                Logger = logger;
                Subscriber = subscriber;
                UserEvent = userEvent;
            }

            public Task ConstructMapping()
            {
                Logger.LogDebug("Forming now playing event mapping TO cache FROM event bus");

                UserEvent.CurrentlyPlaying += async (o, e) =>
                {
                    var payload = JsonSerializer.Serialize(e, JsonContext.Default.CurrentlyPlayingDTO);
                    await Subscriber.PublishAsync(Key.CurrentlyPlaying(e.UserId), payload);
                };

                return Task.CompletedTask;
            }
        }
    }
}