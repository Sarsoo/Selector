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
    public class Publisher : IPlayerConsumer
    {
        private readonly IPlayerWatcher Watcher;
        private readonly ISubscriber Subscriber;
        private readonly ILogger<Publisher> Logger;

        public CancellationToken CancelToken { get; set; }

        public Publisher(
            IPlayerWatcher watcher,
            ISubscriber subscriber,
            ILogger<Publisher> logger = null,
            CancellationToken token = default
        ){
            Watcher = watcher;
            Subscriber = subscriber;
            Logger = logger ?? NullLogger<Publisher>.Instance;
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
            using var scope = Logger.GetListeningEventArgsScope(e);

            var payload = JsonSerializer.Serialize((CurrentlyPlayingDTO) e, JsonContext.Default.CurrentlyPlayingDTO);

            Logger.LogTrace("Publishing current");
            
            // TODO: currently using spotify username for cache key, use db username
            var receivers = await Subscriber.PublishAsync(Key.CurrentlyPlaying(e.Id), payload);

            Logger.LogDebug("Published current, {receivers} receivers", receivers);
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
