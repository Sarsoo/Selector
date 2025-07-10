using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.Cache.Extensions;
using Selector.Extensions;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class SpotifyPublisher(
        ISpotifyPlayerWatcher watcher,
        ISubscriber subscriber,
        ILogger<SpotifyPublisher> logger = null,
        CancellationToken token = default)
        : BaseParallelPlayerConsumer<ISpotifyPlayerWatcher, SpotifyListeningChangeEventArgs>(watcher, logger),
            ISpotifyPlayerConsumer
    {
        public CancellationToken CancelToken { get; set; } = token;

        protected override async Task ProcessEvent(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            using var scope = Logger.GetListeningEventArgsScope(e);

            var payload =
                ((SpotifyCurrentlyPlayingDTO)e).SerialiseBaggageWrapped(SpotifyJsonContext.Default
                    .SpotifyCurrentlyPlayingDTO);

            Logger.LogTrace("Publishing current");

            // TODO: currently using spotify username for cache key, use db username
            var receivers =
                await subscriber.PublishAsync(RedisChannel.Literal(Key.CurrentlyPlayingSpotify(e.Id)), payload);

            Logger.LogDebug("Published current, {receivers} receivers", receivers);
        }
    }
}