using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.Extensions;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class SpotifyCacheWriter(
        ISpotifyPlayerWatcher watcher,
        IDatabaseAsync db,
        ILogger<SpotifyCacheWriter> logger = null,
        CancellationToken token = default)
        : BaseParallelPlayerConsumer<ISpotifyPlayerWatcher, SpotifyListeningChangeEventArgs>(watcher, logger),
            ISpotifyPlayerConsumer
    {
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromMinutes(20);

        public CancellationToken CancelToken { get; set; } = token;

        protected override async Task ProcessEvent(SpotifyListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            using var scope = Logger.GetListeningEventArgsScope(e);

            var payload =
                JsonSerializer.Serialize((SpotifyCurrentlyPlayingDTO)e,
                    SpotifyJsonContext.Default.SpotifyCurrentlyPlayingDTO);

            Logger.LogTrace("Caching current");

            var resp = await db.StringSetAsync(Key.CurrentlyPlayingSpotify(e.Id), payload, expiry: CacheExpiry);

            Logger.LogDebug("Cached current, {state}", (resp ? "value set" : "value NOT set"));
        }
    }
}