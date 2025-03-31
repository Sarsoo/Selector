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
using Selector.AppleMusic;
using Selector.AppleMusic.Watcher;
using Selector.Cache;
using Selector.CLI.Consumer;
using Selector.Spotify;
using Selector.Spotify.Consumer.Factory;
using Selector.Spotify.FactoryProvider;
using Selector.Spotify.Watcher;

namespace Selector.CLI
{
    class LocalWatcherService : IHostedService
    {
        private const string ConfigInstanceKey = "localconfig";

        private readonly ILogger<LocalWatcherService> Logger;
        private readonly RootOptions Config;
        private readonly ISpotifyWatcherFactory _spotifyWatcherFactory;
        private readonly IAppleMusicWatcherFactory _appleWatcherFactory;
        private readonly IWatcherCollectionFactory WatcherCollectionFactory;
        private readonly IRefreshTokenFactoryProvider SpotifyFactory;
        private readonly AppleMusicApiProvider _appleMusicApiProvider;
        private readonly IOptions<AppleMusicOptions> _appleMusicOptions;

        private readonly IServiceProvider ServiceProvider;

        private Dictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public LocalWatcherService(
            ISpotifyWatcherFactory spotifyWatcherFactory,
            IAppleMusicWatcherFactory appleWatcherFactory,
            IWatcherCollectionFactory watcherCollectionFactory,
            IRefreshTokenFactoryProvider spotifyFactory,
            AppleMusicApiProvider appleMusicApiProvider,
            IOptions<AppleMusicOptions> appleMusicOptions,
            IServiceProvider serviceProvider,
            ILogger<LocalWatcherService> logger,
            IOptions<RootOptions> config
        )
        {
            Logger = logger;
            Config = config.Value;

            _spotifyWatcherFactory = spotifyWatcherFactory;
            _appleWatcherFactory = appleWatcherFactory;
            WatcherCollectionFactory = watcherCollectionFactory;
            SpotifyFactory = spotifyFactory;
            _appleMusicApiProvider = appleMusicApiProvider;
            _appleMusicOptions = appleMusicOptions;

            ServiceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting local watcher service...");

            var watcherIndices = await InitInstances();

            Logger.LogInformation("Starting {count} affected watcher collection(s)...", watcherIndices.Count());
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

                if (!string.IsNullOrWhiteSpace(watcherOption.PlaylistUri))
                    logMsg.Append($" [{watcherOption.PlaylistUri}]");
                Logger.LogInformation(logMsg.ToString());

                var watcherCollectionIdx = watcherOption.WatcherCollection ?? ConfigInstanceKey;
                indices.Add(watcherCollectionIdx);

                if (!Watchers.ContainsKey(watcherCollectionIdx))
                    Watchers[watcherCollectionIdx] = WatcherCollectionFactory.Get();

                var watcherCollection = Watchers[watcherCollectionIdx];

                Logger.LogDebug("Getting Spotify factory");
                var spotifyFactory = await SpotifyFactory.GetFactory(watcherOption.RefreshKey);

                IWatcher watcher = null;
                switch (watcherOption.Type)
                {
                    case WatcherType.SpotifyPlayer:
                        watcher = await _spotifyWatcherFactory.Get<SpotifyPlayerWatcher>(spotifyFactory,
                            id: watcherOption.Name, pollPeriod: watcherOption.PollPeriod);
                        break;
                    case WatcherType.SpotifyPlaylist:
                        var playlistWatcher = await _spotifyWatcherFactory.Get<PlaylistWatcher>(spotifyFactory,
                            id: watcherOption.Name, pollPeriod: watcherOption.PollPeriod) as PlaylistWatcher;
                        playlistWatcher.config = new() { PlaylistId = watcherOption.PlaylistUri };

                        watcher = playlistWatcher;
                        break;
                    case WatcherType.AppleMusicPlayer:
                        var appleMusicWatcher = await _appleWatcherFactory.Get<AppleMusicPlayerWatcher>(
                            _appleMusicApiProvider, _appleMusicOptions.Value.Key, _appleMusicOptions.Value.TeamId,
                            _appleMusicOptions.Value.KeyId, watcherOption.AppleUserToken,
                            id: watcherOption.Name);

                        watcher = appleMusicWatcher;
                        break;
                }

                List<IConsumer> consumers = new();

                if (watcherOption.Consumers is not null)
                {
                    foreach (var consumer in watcherOption.Consumers)
                    {
                        switch (consumer)
                        {
                            case Consumers.AudioFeatures:
                                consumers.Add(await ServiceProvider.GetService<AudioFeatureInjectorFactory>()
                                    .Get(spotifyFactory));
                                break;

                            case Consumers.AudioFeaturesCache:
                                consumers.Add(await ServiceProvider.GetService<CachingAudioFeatureInjectorFactory>()
                                    .Get(spotifyFactory));
                                break;

                            case Consumers.CacheWriter:
                                if (watcher is ISpotifyPlayerWatcher or IPlaylistWatcher)
                                {
                                    consumers.Add(await ServiceProvider.GetService<CacheWriterFactory>().GetSpotify());
                                }
                                else
                                {
                                    consumers.Add(await ServiceProvider.GetService<CacheWriterFactory>().GetApple());
                                }

                                break;

                            case Consumers.Publisher:
                                if (watcher is ISpotifyPlayerWatcher or IPlaylistWatcher)
                                {
                                    consumers.Add(await ServiceProvider.GetService<PublisherFactory>().GetSpotify());
                                }
                                else
                                {
                                    consumers.Add(await ServiceProvider.GetService<PublisherFactory>().GetApple());
                                }

                                break;

                            case Consumers.PlayCounter:
                                if (!string.IsNullOrWhiteSpace(watcherOption.LastFmUsername))
                                {
                                    consumers.Add(await ServiceProvider.GetService<PlayCounterFactory>()
                                        .Get(creds: new() { Username = watcherOption.LastFmUsername }));
                                }
                                else
                                {
                                    Logger.LogError("No Last.fm username provided, skipping play counter");
                                }

                                break;

                            case Consumers.MappingPersister:
                                consumers.Add(await ServiceProvider.GetService<MappingPersisterFactory>().Get());
                                break;
                        }
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
                    Logger.LogInformation("Starting watcher collection [{index}]", index);
                    Watchers[index].Start();
                }
                catch (KeyNotFoundException)
                {
                    Logger.LogError("Unable to retrieve watcher collection [{index}] when starting", index);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Shutting down");

            foreach ((var key, var watcher) in Watchers)
            {
                Logger.LogInformation("Stopping watcher collection [{key}]", key);
                watcher.Stop();
            }

            return Task.CompletedTask;
        }
    }
}