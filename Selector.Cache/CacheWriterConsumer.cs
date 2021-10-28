using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SpotifyAPI.Web;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class CacheWriter : IConsumer
    {
        private readonly IPlayerWatcher Watcher;
        private readonly IDatabaseAsync Db;
        private readonly ILogger<CacheWriter> Logger;

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

            Task.Run(() => { return AsyncCallback(e); }, CancelToken);
        }

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            var payload = JsonSerializer.Serialize(e);
            await Db.StringSetAsync(Key.CurrentlyPlaying(e.Username), payload);
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
