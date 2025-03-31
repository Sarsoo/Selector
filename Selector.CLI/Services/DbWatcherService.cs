using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Selector.AppleMusic;
using Selector.AppleMusic.Watcher;
using Selector.Cache;
using Selector.CLI.Consumer;
using Selector.Events;
using Selector.Model;
using Selector.Model.Extensions;
using Selector.Spotify;
using Selector.Spotify.Consumer;
using Selector.Spotify.Consumer.Factory;
using Selector.Spotify.FactoryProvider;
using Selector.Spotify.Watcher;

namespace Selector.CLI
{
    class DbWatcherService : IHostedService
    {
        private const int PollPeriod = 1000;

        private readonly ILogger<DbWatcherService> Logger;
        private readonly IOptions<AppleMusicOptions> _appleMusicOptions;
        private readonly IServiceProvider ServiceProvider;
        private readonly UserEventBus UserEventBus;

        private readonly ISpotifyWatcherFactory _spotifyWatcherFactory;
        private readonly IAppleMusicWatcherFactory _appleWatcherFactory;
        private readonly IWatcherCollectionFactory WatcherCollectionFactory;
        private readonly IRefreshTokenFactoryProvider SpotifyFactory;
        private readonly AppleMusicApiProvider _appleMusicProvider;

        private readonly IAudioFeatureInjectorFactory AudioFeatureInjectorFactory;
        private readonly IPlayCounterFactory PlayCounterFactory;

        private readonly IUserEventFirerFactory UserEventFirerFactory;

        private readonly IPublisherFactory PublisherFactory;
        private readonly ICacheWriterFactory CacheWriterFactory;

        private readonly IMappingPersisterFactory MappingPersisterFactory;

        private ConcurrentDictionary<string, IWatcherCollection> Watchers { get; set; } = new();

        public DbWatcherService(
            ISpotifyWatcherFactory spotifyWatcherFactory,
            IAppleMusicWatcherFactory appleWatcherFactory,
            IWatcherCollectionFactory watcherCollectionFactory,
            IRefreshTokenFactoryProvider spotifyFactory,
            AppleMusicApiProvider appleMusicProvider,
            IAudioFeatureInjectorFactory audioFeatureInjectorFactory,
            IPlayCounterFactory playCounterFactory,
            UserEventBus userEventBus,
            ILogger<DbWatcherService> logger,
            IOptions<AppleMusicOptions> appleMusicOptions,
            IServiceProvider serviceProvider,
            IPublisherFactory publisherFactory = null,
            ICacheWriterFactory cacheWriterFactory = null,
            IMappingPersisterFactory mappingPersisterFactory = null,
            IUserEventFirerFactory userEventFirerFactory = null
        )
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
            UserEventBus = userEventBus;

            _spotifyWatcherFactory = spotifyWatcherFactory;
            _appleWatcherFactory = appleWatcherFactory;
            _appleMusicOptions = appleMusicOptions;
            WatcherCollectionFactory = watcherCollectionFactory;
            SpotifyFactory = spotifyFactory;
            _appleMusicProvider = appleMusicProvider;

            AudioFeatureInjectorFactory = audioFeatureInjectorFactory;
            PlayCounterFactory = playCounterFactory;

            UserEventFirerFactory = userEventFirerFactory;

            PublisherFactory = publisherFactory;
            CacheWriterFactory = cacheWriterFactory;

            MappingPersisterFactory = mappingPersisterFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Starting database watcher service...");

            var watcherIndices = await InitInstances();
            AttachEventBus();

            Logger.LogInformation("Starting {count} affected watcher collection(s)...", watcherIndices.Count());
            StartWatcherCollections(watcherIndices);
        }

        private async Task<IEnumerable<string>> InitInstances()
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

            var indices = new HashSet<string>();

            foreach (var dbWatcher in db.Watcher
                         .Include(w => w.User)
                         .Where(w =>
                             ((w.Type == WatcherType.SpotifyPlayer || w.Type == WatcherType.SpotifyPlaylist) &&
                              !string.IsNullOrWhiteSpace(w.User.SpotifyRefreshToken)) ||
                             (w.Type == WatcherType.AppleMusicPlayer && w.User.AppleMusicLinked)
                         ))
            {
                using var logScope = Logger.BeginScope(new Dictionary<string, string>
                {
                    { "username", dbWatcher.User.UserName }
                });

                var watcherCollectionIdx = dbWatcher.UserId;
                indices.Add(watcherCollectionIdx);

                await InitInstance(dbWatcher);
            }

