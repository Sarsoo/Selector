using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Selector.AppleMusic;
using Selector.Cache;
using Selector.Spotify;
using StackExchange.Redis;
using Trace = Selector.Event.Trace;

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

                (await Subscriber.SubscribeAsync(RedisChannel.Pattern(Key.AllCurrentlyPlayingSpotify)))
                    .OnMessage(message =>
                    {
                        using var span = Trace.Tracer.StartActivity();
                        try
                        {
                            var userId = Key.Param(message.Channel);
                            span?.AddBaggage(TraceConst.Username, userId);

                            var deserialised =
                                JsonSerializer.Deserialize(message.Message,
                                    SpotifyJsonContext.Default.SpotifyCurrentlyPlayingDTO);
                            Logger.LogDebug("Received new Spotify currently playing [{username}]",
                                deserialised.Username);

                            UserEvent.OnCurrentlyPlayingChangeSpotify(this, deserialised);
                            span?.SetStatus(ActivityStatusCode.Ok);
                        }
                        catch (Exception e)
                        {
                            span?.AddException(e);
                            Logger.LogError(e, "Error parsing new Spotify currently playing [{message}]", message);
                        }
                    });

                (await Subscriber.SubscribeAsync(RedisChannel.Pattern(Key.AllCurrentlyPlayingApple)))
                    .OnMessage(message =>
                    {
                        using var span = Trace.Tracer.StartActivity();
                        try
                        {
                            var userId = Key.Param(message.Channel);
                            span?.AddBaggage(TraceConst.Username, userId);

                            var deserialised = JsonSerializer.Deserialize(message.Message,
                                AppleJsonContext.Default.AppleCurrentlyPlayingDTO);
                            Logger.LogDebug("Received new Apple Music currently playing");

                            UserEvent.OnCurrentlyPlayingChangeApple(this, deserialised);
                            span?.SetStatus(ActivityStatusCode.Ok);
                        }
                        catch (Exception e)
                        {
                            span?.AddException(e);
                            Logger.LogError(e, "Error parsing new Apple Music currently playing [{message}]", message);
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

                UserEvent.CurrentlyPlayingSpotify += async (o, e) =>
                {
                    var payload = JsonSerializer.Serialize(e, SpotifyJsonContext.Default.SpotifyCurrentlyPlayingDTO);
                    await Subscriber.PublishAsync(RedisChannel.Literal(Key.CurrentlyPlayingSpotify(e.UserId)), payload);
                };

                UserEvent.CurrentlyPlayingApple += async (o, e) =>
                {
                    var payload = JsonSerializer.Serialize(e, AppleJsonContext.Default.AppleCurrentlyPlayingDTO);
                    await Subscriber.PublishAsync(RedisChannel.Literal(Key.CurrentlyPlayingAppleMusic(e.UserId)),
                        payload);
                };

                return Task.CompletedTask;
            }
        }
    }
}