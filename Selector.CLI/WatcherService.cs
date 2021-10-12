using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Selector.CLI
{
    class WatcherService : IHostedService
    {
        private const string ConfigInstanceKey = "localconfig";

        private readonly ILogger<WatcherService> Logger;
        private readonly RootOptions Config;
        private readonly IWatcherFactory WatcherFactory;
        private readonly IWatcherCollectionFactory WatcherCollectionFactory;
        private readonly IRefreshTokenFactoryProvider SpotifyFactory;

        private Dictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public WatcherService(
            IWatcherFactory watcherFactory,
            IWatcherCollectionFactory watcherCollectionFactory,
            IRefreshTokenFactoryProvider spotifyFactory,
            ILogger<WatcherService> logger,
            IOptions<RootOptions> config
        ) {
            Logger = logger;
            Config = config.Value;
            WatcherFactory = watcherFactory;
            WatcherCollectionFactory = watcherCollectionFactory;
            SpotifyFactory = spotifyFactory;

            SpotifyFactory.Initialise(Config.ClientId, Config.ClientSecret);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting up");

            Logger.LogInformation("Loading config instances...");
            var watcherIndices = await InitialiseConfigInstances();

            Logger.LogInformation($"Starting {watcherIndices.Count()} affected watcher collection(s)...");
            StartWatcherCollections(watcherIndices);
        }

        private async Task<IEnumerable<string>> InitialiseConfigInstances()
        {
            var indices = new HashSet<string>();

            foreach (var watcherOption in Config.WatcherOptions.Instances)
            {
                var logMsg = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(watcherOption.Name))
                {
                    logMsg.Append($"Creating {watcherOption.Name} watcher [{watcherOption.Type}]");
                }
                else
                {
                    logMsg.Append($"Creating new {watcherOption.Type} watcher");
                }

                if (!string.IsNullOrWhiteSpace(watcherOption.PlaylistUri)) logMsg.Append($" [{ watcherOption.PlaylistUri}]");
                Logger.LogInformation(logMsg.ToString());

                var watcherCollectionIdx = watcherOption.WatcherCollection ?? ConfigInstanceKey;
                indices.Add(watcherCollectionIdx);

                if (!Watchers.ContainsKey(watcherCollectionIdx))
                    Watchers[watcherCollectionIdx] = WatcherCollectionFactory.Get();

                var watcherCollection = Watchers[watcherCollectionIdx];

                Logger.LogDebug("Getting Spotify factory");
                var spotifyFactory = await SpotifyFactory.GetFactory(watcherOption.RefreshKey);

                IWatcher watcher = null;
                switch(watcherOption.Type)
                {
                    case WatcherType.Player:
                        watcher = await WatcherFactory.Get<PlayerWatcher>(spotifyFactory, watcherOption.PollPeriod);
                        break;
                    case WatcherType.Playlist:
                        throw new NotImplementedException("Playlist watchers not implemented");
                        break;
                }

                watcherCollection.Add(watcher);
            }

            return indices;
        }

        private void StartWatcherCollections(IEnumerable<string> indices)
        {
            foreach (var index in indices)
            {
                try
                {
                    Watchers[index].Start();
                }
                catch (KeyNotFoundException)
                {
                    Logger.LogError($"Unable to retrieve watcher collection [{index}] when starting");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Shutting down");

            foreach((var key, var watcher) in Watchers)
            {
                Logger.LogInformation($"Stopping watcher collection: {key}");
                watcher.Stop();
            }

            return Task.CompletedTask;
        }
    }
}
