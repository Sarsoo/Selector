using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.Extensions;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class SpotifyPublisher : ISpotifyPlayerConsumer
    {
        private readonly ISpotifyPlayerWatcher Watcher;
        private readonly ISubscriber Subscriber;
        private readonly ILogger<SpotifyPublisher> Logger;

        public CancellationToken CancelToken { get; set; }

        public SpotifyPublisher(
            ISpotifyPlayerWatcher watcher,
            ISubscriber subscriber,
            ILogger<SpotifyPublisher> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            Subscriber = subscriber;
            Logger = logger ?? NullLogger<SpotifyPublisher>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            Task.Run(async () =>
            {
                try
                {
                    await AsyncCallback(e);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occured during callback");
                }
            }, CancelToken);
        }

        public async Task AsyncCallback(SpotifyListeningChangeEventArgs e)
        {
            using var scope = Logger.GetListeningEventArgsScope(e);

            var payload =
                JsonSerializer.Serialize((CurrentlyPlayingDTO)e, SpotifyJsonContext.Default.CurrentlyPlayingDTO);

            Logger.LogTrace("Publishing current");

            // TODO: currently using spotify username for cache key, use db username
            var receivers =
                await Subscriber.PublishAsync(RedisChannel.Literal(Key.CurrentlyPlayingSpotify(e.Id)), payload);

            Logger.LogDebug("Published current, {receivers} receivers", receivers);
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is ISpotifyPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange += Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }

        public void Unsubscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is ISpotifyPlayerWatcher watcherCast)
            {
                watcherCast.ItemChange -= Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }
    }
}