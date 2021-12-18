using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using StackExchange.Redis;

namespace Selector.Cache
{
    public class CacheWriter : IConsumer
    {
        private readonly IPlayerWatcher Watcher;
        private readonly IDatabaseAsync Db;
        private readonly ILogger<CacheWriter> Logger;
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromMinutes(20);

        public CancellationToken CancelToken { get; set; }

        public CacheWriter(
            IPlayerWatcher watcher,
            IDatabaseAsync db,
            ILogger<CacheWriter> logger = null,
            CancellationToken token = default
        ){
            Watcher = watcher;
            Db = db;
            Logger = logger ?? NullLogger<CacheWriter>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, ListeningChangeEventArgs e)
        {
            if (e.Current is null) return;
            
            Task.Run(async () => {
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
            var payload = JsonSerializer.Serialize((CurrentlyPlayingDTO) e);
            
            Logger.LogTrace($"Caching current for [{e.Id}/{e.SpotifyUsername}]");

            var resp = await Db.StringSetAsync(Key.CurrentlyPlaying(e.Id), payload, expiry: CacheExpiry);

            Logger.LogDebug($"Cached current for [{e.Id}/{e.SpotifyUsername}], {(resp ? "value set" : "value NOT set")}");

        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IPlayerWatcher watcherCast)
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

            if (watcher is IPlayerWatcher watcherCast)
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
