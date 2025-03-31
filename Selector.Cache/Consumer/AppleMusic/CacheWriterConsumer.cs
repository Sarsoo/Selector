using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.AppleMusic;
using Selector.AppleMusic.Consumer;
using StackExchange.Redis;

namespace Selector.Cache
{
    public class AppleCacheWriter : IApplePlayerConsumer
    {
        private readonly IAppleMusicPlayerWatcher Watcher;
        private readonly IDatabaseAsync Db;
        private readonly ILogger<AppleCacheWriter> Logger;
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromMinutes(20);

        public CancellationToken CancelToken { get; set; }

        public AppleCacheWriter(
            IAppleMusicPlayerWatcher watcher,
            IDatabaseAsync db,
            ILogger<AppleCacheWriter> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            Db = db;
            Logger = logger ?? NullLogger<AppleCacheWriter>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, AppleListeningChangeEventArgs e)
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

        public async Task AsyncCallback(AppleListeningChangeEventArgs e)
        {
            // using var scope = Logger.GetListeningEventArgsScope(e);

            var payload = JsonSerializer.Serialize(e, AppleJsonContext.Default.AppleListeningChangeEventArgs);

            Logger.LogTrace("Caching current");

            var resp = await Db.StringSetAsync(Key.CurrentlyPlayingAppleMusic(e.Id), payload, expiry: CacheExpiry);

            Logger.LogDebug("Cached current, {state}", (resp ? "value set" : "value NOT set"));
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IAppleMusicPlayerWatcher watcherCastApple)
            {
                watcherCastApple.ItemChange += Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }

        public void Unsubscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IAppleMusicPlayerWatcher watcherCastApple)
            {
                watcherCastApple.ItemChange -= Callback;
            }
            else
            {
                throw new ArgumentException("Provided watcher is not a PlayerWatcher");
            }
        }
    }
}