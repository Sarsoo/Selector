using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SpotifyAPI.Web;

namespace Selector.Cache
{
    public class CacheUpdater : IConsumer
    {
        private readonly IPlayerWatcher Watcher;
        private readonly ILogger<CacheUpdater> Logger;

        public CancellationToken CancelToken { get; set; }

        public CacheUpdater(
            IPlayerWatcher watcher,
            ILogger<CacheUpdater> logger = null,
            CancellationToken token = default
        ){
            Watcher = watcher;
            Logger = logger ?? NullLogger<CacheUpdater>.Instance;
            CancelToken = token;
        }

        public void Callback(object sender, ListeningChangeEventArgs e)
        {
            if (e.Current is null) return;

            Task.Run(() => { return AsyncCallback(e); }, CancelToken);
        }

        public async Task AsyncCallback(ListeningChangeEventArgs e)
        {
            if (e.Current.Item is FullTrack track)
            {
                
            }
            else if (e.Current.Item is FullEpisode episode)
            {
                
            }
            else
            {
                Logger.LogError($"Unknown item pulled from API [{e.Current.Item}]");
            }
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