            return indices;
        }

        private async Task<IWatcherContext> InitInstance(Watcher dbWatcher)
        {
            Logger.LogInformation("Creating new [{type}] watcher", dbWatcher.Type);

            var watcherCollectionIdx = dbWatcher.UserId;

            if (!Watchers.ContainsKey(watcherCollectionIdx))
                Watchers[watcherCollectionIdx] = WatcherCollectionFactory.Get();

            var watcherCollection = Watchers[watcherCollectionIdx];

            IWatcher watcher = null;
            List<IConsumer> consumers = new();

            switch (dbWatcher.Type)
            {
                case WatcherType.SpotifyPlayer:
                    Logger.LogDebug("Getting Spotify factory");
                    var spotifyFactory = await SpotifyFactory.GetFactory(dbWatcher.User.SpotifyRefreshToken);

                    watcher = await _spotifyWatcherFactory.Get<SpotifyPlayerWatcher>(spotifyFactory,
                        id: dbWatcher.UserId, pollPeriod: PollPeriod);

                    consumers.Add(await AudioFeatureInjectorFactory.Get(spotifyFactory));
                    if (CacheWriterFactory is not null) consumers.Add(await CacheWriterFactory.GetSpotify());
                    if (PublisherFactory is not null) consumers.Add(await PublisherFactory.GetSpotify());

                    if (MappingPersisterFactory is not null && !Magic.Dummy)
                        consumers.Add(await MappingPersisterFactory.Get());

                    if (UserEventFirerFactory is not null) consumers.Add(await UserEventFirerFactory.Get());

                    if (dbWatcher.User.LastFmConnected())
                    {
                        consumers.Add(await PlayCounterFactory.Get(creds: new()
                            { Username = dbWatcher.User.LastFmUsername }));
                    }
                    else
                    {
                        Logger.LogDebug("[{username}] No Last.fm username, skipping play counter",
                            dbWatcher.User.UserName);
                    }

                    break;

                case WatcherType.SpotifyPlaylist:
                    throw new NotImplementedException("Playlist watchers not implemented");
                    break;
                case WatcherType.AppleMusicPlayer:
                    watcher = await _appleWatcherFactory.Get<AppleMusicPlayerWatcher>(_appleMusicProvider,
                        _appleMusicOptions.Value.Key, _appleMusicOptions.Value.TeamId, _appleMusicOptions.Value.KeyId,
                        dbWatcher.User.AppleMusicKey, id: dbWatcher.UserId);

                    if (CacheWriterFactory is not null) consumers.Add(await CacheWriterFactory.GetApple());
                    if (PublisherFactory is not null) consumers.Add(await PublisherFactory.GetApple());

                    break;
            }

            return watcherCollection.Add(watcher, consumers);
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

            DetachEventBus();

            return Task.CompletedTask;
        }

        private void AttachEventBus()
        {
            UserEventBus.SpotifyLinkChange += SpotifyChangeCallback;
            UserEventBus.AppleLinkChange += AppleMusicChangeCallback;
            UserEventBus.LastfmCredChange += LastfmChangeCallback;
        }

        private void DetachEventBus()
        {
            UserEventBus.SpotifyLinkChange -= SpotifyChangeCallback;
            UserEventBus.AppleLinkChange -= AppleMusicChangeCallback;
            UserEventBus.LastfmCredChange -= LastfmChangeCallback;
        }

        public async void SpotifyChangeCallback(object sender, SpotifyLinkChange change)
        {
            if (Watchers.ContainsKey(change.UserId))
            {
                Logger.LogDebug("Setting new Spotify link state for [{username}], [{}]", change.UserId,
                    change.NewLinkState);

                var watcherCollection = Watchers[change.UserId];

                if (change.NewLinkState)
                {
                    watcherCollection.Start();
                }
                else
                {
                    watcherCollection.Stop();
                }
            }
            else
            {
                using var scope = ServiceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var watcherEnum = db.Watcher
                    .Include(w => w.User)
                    .Where(w => w.UserId == change.UserId);

                foreach (var dbWatcher in watcherEnum)
                {
                    var context = await InitInstance(dbWatcher);
                }

                Watchers[change.UserId].Start();

                Logger.LogDebug("Started {} watchers for [{username}]", watcherEnum.Count(), change.UserId);
            }
        }

        public async void AppleMusicChangeCallback(object sender, AppleMusicLinkChange change)
        {
            if (Watchers.ContainsKey(change.UserId))
            {
                Logger.LogDebug("Setting new Apple Music link state for [{username}], [{}]", change.UserId,
                    change.NewLinkState);

                var watcherCollection = Watchers[change.UserId];

                if (change.NewLinkState)
                {
                    watcherCollection.Start();
                }
                else
                {
                    watcherCollection.Stop();
                }
            }
            else
            {
                using var scope = ServiceProvider.CreateScope();
                var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var watcherEnum = db.Watcher
                    .Include(w => w.User)
                    .Where(w => w.UserId == change.UserId);

                foreach (var dbWatcher in watcherEnum)
                {
                    var context = await InitInstance(dbWatcher);
                }

                Watchers[change.UserId].Start();

                Logger.LogDebug("Started {} watchers for [{username}]", watcherEnum.Count(), change.UserId);
            }
        }

        public void LastfmChangeCallback(object sender, LastfmChange change)
        {
            if (Watchers.ContainsKey(change.UserId))
            {
                Logger.LogDebug("Setting new username for [{}], [{}]", change.UserId, change.NewUsername);

                var watcherCollection = Watchers[change.UserId];

                foreach (var watcher in watcherCollection.Consumers)
                {
                    if (watcher is PlayCounter counter)
                    {
                        counter.Credentials.Username = change.NewUsername;
                    }
                }
            }
            else
            {
                Logger.LogDebug("No watchers running for [{username}], skipping Spotify event", change.UserId);
            }
        }
    }
}