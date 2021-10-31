using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Selector.Cache;
using StackExchange.Redis;

namespace Selector.CLI
{
    class WatcherService : IHostedService
    {
        private const string ConfigInstanceKey = "localconfig";

        private readonly ILogger<WatcherService> Logger;
        private readonly ILoggerFactory LoggerFactory;
        private readonly RootOptions Config;
        private readonly IWatcherFactory WatcherFactory;
        private readonly IWatcherCollectionFactory WatcherCollectionFactory;
        private readonly IRefreshTokenFactoryProvider SpotifyFactory;
        private readonly LastAuth LastAuth;
        
        private readonly IDatabaseAsync Cache; 
        private readonly ISubscriber Subscriber; 

        private Dictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public WatcherService(
            IWatcherFactory watcherFactory,
            IWatcherCollectionFactory watcherCollectionFactory,
            IRefreshTokenFactoryProvider spotifyFactory,
            ILoggerFactory loggerFactory,
            IOptions<RootOptions> config,
            LastAuth lastAuth = null,
            IDatabaseAsync cache = null,
            ISubscriber subscriber = null
        ) {
            Logger = loggerFactory.CreateLogger<WatcherService>();
            LoggerFactory = loggerFactory;
            Config = config.Value;
            WatcherFactory = watcherFactory;
            WatcherCollectionFactory = watcherCollectionFactory;
            SpotifyFactory = spotifyFactory;
            LastAuth = lastAuth;
            Cache = cache;
            Subscriber = subscriber;

            SpotifyFactory.Initialise(Config.ClientId, Config.ClientSecret);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting watcher service...");

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
                        watcher = await WatcherFactory.Get<PlayerWatcher>(spotifyFactory, watcherOption.PollPeriod);
                        break;
                    case WatcherType.Playlist:
                        throw new NotImplementedException("Playlist watchers not implemented");
                        break;
                }

                List<IConsumer> consumers = new();
                foreach(var consumer in watcherOption.Consumers)
                {
                    switch(consumer)
                    {
                        case Consumers.AudioFeatures:
                            var featureInjector = new AudioFeatureInjectorFactory(LoggerFactory);
                            consumers.Add(await featureInjector.Get(spotifyFactory));
                            break;

                        case Consumers.AudioFeaturesCache:
                            var featureInjectorCache = new CachingAudioFeatureInjectorFactory(LoggerFactory, Cache);
                            consumers.Add(await featureInjectorCache.Get(spotifyFactory));
                            break;

                        case Consumers.CacheWriter:
                            var cacheWriter = new CacheWriterFactory(Cache, LoggerFactory);
                            consumers.Add(await cacheWriter.Get());
                            break;

                        case Consumers.Publisher:
                            var pub = new PublisherFactory(Subscriber, LoggerFactory);
                            consumers.Add(await pub.Get());
                            break;

                        case Consumers.PlayCounter:
                            if(!string.IsNullOrWhiteSpace(watcherOption.LastFmUsername))
                            {
                                if(LastAuth is null) throw new ArgumentNullException("No Last Auth Injected");
                                
                                var client = new LastfmClient(LastAuth);

                                var playCount = new PlayCounterFactory(LoggerFactory, client: client, creds: new(){ Username = watcherOption.LastFmUsername });
                                consumers.Add(await playCount.Get());
                            }
                            else 
                            {
                                Logger.LogError("No Last.fm usernmae provided, skipping play counter");
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
