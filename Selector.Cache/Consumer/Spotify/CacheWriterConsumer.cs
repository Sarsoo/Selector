using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class CacheWriter : ISpotifyPlayerConsumer
    {
        private readonly ISpotifyPlayerWatcher Watcher;
        private readonly IDatabaseAsync Db;
        private readonly ILogger<CacheWriter> Logger;
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromMinutes(20);

        public CancellationToken CancelToken { get; set; }

        public CacheWriter(
            ISpotifyPlayerWatcher watcher,
            IDatabaseAsync db,
            ILogger<CacheWriter> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            Db = db;
            Logger = logger ?? NullLogger<CacheWriter>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, ListeningChangeEventArgs e)
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

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            using var scope = Logger.GetListeningEventArgsScope(e);

            var payload = JsonSerializer.Serialize((CurrentlyPlayingDTO)e, JsonContext.Default.CurrentlyPlayingDTO);

            Logger.LogTrace("Caching current");

            var resp = await Db.StringSetAsync(Key.CurrentlyPlayingSpotify(e.Id), payload, expiry: CacheExpiry);

            Logger.LogDebug("Cached current, {state}", (resp ? "value set" : "value NOT set"));
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