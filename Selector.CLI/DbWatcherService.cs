using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Selector.Cache;
using Selector.Model;
using IF.Lastfm.Core.Api;
using StackExchange.Redis;

namespace Selector.CLI
{
    class DbWatcherService : IHostedService
    {
        private const int PollPeriod = 1000;

        private readonly ILogger<LocalWatcherService> Logger;
        private readonly ILoggerFactory LoggerFactory;
        private readonly IServiceProvider ServiceProvider;
        private readonly RootOptions Config;
        private readonly IWatcherFactory WatcherFactory;
        private readonly IWatcherCollectionFactory WatcherCollectionFactory;
        private readonly IRefreshTokenFactoryProvider SpotifyFactory;
        private readonly LastAuth LastAuth;
        
        private readonly IDatabaseAsync Cache;
        private readonly ISubscriber Subscriber; 

        private Dictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public DbWatcherService(
            IWatcherFactory watcherFactory,
            IWatcherCollectionFactory watcherCollectionFactory,
            IRefreshTokenFactoryProvider spotifyFactory,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            IOptions<RootOptions> config,
            LastAuth lastAuth = null,
            IDatabaseAsync cache = null,
            ISubscriber subscriber = null
        ) {
            Logger = loggerFactory.CreateLogger<LocalWatcherService>();
            LoggerFactory = loggerFactory;
            Config = config.Value;
            WatcherFactory = watcherFactory;
            WatcherCollectionFactory = watcherCollectionFactory;
            SpotifyFactory = spotifyFactory;
            LastAuth = lastAuth;
            ServiceProvider = serviceProvider;
            Cache = cache;
            Subscriber = subscriber;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting database watcher service...");

            var watcherIndices = await InitInstances();

            Logger.LogInformation($"Starting {watcherIndices.Count()} affected watcher collection(s)...");
            StartWatcherCollections(watcherIndices);
        }

        private async Task<IEnumerable<string>> InitInstances()
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

            var indices = new HashSet<string>();

            foreach (var dbWatcher in db.Watcher.Include(w => w.User))
            {
                Logger.LogInformation($"Creating new [{dbWatcher.Type}] watcher");

                var watcherCollectionIdx = dbWatcher.UserId;
                indices.Add(watcherCollectionIdx);

                if (!Watchers.ContainsKey(watcherCollectionIdx))
                    Watchers[watcherCollectionIdx] = WatcherCollectionFactory.Get();

                var watcherCollection = Watchers[watcherCollectionIdx];

                Logger.LogDebug("Getting Spotify factory");
                var spotifyFactory = await SpotifyFactory.GetFactory(dbWatcher.User.SpotifyRefreshToken);

                IWatcher watcher = null;
                List<IConsumer> consumers = new();

                switch (dbWatcher.Type)
                {
                    case WatcherType.Player:
                        watcher = await WatcherFactory.Get<PlayerWatcher>(spotifyFactory, id: dbWatcher.UserId, pollPeriod: PollPeriod);

                        var featureInjector = new AudioFeatureInjectorFactory(LoggerFactory);
                        consumers.Add(await featureInjector.Get(spotifyFactory));

                        var featureInjectorCache = new CachingAudioFeatureInjectorFactory(LoggerFactory, Cache);
                        consumers.Add(await featureInjectorCache.Get(spotifyFactory));

                        var cacheWriter = new CacheWriterFactory(Cache, LoggerFactory);
                        consumers.Add(await cacheWriter.Get());

                        var pub = new PublisherFactory(Subscriber, LoggerFactory);
                        consumers.Add(await pub.Get());

                        if (!string.IsNullOrWhiteSpace(dbWatcher.User.LastFmUsername))
                        {
                            if (LastAuth is null) throw new ArgumentNullException("No Last Auth Injected");

                            var client = new LastfmClient(LastAuth);

                            var playCount = new PlayCounterFactory(LoggerFactory, client: client, creds: new() { Username = dbWatcher.User.LastFmUsername });
                            consumers.Add(await playCount.Get());
                        }
                        else
                        {
                            Logger.LogDebug($"[{dbWatcher.User.UserName}] No Last.fm username, skipping play counter");
                        }

                        break;

                    case WatcherType.Playlist:
                        throw new NotImplementedException("Playlist watchers not implemented");
                        // break;
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
