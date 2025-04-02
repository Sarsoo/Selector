using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Selector.AppleMusic;
using Selector.AppleMusic.Consumer;
using StackExchange.Redis;

namespace Selector.Cache.Consumer.AppleMusic
{
    public class ApplePublisher : IApplePlayerConsumer
    {
        private readonly IAppleMusicPlayerWatcher Watcher;
        private readonly ISubscriber Subscriber;
        private readonly ILogger<ApplePublisher> Logger;

        public CancellationToken CancelToken { get; set; }

        public ApplePublisher(
            IAppleMusicPlayerWatcher watcher,
            ISubscriber subscriber,
            ILogger<ApplePublisher> logger = null,
            CancellationToken token = default
        )
        {
            Watcher = watcher;
            Subscriber = subscriber;
            Logger = logger ?? NullLogger<ApplePublisher>.Instance;
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

            var payload = JsonSerializer.Serialize((AppleCurrentlyPlayingDTO)e,
                AppleJsonContext.Default.AppleCurrentlyPlayingDTO);

            Logger.LogTrace("Publishing current");

            // TODO: currently using spotify username for cache key, use db username
            var receivers =
                await Subscriber.PublishAsync(RedisChannel.Literal(Key.CurrentlyPlayingAppleMusic(e.Id)), payload);

            Logger.LogDebug("Published current, {receivers} receivers", receivers);
        }

        public void Subscribe(IWatcher watch = null)
        {
            var watcher = watch ?? Watcher ?? throw new ArgumentNullException("No watcher provided");

            if (watcher is IAppleMusicPlayerWatcher watcherCast)
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

            if (watcher is IAppleMusicPlayerWatcher watcherCast)
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