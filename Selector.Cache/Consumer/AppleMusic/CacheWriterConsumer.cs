using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Selector.AppleMusic;
using Selector.AppleMusic.Consumer;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class AppleCacheWriter(
        IAppleMusicPlayerWatcher watcher,
        IDatabaseAsync db,
        ILogger<AppleCacheWriter> logger = null,
        CancellationToken token = default)
        : BaseSequentialPlayerConsumer<IAppleMusicPlayerWatcher, AppleListeningChangeEventArgs>(watcher, logger),
            IApplePlayerConsumer
    {
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromMinutes(20);

        public CancellationToken CancelToken { get; set; } = token;

        protected override async Task ProcessEvent(AppleListeningChangeEventArgs e)
        {
            // using var scope = Logger.GetListeningEventArgsScope(e);

            var payload = JsonSerializer.Serialize((AppleCurrentlyPlayingDTO)e,
                AppleJsonContext.Default.AppleCurrentlyPlayingDTO);

            Logger.LogTrace("Caching current");

            var resp = await db.StringSetAsync(Key.CurrentlyPlayingAppleMusic(e.Id), payload, expiry: CacheExpiry);

            Logger.LogDebug("Cached current, {state}", (resp ? "value set" : "value NOT set"));
        }
    }
}