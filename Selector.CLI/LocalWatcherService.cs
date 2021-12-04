using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Selector.Cache;

namespace Selector.CLI
{
    class LocalWatcherService : IHostedService
    {
        private const string ConfigInstanceKey = "localconfig";

        private readonly ILogger<LocalWatcherService> Logger;
        private readonly RootOptions Config;
        private readonly IWatcherFactory WatcherFactory;
        private readonly IWatcherCollectionFactory WatcherCollectionFactory;
        private readonly IRefreshTokenFactoryProvider SpotifyFactory;

        private readonly IServiceProvider ServiceProvider;

        private Dictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public LocalWatcherService(
            IWatcherFactory watcherFactory,
            IWatcherCollectionFactory watcherCollectionFactory,
            IRefreshTokenFactoryProvider spotifyFactory,

            IServiceProvider serviceProvider,

            ILogger<LocalWatcherService> logger,
            IOptions<RootOptions> config
        ) {
            Logger = logger;
            Config = config.Value;

            WatcherFactory = watcherFactory;
            WatcherCollectionFactory = watcherCollectionFactory;
            SpotifyFactory = spotifyFactory;

            ServiceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting local watcher service...");

            var watcherIndices = await InitInstances();

            Logger.LogInformation($"Starting {watcherIndices.Count()} affected watcher collection(s)...");
            StartWatcherCollections(watcherIndices);
        }

        private async Task<IEnumerable<string>> InitInstances()
        {
            var indices = new HashSet<string>();

            foreach (var watcherOption in Config.WatcherOptions.Instances)
            {
                var logMsg = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(watcherOption.Name))
                {
                    logMsg.Append($"Creating [{watcherOption.Name}] watcher [{watcherOption.Type}]");
                }
                else
                {
                    logMsg.Append($"Creating new [{watcherOption.Type}] watcher");
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
                        watcher = await WatcherFactory.Get<PlayerWatcher>(spotifyFactory, id: watcherOption.Name, pollPeriod: watcherOption.PollPeriod);
                        break;
                    case WatcherType.Playlist:
                        throw new NotImplementedException("Playlist watchers not implemented");
                        // break;
                }

                List<IConsumer> consumers = new();
                foreach(var consumer in watcherOption.Consumers)
                {
                    switch(consumer)
                    {
                        case Consumers.AudioFeatures:
                            consumers.Add(await ServiceProvider.GetService<AudioFeatureInjectorFactory>().Get(spotifyFactory));
                            break;

                        case Consumers.AudioFeaturesCache:
                            consumers.Add(await ServiceProvider.GetService<CachingAudioFeatureInjectorFactory>().Get(spotifyFactory));
                            break;

                        case Consumers.CacheWriter:
                            consumers.Add(await ServiceProvider.GetService<CacheWriterFactory>().Get());
                            break;

                        case Consumers.Publisher:
                            consumers.Add(await ServiceProvider.GetService<PublisherFactory>().Get());
                            break;

                        case Consumers.PlayCounter:
                            if(!string.IsNullOrWhiteSpace(watcherOption.LastFmUsername))
                            {
                                consumers.Add(await ServiceProvider.GetService<PlayCounterFactory>().Get(creds: new() { Username = watcherOption.LastFmUsername }));
                            }
                            else 
                            {
                                Logger.LogError("No Last.fm username provided, skipping play counter");
                            }
                            break;
                    }
                }

                watcherCollection.Add(watcher, consumers);
            }

            return indices;
        }

        private void StartWatcherCollections(IEnumerable<string> indices)
        {
            foreach (var index in indices)
            {
                try
                {
                    Logger.LogInformation($"Starting watcher collection [{index}]");
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
                Logger.LogInformation($"Stopping watcher collection [{key}]");
                watcher.Stop();
            }

            return Task.CompletedTask;
        }
    }
}
